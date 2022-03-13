using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
//using gameBackendTest.Controllers;
//using static gameBackendTest.GameObj.StringFormat;
using Newtonsoft.Json;
//using System.Collections.Generic;

using Keybored.BackendServer.Settings;
using Keybored.BackendServer.Logging;
using Keybored.BackendServer.AI;


namespace Keybored.BackendServer.Network {


    /// <summary> This class is the main controller for all UDP communications. </summary>
	/// <remarks> <para> This class handles everything from running multiple concurrent matches to send all data UDP packets to the connected players. </para> </remarks>
    public class Master {

        /// <summary> An array that hold <see cref="Game"/> object instances. Each instance is a lobby/match that players can connect to. </summary>
		/// <value> The length of this array is determined by <see cref="ServerSettings.simultaneousRounds"/>. </value>
        public Game[] games;
        
        /// <summary> This counter is used to give each game round a unique ID. </summary>
        private int gameIDStepper;

        /// <summary> This is the main instance of the wrapper class <see cref="UDPController"/> which is based on <see href="https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.udpclient?view=netcore-3.1">UdpClient</see>. It is used for everything UDP. </summary>
        public UDPController udpServer;
        
        /// <summary> Basic constructor for the class. It is called in <see cref="Keybored.BackendServer.Driver.Startup.ConfigureServices"/>. </summary>
        /// <remarks> <para> Note: This method also initializes both the UDP listen and send loops. </para>
        ///  <para> Note: It also has logic to setup simultaneous game rounds. </para>
        /// </remarks>
        public Master(){
            games = new Game[ServerSettings.simultaneousRounds];

            //Initialize the games array.
            for(int i=0; i < games.Length; i++){
                games[i] = new Game(this, -1);
                games[i].purge();
            }

            //Init for the UDP server. Only one udp server is allowed per master!
            udpServer = new UDPController();
            
            //Start both the sending and listening UDP loops...
            deltaTimeLoop();
            udpLoop();
        }


        /// <summary> This gets a count of online players.  </summary>
        /// <returns> This will returns a count of every player currently connected to the server. </returns>
        /// <remarks>
        ///  <para> Fix note:
        ///  <para /> <c>I probably need to cache this value!</c> </para>
        /// </remarks>
        public int onlinePlayers {
            get {
                int onCount = 0;
                for(int i=0; i < games.Length; i++){
                    if(!games[i].isEnded){
                        for(int c=1; c < games[i].connections.Count; c++){ //c=0 is the dummy connection so skip it!
                            bool isBot = games[i].connections[c] is PlayerBot;
                            bool isActive = games[i].connections[c].isActive;
                            if(!isBot && isActive){
                                onCount++;
                            }
                        }
                    }
                }
                return onCount;
            }
        }

        
        /// <summary> This method recycles ended games into fresh ones. </summary>
        /// <returns> This will return the array index of the new game, not the gameID. And it will return -1 if there was a problem/error. </returns>
        public int newGame() {
            int newInd = findEndedGame();

            if(newInd == -1){ //All game slots are currently full, the server can't start a new game!
                return -1;
            }

            games[newInd] = new Game(this, gameIDStepper++); //Start a new game in the found array index position.
            return newInd;
		}

        /// <summary> Returns the array index of the first found ended game. </summary>
        /// <returns> The int array index of the ended game. </returns>
        private int findEndedGame() {
            int ind = -1;

            for(int i=0; i < games.Length; i++){
                if(games[i].isEnded){
                    ind = i;
                    break;
                }
            }

            return ind;
        }


        /// <summary> Gets the first game with an open connection slot. </summary>
        /// <returns> The actual <see cref="Game"/> instance of the first found "in lobby" game that still has room left for players to join. </returns>
        public Game getOpenGame() {
            
            int ind = -1;

            //Return the first game that is full and that is in lobby phase.
            for(int i=0; i < games.Length; i++){
                if(!games[i].isFull && games[i].inLobby){
                    ind = i;
                    break;
                }
            }

            //If there are no open games, try to start a new one.
            if(ind == -1) {
                ind = newGame();
            }

            Game open = (ind != -1) ? games[ind] : new Game(this, -1).purge(); //If all games are full and a new one can't be started, return a empty "purged" game. You'll need to check for this outside of the function.
            return open;
        }


        /// <summary> Gets the game by gameID, not array index. </summary>
        /// <returns> The actual <see cref="Game"/> instance that matches the passed gameID. </returns>
        public Game getGame(int seekID) {
            Game rtnGame = null;
            int ind = getGameIndexByID(seekID);

            if(ind != -1){
                rtnGame = getGameByArrIndex(ind);
            }

            //If the gameID does not belong to a real/current game return a new "purged" game.
            if(rtnGame == null) {
                rtnGame = new Game(this, -1);
                rtnGame.purge();
            }

            return rtnGame;
        }


