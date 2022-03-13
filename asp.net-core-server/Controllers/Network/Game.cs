using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System.Net;

using Keybored.BackendServer.AI;
using Keybored.BackendServer.GameEngine;
using Keybored.BackendServer.GameLevel;
using Keybored.BackendServer.Logging;
using Keybored.BackendServer.Settings;
using static Keybored.BackendServer.Network.StringFormat;

namespace Keybored.BackendServer.Network {

    public class Game {
		
		public Master master;
		public enum GameState {Lobby, Running, Ended};

		public int gameID;
		public GameState gameStatus;
		public List<connection> connections;
		
		// Bot Vars
		public List<PlayerBot> bots;
		public List<bulletObj> bullets; //Can check who a bullet belongs to by using "bulletObj.objID" // This will later hold all player bullets too!

		
		private int _connectionsLimit; //public readonly, but private can edit.
		public int connectionsLimit{
			get{
				return _connectionsLimit;
			}
		}
		
		public LevelData lvl;

		public bool isFull {
			get{
				bool allConnectionsActive = (findInactiveConnection() == -1); //"findInactiveConnection" will return "-1" if there are no inactive connections.
				bool maxConnectionsReached = (connections.Count >= connectionsLimit+1); //"+1" because the first connection is a null dummy.
				return (maxConnectionsReached && allConnectionsActive);
			}
		}

		public bool inLobby {
			get{
				return (gameStatus == GameState.Lobby);
			}
		}

		public bool isRunning {
			get{
				return (gameStatus == GameState.Running);
			}
		}

		public bool isEnded {
			get{
				bool gameRunningWithNoPlayers = (alivePlayers.Length == 0 && isRunning);
				bool gameEnded = (gameStatus == GameState.Ended);
				return (gameEnded || gameRunningWithNoPlayers);
			}
		}

		public connection[] alivePlayers{
			get {
				int playerArrLen = 0;
				int connCount = connections.Count;
				for(int i=1; i < connCount; i++){ //"i=1" to skip dummy.
					if(connections[i].isPlayerAlive){ //"connection.isPlayerAlive" takes "connection.isActive" into account.
						playerArrLen++;
					}
				}

				connection[] playerArr = new connection[playerArrLen];
				int currInd = 0;
				for(int i=1; i < connCount; i++){
					if(connections[i].isPlayerAlive){
						playerArr[currInd] = connections[i];
						currInd++;
					}
				}
				
				return playerArr;
			}
		}
		
		public Game(Master masterIN, int gameIDIN) {
			master = masterIN;
			
			gameID = gameIDIN;
			gameStatus = GameState.Lobby;

			_connectionsLimit = ServerSettings.connectionsLimit;
			connections = new List<connection>();
			connections.Capacity = _connectionsLimit;
			connections.Add(new connection(this,0,"")); //I want the connection IDs to start at "1" not "0", so make the first index a null value.
			
			lvl = new syncLobbyData(this);
			bots = new List<PlayerBot>();
			bullets = new List<bulletObj>();
			bullets.Capacity = _connectionsLimit*500; //Guess that eaceh player will have up to 500 bullets at a time...
		}
		
		public int addConnection(string connectionIP) {
			//Check if there is an inactive connection in the lobby, if there is reset it and return its ID to the new player trying to connect.
			int inactiveID = findInactiveConnection();
			if(inactiveID != -1){
				connections[inactiveID] = new connection(this, inactiveID, connectionIP);
				return inactiveID;
			} else { //If there are no inactive connections in the lobby, make a new connection object for the new player.
				connection connect = new connection(this, connections.Count, connectionIP);
				this.connections.Add(connect);
				return connect.index; //Return this so it can be sent to client.
			}
		}

		// Add a bot, first fill any inactive syncID's, then add new syncID's.
		public int addConnectionBot() {
			int inactiveID = findInactiveConnection();
			PlayerBot newBot;

			if(inactiveID != -1){
				newBot = new PlayerBot(this, inactiveID, lvl.mapData);
				connections[inactiveID] = newBot;
			} else {
				newBot = new PlayerBot(this, connections.Count, lvl.mapData);
				this.connections.Add(newBot);
			}

			bots.Add(newBot); // Also, add the bot to a "bots" list so we can update their movement.
			return newBot.index;
		}

