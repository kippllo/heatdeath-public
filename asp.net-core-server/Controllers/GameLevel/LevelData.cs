using System;
using System.Threading;
using Keybored.BackendServer.GameEngine;
using Keybored.BackendServer.Logging;
using Keybored.BackendServer.Network;
using Keybored.BackendServer.Settings;
using Keybored.BackendServer.Files;
using static Keybored.BackendServer.Network.StringFormat;


namespace Keybored.BackendServer.GameLevel {

    public abstract class LevelData {
		
		public enum LvlType {Lobby, GameLvl, Ended}; //These are different from the "game" state because the game state is used on the backend, but this level state is sent to the frontend!
        public LvlType lvlType;
        public MapData mapData;
		protected Game parent;

		private long lastLvlTime;

		private float lvlDeltaTime { //Dt is a float in seconds.
			get {
				return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastLvlTime)/1000.0f; //The 1000's var type matters!
			}
		}

		//Base Constructor
		public LevelData(Game parentIN) {
            parent = parentIN;
			lastLvlTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		private void tickLvlClock() {
			lastLvlTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		protected abstract void updateLvl(float dt);

		//This is the public update called by the game host, "updateLvl" is an override class that will be defined for each subclasses. "updateLvl" is automatically passed delta time.
		public void update() {
			float dt = lvlDeltaTime; //Grab a copy of the current DT.
			tickLvlClock(); //Tick the timer after the dt is recorded. (might want to move to the end of the function...)
			
			updateLvl(dt);
		}

		public async void setMapData(){
			mapData = await FileData.readRandomMap();
		}
	}


    public class syncLobbyData: LevelData {

        public float lobbyStartTimer = 0.0f;
        public float lobbyStartTimerMax = ServerSettings.lobbyWaitTime; //When this is reached or when the game has the maximum player count.
		private float lvlUpdateTimer = 0;
		private float lvlUpdateTimerMax = 1.0f;
		private int playersLeft;

		public syncLobbyData(Game parentIN) : base(parentIN) {
			lvlType = LvlType.Lobby;
			setMapData(); //async void to read map data from a regular constructor. Help: https://stackoverflow.com/questions/8145479/can-constructors-be-async
		}

		protected override void updateLvl(float dt) {
			lvlUpdateTimer += dt;
			if(lvlUpdateTimer >= lvlUpdateTimerMax) { //These three lines are not used, but are for template purposes.
				lvlUpdateTimer -= lvlUpdateTimerMax;
			}

			connection[] alivePlayers = parent.alivePlayers;
			playersLeft = alivePlayers.Length;

            if(parent.isFull){
                //Start game...
                parent.startGame();
            }

            lobbyStartTimer += dt;
            if(lobbyStartTimer >= lobbyStartTimerMax) {
                //Start the game and spawn bots if there are not enough players.

                //Spawn bots. Keep this is the level class because the level's update will update the bots also.
				if(ServerSettings.FillWithAI && !parent.isFull) {
					parent.fillEmptyWithBots();
				}

                //Start game...
                parent.startGame();
            }
		}

		override public string ToString() {
			string str = "{";
            str += q0 + "lvlType" + q0 + ":" + (int)lvlType; //Must use an int parse so the JSON convert will work correctly.
            str += "," + q0 + "JSONLvl" + q0 + ":" + q0 + "{";
			str += q1 + "lobbyStartTimer" + q1 + ":" + (lobbyStartTimerMax-lobbyStartTimer);
			str += "," + q1 + "playersLeft" + q1 + ":" + playersLeft;
			str += "," + q1 + "playersMax" + q1 + ":" + parent.connectionsLimit;
            str +=  "}" + q0;
            str += "}";
			return str;
		}
	}


	public class syncLevelData: LevelData {
		public Vector3Thin safeCenter; //The center of the safety circle.
		public float safeDist; //The diameter of the safety circle.
		public Vector3Thin NextSafeCenter; //The same vars for the next circle.
		public float NextSafeDist;
		
		private float _circleUpdateTimer = 0;
		public float circleUpdateTimer {
			get {return _circleUpdateTimer;} //public readonly...
		}

		private float _circleUpdateTimerMax = ServerSettings.zoneTime;
		public float circleUpdateTimerMax {
			get {return _circleUpdateTimerMax;}
		}

		private int playersLeft;

		private float botsTimer = 0;
		private float botsTimerMax = 0.0167f; //In seconds. "0.0167" is about 60FPS.
		private long lastBotUpdateTime;
		private Thread botUpdateThread;
		//Thread listen = new Thread(new ThreadStart(udpSyncListenLoop));
        //listen.Start();

		public syncLevelData(Game parentIN) : base(parentIN) { //Calling base/inherited constructor help: https://stackoverflow.com/questions/12051/calling-the-base-constructor-in-c-sharp
			lvlType = LvlType.GameLvl;

			//This will have problems if I don't pass the map from the lobby!
			safeCenter = new Vector3Thin(0,0,0); //Start with the whole map in saftey.
			safeDist = 9999; //The map is not read yet, so start with a huge diameter that will cover the whole map!

			playersLeft = parent.connectionsLimit; //Set the limit to start as the max number of allow players...

			setMapData(); //Load a random map different from the lobby.

			lastBotUpdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		public syncLevelData(Game parentIN, MapData lobbyMap) : base(parentIN) { //Calling base/inherited constructor help: https://stackoverflow.com/questions/12051/calling-the-base-constructor-in-c-sharp
			lvlType = LvlType.GameLvl;

			playersLeft = parent.connectionsLimit;

			mapData = lobbyMap; //Set map data as a copy of the lobby's.

			//These need to be set after the map is set!
			safeCenter = new Vector3Thin(0,0,0); //Start with the whole map in saftey.
			safeDist = (mapData.height >= mapData.width) ? mapData.height : mapData.width;
			safeDist *= 1.5f; //Make sure even the corners are in the safety zone at first!

			NextSafeCenter = new Vector3Thin(safeCenter.x, safeCenter.y, safeCenter.z); //For now, these can just start with the same value!
			NextSafeDist = safeDist;
			findNextCircle(); //Maybe change later, but for now call this here to skip the double-same-zone at the beginning of the game.
			//Note: calling the above will make a new problem: now the zone timeMax will start at 75% of what it is set to in "setting.json".

			lastBotUpdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		}

		protected override void updateLvl(float dt) {
			_circleUpdateTimer += dt;
			if(_circleUpdateTimer >= _circleUpdateTimerMax) {
				_circleUpdateTimer -= _circleUpdateTimerMax;

				//Update the level variables.
				findNextCircle();
			}

			
			//Update bots
			botsTimer += dt;
			if(botsTimer >= botsTimerMax){
				//Only start a new "botUpdate" thread if the old has been completed...
				if(botUpdateThread == null || !botUpdateThread.IsAlive){
					//Find the correct dt since the last bot update call.
					float botDT = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastBotUpdateTime)/1000.0f;
					lastBotUpdateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

					//Start a new thread to update the bots...
					botUpdateThread = new Thread(()=>{
						parent.updateBots(botDT);
					});
					botUpdateThread.Start();
					botsTimer -= botsTimerMax;
				}
			}

			
			connection[] alivePlayers = parent.alivePlayers;
			playersLeft = alivePlayers.Length; //Update the alive player count.
			
			if(alivePlayers.Length <= 1){ //end the game if there is only 1 or less players alive...
				string strLog = "Game Ended!";
				strLog += "\nEnd Time: " + DateTimeOffset.Now.ToString("G");
				strLog += "\ngameID: " + parent.gameID;

				if(alivePlayers.Length == 1){ //If a player won the game log his username...
					strLog += "\nWinner: " + alivePlayers[0].playerObject.username;
					strLog += "\nConnection IP: " + alivePlayers[0].ipHash.ip;
				}

				FileLogger.logToFile(strLog);

				parent.purge(100); //Clean up the parent game, but wait a bit to set it as ended so that all players can see who won!
			}
		}

		private void findNextCircle(){
			safeCenter = new Vector3Thin(NextSafeCenter.x, NextSafeCenter.y, NextSafeCenter.z); //Set these based on the next values that are already found.
			safeDist = NextSafeDist;
			
			//Find new "Next" values.
			NextSafeDist = NextSafeDist/2; //Cut the saftey diameter in half, so the below will not make the new center outside of the old circle!

			NextSafeCenter.x = GameMath.randomRange(NextSafeCenter.x-NextSafeDist, NextSafeCenter.x+NextSafeDist); //Move the center to a random place within half its diameter. Half is so the circle will not move outside of the limits of the course.
			NextSafeCenter.y = GameMath.randomRange(NextSafeCenter.y-NextSafeDist, NextSafeCenter.y+NextSafeDist);
			NextSafeCenter.z = 0; //Z always stays at zero for this game!
			
			_circleUpdateTimerMax *= ServerSettings.zoneTimeScale; //Reduce the max timer...
		}

		override public string ToString() {
            string str = "{";
            str += q0 + "lvlType" + q0 + ":" + (int)lvlType;
            str += "," + q0 + "JSONLvl" + q0 + ":" + q0 + "{";
			str += q1 + "playersLeft" + q1 + ":" + playersLeft;
			str += "," + q1 + "playersMax" + q1 + ":" + parent.connectionsLimit;
			str += "," + q1 + "safeCenter" + q1 + ":" + safeCenter.toJSON(q1);
			str += "," + q1 + "safeDist" + q1 + ":" + safeDist;
			str += "," + q1 + "circleUpdateTimer" + q1 + ":" + (_circleUpdateTimerMax-_circleUpdateTimer); //Send the timer to the player as a count down.
			str += "," + q1 + "NextSafeCenter" + q1 + ":" + NextSafeCenter.toJSON(q1);
			str += "," + q1 + "NextSafeDist" + q1 + ":" + NextSafeDist;
            str +=  "}" + q0;
            str += "}";
			return str;
		}
	}


	public class syncEndedData: LevelData {

		public string winner;

		public syncEndedData(Game parentIN, string winnerIN) : base(parentIN) {
			lvlType = LvlType.Ended;
			winner = winnerIN;
		}

		//Just a blank update is fine...
		protected override void updateLvl(float dt) {
		}

		override public string ToString() {
			string str = "{";
            str += q0 + "lvlType" + q0 + ":" + (int)lvlType;
            str += "," + q0 + "JSONLvl" + q0 + ":" + q0 + "{";
			str += q1 + "winner" + q1 + ":" + q1 + winner + q1;
            str +=  "}" + q0;
            str += "}";
			return str;
		}
	}



    //This class used on the client side to make the "syncLevelData.ToString()" and "syncLevelData.syncLobbyData()"
    public class LevelDataClient{
        public LevelData.LvlType lvlType;
        public string JSONLvl;
        
        public LevelDataClient(){
            lvlType = LevelData.LvlType.Lobby;
            JSONLvl = "";
        }
    }

}