        /// <summary> Get the index by gameID. </summary>
        /// <returns> Returns the array index for the gameID passed. Will return -1 if the ID can not be found. </returns>
        /// <remarks>
        ///  <para> Fix note:
        ///  <para /> <c>I might want to change this to implement some sort of dictionary instead of loop through the game array every time...</c> </para>
        /// </remarks>
        private int getGameIndexByID(int seekID) {
            int rtnIndex = -1;

            for(int i=0; i < games.Length; i++){
                if(games[i].gameID == seekID){
                    rtnIndex = i;
                    break;
                }
            }

            return rtnIndex;
        }

        
        /// <summary> Gets the <see cref="Game"/> instance by array index. </summary>
        private Game getGameByArrIndex(int ind){
            return games[ind];
        }


        /// <summary> Logs connection info the log files. This is called whenever a client first connects. </summary>
        /// <remarks>
        ///  <para> Fix note:
        ///  <para /><c>Maybe take the username here too...</c></para>
        /// </remarks>
        public void logConnection(int gameID, int syncID, string remoteIP){
            string strLog = "--------------------------------\nConnection Stats"
                + "\nTotal Connections: " + onlinePlayers
                + "\nConnection Time: " + DateTimeOffset.Now.ToString("G")
                + "\nConnection IP: " + remoteIP
                + "\nGame ID: " + gameID
                + "\nSync ID: " + syncID
                + "\n--------------------------------";
            FileLogger.logConnectionToFile(strLog);
        }


        /// <summary> This is the main UDP Send loop. It sets up a timer loop to send sync packets on a scheduled time frame. </summary>
        /// <remarks>
        ///  <para> Note: This backend server does not wait to receive data packets from specific connected players before sending out packets. It always sends out packets to every connected player on a global timer. </para>
        /// </remarks>
        private async void deltaTimeLoop(){
            long lastLvlTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            float dt = 0f;

            float sendDataSyncTimer = 0f;
            float sendDataSyncTimerMax = 0.01f;
            if(ServerSettings.LANProfile) sendDataSyncTimerMax = 0.0167f; //This line will apply a tuning profile better for LAN servers if the user has set the game to do so in the settings JSON.

            while(true){
                await Task.Delay(5); //This "delay" is need to free the CPU to do other thing during this LONG send loop.
                
                //Update DT...
                dt = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastLvlTime)/1000.0f;
                lastLvlTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                //Keep track of Delta time here and switch a boolean flag that will alert a second thread that it needs to send data to all currently (non-timed out) players.
                sendDataSyncTimer += dt;
                if(sendDataSyncTimer > sendDataSyncTimerMax){
                    sendDataSyncTimer -= sendDataSyncTimerMax;
                    await udpSendLoop();
                }
            }
        }


        /// <summary> The main UDP receive loop. </summary>
        /// <remarks>
        ///  <para> Note: This is a non-blocking UDP server loop. It is always listening and once a player sends a data packet this method will match the player to the correct game and update the server's record of that player's data accordingly. </para>
        /// </remarks>
        private async void udpLoop(){
            while(true){
                try{
                    UdpReceiveResult udpListenData = await udpServer.Listen();
                    sendClientData body = HTTPServer_PathsController.getReqJson<sendClientData>(udpListenData.Buffer);
                    Game game = getGame(body.gameID);
                    game.updateConnection(body, udpListenData.RemoteEndPoint);
                } catch(Exception err){
                    FileLogger.logErrorToFile(err.ToString());
			    }
            }
        }

        
        /// <summary> This method is not a continuous loop, but sends UDP sync data packets every connected player of every game instance. </summary>
        /// <returns> Always returns 1 to let the calling function be able to "await" this method. (You can't "await" a void method). </returns>
        /// <remarks>
        ///  <para> Note: This loop uses the "Network Frame Buffer System" to send data in a short delays to allow for smooth client rendering. </para>
        /// </remarks>
        private async Task<int> udpSendLoop(){
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

            for(int g=0; g<games.Length; g++){
                Game game = games[g];
                if(game.isEnded) continue; //Don't update ended games!

                for(int i=1; i<game.connections.Count; i++){ //Start at 1 to avoid dummy. Use "game.connections.Count" because the connnections list might change length during this loop.
                    try{
                        connection connect = game.connections[i];

                        bool isBot = connect is PlayerBot; //Check and don't try to send data to bots!!!
                        bool noIPData = connect.ipData == null;
                        bool notActive = !connect.isActive; //Don't send data to disconnected players...
                        if(isBot || noIPData || notActive) continue;

                        string dataSync = game.getClientSyncObjects(i);
                        byte[] res = HTTPServer_PathsController.getResJson(dataSync);
                    
                        IPEndPoint clientAddress = connect.ipData;
                        await udpServer.Send(clientAddress, res);
                        connect.renderBufferFrame(); //It is most efficient to only advance the send buffer once after the last frame of data has been sent.
                    } catch(Exception err){
                        FileLogger.logErrorToFile(err.ToString());
                    }
                }
            }
            
            return 1;
        }

        
    }

}