		// This function adds as many bots that are needed to completely fill up the round...
		public void fillEmptyWithBots() {
			while(!isFull){
				addConnectionBot();
			}
		}

		public void updateBots(float dt){
			// Since this function is called on a separate thread it needs its own try-catch...
			try{
				int len = bots.Count;
				for(int i=0; i<len; i++){ //Note: there are no dummies in the "bots" list.
					PlayerBot bot = bots[i];
					bot.updateBot(dt);
				}

				// Update all bot bullets
				int bulletlen = bullets.Count;
				for(int b=bulletlen-1; b>-1; b--){
					bulletObj pew = bullets[b];
					pew.stepBullet(dt);
					// No need to transform pew.pos because "map.genGrid" already takes this into account!
					//I need to translate the bullet's position to account for negative values!
					Vector3Thin transPos = lvl.mapData.convertToGridPos(pew.pos);
					bool outsideOfMap = lvl.mapData.checkOutsideGrid(transPos);
					bool wallCollision = (outsideOfMap) ? true : lvl.mapData.grid[(int)transPos.x, (int)transPos.y]; //Don't even check for wall collision if the bullet is outside of the map. This will also stop the "index out of range error!"
					if(wallCollision || outsideOfMap){
						bullets.RemoveAt(b);
					}
				}

			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
			}
		}

		//Right now this function only returns bot bullets, but that will changes once player's clients no longer update their own bullets...
		public List<bulletObj> returnBulletsInView(connection clientPlayer){
			List<bulletObj> rtrnList = new List<bulletObj>();
			rtrnList.Capacity = bullets.Count/_connectionsLimit;
			
			Vector3Thin checkPos = clientPlayer.playerObject.pos;
			int bulletlen = bullets.Count;
			for(int b=0; b<bulletlen; b++){
				bulletObj pew = bullets[b];
				
				bool bulletBelongsToPlayer = pew.isOwner(clientPlayer.index); //Only return the bullet if it does not belong to the connection asking for it.
				if(!bulletBelongsToPlayer && Vector3Thin.distance(checkPos, pew.pos) <= clientPlayer.maxObjDist){
					rtrnList.Add(pew);
				}
			}

			return rtrnList;
		}

		public string strReturnBulletsInView(connection clientPlayer){ //Work-around for regular players and not bots. Maybe remove later when all bullet physics are on server-side.
			Vector3Thin checkPos = clientPlayer.playerObject.pos;
			string bulletsInDist = "";
			int bulletlen = bullets.Count;
			try{
				for(int b=0; b<bulletlen; b++){
					bulletObj pew = bullets[b];	
					//Add an ID check here once the player's bullet physics are moved to the beckend!
					if(Vector3Thin.distance(checkPos, pew.pos) <= clientPlayer.maxObjDist){
						bulletsInDist += pew.source() + ",";
					}
				}
			} catch(Exception err){ // This is a band-aid fix to the error here. See "connection.getBullets()" for more info...
				if(err.GetType() != typeof(System.ArgumentOutOfRangeException)) { //ignore index out of range errors because of the above...
					FileLogger.logErrorToFile(err.ToString());
				}
			}
			bulletsInDist = (bulletsInDist != "") ? bulletsInDist.Substring(0, bulletsInDist.Length-1) : ""; //Remove the last unneeded comma...
			return bulletsInDist;
		}
		

		//For now I'm just passing the "idData", maybe connect it to the "clientIPData" object later...
		public void updateConnection(sendClientData clientData, IPEndPoint clientIPData){ //sendClientData is a C# class sent to the server in JSON from.
			if(!validConnectionID(clientData.syncID)){ return; } //Don't try to access an index that is out of range of the "connections" list.
			
			connection connect = connections[clientData.syncID];
			
			//Update the lvl each time a client sends their data.
			lvl.update();
			connect.addToSendBuffer(clientData);
			connect.ipData = clientIPData;
		}
		
		public string getClientSyncObjects(int reqID){
			string strPlayers = "";
			string strBullets = "";

			connection clientPlayer = connections[reqID]; // Cache a reference to the player in question...
			
			int len = connections.Count;
			for(int i=1; i < len; i++){ //"i" starts at 1 here to skip the first null index of the connections array.
				
				if(i != reqID){
					string getPlayerCache = connections[i].getPlayer(clientPlayer.playerObject.pos);
					string getBulletsCache = connections[i].getBullets(clientPlayer.playerObject.pos);

					if(getPlayerCache != "") strPlayers += getPlayerCache + ',';
					if(getBulletsCache != "") strBullets += getBulletsCache + ',';
				}
			}
			if(strPlayers != "") strPlayers = strPlayers.Substring(0, strPlayers.Length-1); //Remove the last commas!
			if(strBullets != "") strBullets = strBullets.Substring(0, strBullets.Length-1);

			//Add all bot bullets to the list...
			string bulletsInDist = strReturnBulletsInView(clientPlayer);
			if(strBullets != "" && bulletsInDist != "") strBullets += ","+bulletsInDist; //Append the bot's bullets to the returned player's bullets. But if instead "strBullets" is an empty string, just return "bulletsInDist" because that will already contain all bullets that need to be sent.
			else if(bulletsInDist != "") strBullets = bulletsInDist;
			

			string syncConnections = "{ " + q0 + "players" + q0 + ": [" + strPlayers + "], " + q0 + "bullets" + q0 + ": [" + strBullets + "], " + q0 + "lvlSync" + q0 + ": " + lvl.ToString() + "}";
			return syncConnections;
		}

		private int findInactiveConnection(){ //This function will break if "connections" is changed to an array!
			int ind = -1;
			for(int i=1; i < connections.Count; i++){ //Start at "i=1" because pf the dummy connection.
				if(!connections[i].isActive){
					ind = i;
					break;
				}
			}
			return ind;
		}

		public bool validConnectionID(int syncID){
			return syncID < connections.Count && syncID > 0; // "syncID > 0" to avoid dummy ID...
		}

		public bool testConnection(sendClientData clientData){
			if(!validConnectionID(clientData.syncID)){ return false; }

			//Add some type of Client secret check here later!!!

			return isRunning;
		}

		public string getReconnData(int reqID){
			return connections[reqID].getInactivePlayer();
		}

		public async Task<string> getMapData(){
			
			if(lvl.mapData != null) { //If the lvl map data is already loaded, just return it!
				return JsonConvert.SerializeObject(lvl.mapData, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			}

			//Else if the map data is null, that means it is in the process of being loaded. We need to wait for it...
			//UNCOMMENT this line later. For some reason it is printing a whole lot right now... FileLogger.logErrorToFile("Map data is Null! Waiting and trying again...");

			//Wait for the map to load by a set time.
			await Task.Delay(1000);

			//Try to send the loaded map again via recursion!
			return await getMapData();
		}


		public Game startGame() { //This must be called after the lobby phase is over!
			syncLevelData newLvl = new syncLevelData(this, lvl.mapData); //Keep the lobby's map...
			
			// Set all of the bot's level data
			for(int b=0; b<bots.Count; b++){
				bots[b].levelData = newLvl;
			}

			//Reset lvl.
			lvl = newLvl;

			//Change status
			gameStatus = GameState.Running;

			string strLog =
$@"--------------------------------
New Game Started:
Start Time: {DateTimeOffset.Now.ToString("G")}
gameID: {gameID}
--------------------------------";
			FileLogger.logToFile(strLog);

			return this;
		}

		//This function cleans up this game instance so it can be reused for another round.
		public Game purge() {
			gameStatus = GameState.Ended;
			
			//Note: DO NOT set "gameID = -1;" because I want the player client to get the "ended" level data.
			playerObj winner = alivePlayers.Length > 0 ? alivePlayers[0].playerObject : new playerObj(); //This is a catch for is there is no winner or when dummy game instances are made.
			lvl = new syncEndedData(this, winner.username);

			connections.Clear(); //Clear connections after the winner's name has been sent.
			bots.Clear();
			return this; //Return the current object that was just "purge". This is so it can be used like: Game game = new Game(-1).purge();
		}

		public async void purge(int waitTime){ //This version is only called after a player has won the game.
			
			//Be sure to set this before the wait so all the players can see the info!
			playerObj winner = alivePlayers.Length > 0 ? alivePlayers[0].playerObject : new playerObj();
			lvl = new syncEndedData(this, winner.username);

			await Task.Delay(waitTime); //Wait awhile so all player can see who won the game.
			purge(); //Call the regular version of the function.
		}

    }
	
}