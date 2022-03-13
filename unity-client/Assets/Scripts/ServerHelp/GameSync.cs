//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading.Tasks;
using System;

using UnityEngine.SceneManagement;
using TMPro;

using Newtonsoft.Json; //remove and move "syncClient" to server controls later...
using System.Text; //same as above...
//using System.Net;

using Keybored.FPS;


public class GameSync : MonoBehaviour {

	public charCtrl client;

	public GameObject WorldCanvas;
	public GameObject localPlayerPrefab;
	public GameObject syncPlayerPrefab;
	public GameObject syncBulletPrefab;

	public ZoneCtrl zoneCtrl;

	public TextMeshProUGUI lobbyTimer;
	public TextMeshProUGUI playerCountUI;
	MapData map;

	public static syncServerData curRenderFrame;
	private float downloadTimer = 0f;
	private float downloadTimerMax = 0.03f;
	private readonly float downloadTimerMaxInit = 0.03f;


	//Note: All "async Task" function in unity will not throw error that will appear in the unity console.
	//	You must wrap the function in a try/catch that will log any error!
	async Task Start () {
		//Note because of calling "await", the first "Update" call will happen before this start method finishes!
		//That means all "Update" sensitive configuration must be done before the first "await"!
		//Disregard the above line because using "client != null" as a check flag fixes the issue.

		await serverCtrl.getSyncID(); //Make the initial connection to the serever.

		//If the server responded with the error mess of "gameID == -1", this means no games are available to be joined.
		if(serverCtrl.gameID == -1) {
			SceneManager.LoadScene(0); //Go back to server join screen...
		}
		
		GameObject localPlayerObj = Instantiate(localPlayerPrefab, WorldCanvas.transform); //Parent the new game object to the world canvas so its local UI will show up...

		//Hide the player while its position is loading...
		localPlayerObj.SetActive(false);
		
		TempRoundData.reset(); //Reset the temp data holder. Reset it up before downloading the map, so that it will keep the map data.
		zoneCtrl.gameObject.SetActive(true); //The "zoneCtrl" should stay disabled until the TempRoundData class has be reset! If it does not it will try to draw a safety circle based on the last round's circle stats that are left over in the static "TempRoundData" class.
		await getMapData(); //Download the map...

		if(serverCtrl.tryReconnect){ //Set the player's position to match what was last synced to the server upon a reconnect.
			playerObj serverClientData = await serverCtrl.getReconnData();
			localPlayerObj.GetComponent<charCtrl>().setFromReconnData(serverClientData);
		} else {
			//Move the player to one of the lobby spawn points, from there he can explore to find a good starting place...
			int randInd = UnityEngine.Random.Range(0, map.spawnPoints.Length);
			localPlayerObj.transform.position = map.spawnPoints[randInd].ToVector3();
		}

		//Show the player object again now the it has the correct position.
		localPlayerObj.SetActive(true);
		
		client = localPlayerObj.GetComponent<charCtrl>(); //Set the client script down here so the update won't run until the async start function is fully done!

		serverCtrl.startUDPLoop();
	}
	
	
	void Update () {
		
		//Sync data if connected...
		if (serverCtrl.isConnected() && client != null) {
			SendData(); //prepareSyncData();

			downloadTimer += Time.unscaledDeltaTime;
			if(downloadTimer > downloadTimerMax){
				downloadTimer -= downloadTimerMax;
				DownloadData();
			}


			int bufferLen = serverCtrl.syncResBuffer.Count;

			if(bufferLen < serverCtrl.maxBufferSize){
				//Slow down the render frame timer a bit if our buffer is not full, this will allow the server time to catch-up.
				
				downloadTimerMax = downloadTimerMaxInit; //First rest it to the default timer max...
				downloadTimerMax += downloadTimerMax * (bufferLen/serverCtrl.maxBufferSize); //Then increases it propertionately.

			} else downloadTimerMax = downloadTimerMaxInit;
			//The extra render is done before the buffer wipe so that PPS will stay high. Also, the buffer is its own "if" not an "else if". This way both the extra render and the buffer wipe can be called in the same frame!
			
			//This big server wipe is most for if a player's game is put in sleep mode or something and needs to catch up to realtime fast!
			
			//The below block is only temp disabled, maybe put it back once testing is done...
			int minWipeSize = serverCtrl.maxBufferSize*10;
			if(bufferLen > minWipeSize){
				serverCtrl.syncResBuffer = serverCtrl.wipeBuffer(serverCtrl.syncResBuffer, minWipeSize);
				bufferLen = serverCtrl.syncResBuffer.Count;
			}

			//This extra render is what should be used most times to keep a regular running game in realtime sync with the server.
			if(bufferLen > serverCtrl.maxBufferSize){
				//Render the oldest data frame!
				DownloadData(); //Instead of throwing the oldest frame out, just skip the timer and render it now!
				bufferLen--; //The buffer used one frame in the above line.
			}

			// If this client is running slower than 60FPS, it will fall behind the server in sync time because the server sends network frames at 60PPS.
			// If the above and if the buffer is still bigger than the limit, remove some extra frames based on the amount of data frames we are over the limit...
			if(FPSLib.AvgFPS < 60 && bufferLen > serverCtrl.maxBufferSize) {
				//Skip the next few data oldest data frames. (The amount to skip is based on the amount of extra the buffer has stored!)
				int skipAmount = Mathf.RoundToInt( bufferLen / (serverCtrl.maxBufferSize*2) );
				serverCtrl.syncResBuffer.RemoveRange(0, skipAmount);
				bufferLen -= skipAmount;
			}

		}


		if(serverCtrl.shouldDisconnect){
			// Message is set in the serverCtrl Script...
			serverCtrl.CloseUDP(); //Stop any incoming UDP packets.
			SceneManager.LoadScene(2);
		}
	}






	void SendData(){
		try{
			//Data prepared to be sent to server-------------------
			//Get this client's data ready to be sent.
			//Grab the bullets that need to be sent...
			GameObject[] localBulletsGameObjects = GameObject.FindGameObjectsWithTag("bullet");
			bulletObj[] localBullets = new bulletObj[localBulletsGameObjects.Length];
			for (int i = 0; i < localBulletsGameObjects.Length; i++) {
				localBullets[i] = localBulletsGameObjects[i].GetComponent<bulletCtrl>().toSyncData();
			}

			sendClientData clientData = new sendClientData(serverCtrl.gameID, serverCtrl.syncID, client.toSyncData(), localBullets); //Instantiate a sendClientData based on this local client's player data.
			string json = JsonConvert.SerializeObject(clientData, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			byte[] syncSendBytes = serverCtrl.Compress(Encoding.ASCII.GetBytes(json));

			serverCtrl.udpSyncSend(syncSendBytes);
		}  catch(Exception err){
			Debug.Log(err);
		}
	}



	void DownloadData() {
		try{
			if(serverCtrl.syncResBuffer.Count == 0) return;
			else if(serverCtrl.syncResBuffer[0] == null) { //If a UDP packet failed to download correctly, skip it and try again with the next one!
				serverCtrl.syncResBuffer.RemoveAt(0);
				DownloadData();
				return;
			}

			byte[] bytesDecompressed = serverCtrl.Decompress(serverCtrl.syncResBuffer[0]);
			string resString = Encoding.ASCII.GetString(bytesDecompressed);

			curRenderFrame = JsonConvert.DeserializeObject<syncServerData>(resString, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
			try{
				RenderData();
			} catch(Exception err){ //Keep this catch just in case!!
				Debug.Log("\n\n\n JSON body Error: ");
				Debug.Log("\n resString: " + resString);
				Debug.Log(err);
			}

			//Add the pack to the PPS count only after it is rendered, is will allow the PPS to be more accurate. Don't count your chickens before they hatch...
			if(serverCtrl.syncResBuffer.Count > 1) { //Check to make sure the buffer has more data before remove the current index. The buffer needs at least I frame in it at all times!
				serverCtrl.syncResBuffer.RemoveAt(0);
				serverCtrl.packetCountDown++;
			}
		} catch(Exception err){
			Debug.Log(err);
		}
	}



	void RenderData() {
		if(serverCtrl.isConnected()){ //Sometimes when a player leaves a round, this function is still running as an async task when they are on the main menu. In those cases the syncKey might be stepped even though the current round has ended. This will cause the next round the player joins to have a frozen syncKey. Checking to make sure the player is still connected to match after the download from the server will fix this.
			serverCtrl.levelState = curRenderFrame.lvlSync.lvlType; //Update the level state var of the server.

			//If the play is actually in-game save the reconnection data to a file in case the play drops out for a bit.
			if(curRenderFrame.lvlSync.inGame && serverCtrl.saveReconn && !serverCtrl.hasReconnectFile){ //Remove "&& saveReconn" in production...Debug
				reconnectData reconn = new reconnectData(serverCtrl.gameID, serverCtrl.syncID, serverCtrl.urlPrefix);
				serverCtrl.saveReconnectFile(reconn);
			}
			
		}

		//Change the status on all NetworkTrackers to "Pending".
		GameObject[] networkTrackers = GameObject.FindGameObjectsWithTag("status:Updated");
		for(int i=0; i<networkTrackers.Length; i++){
			networkTrackers[i].tag = "status:Pending";
		}

		//Sync network bullets.
		bulletObj[] networkBullet = curRenderFrame.getBullets();
		if (networkBullet.Length > 0) { //Check to make sure there are network objects returned.
			//Update all the in game object:
			for (int i = 0; i < networkBullet.Length; i++) {
				networkBullet[i].setPrefab(syncBulletPrefab);
				networkBullet[i].setParent(WorldCanvas);
				networkBullet[i].updateObject();
			}
		}

		//Sync Network players
		playerObj[] networkPlayers = curRenderFrame.getPlayers();
		if (networkPlayers.Length > 0) {
			for (int i = 0; i < networkPlayers.Length; i++) {
				networkPlayers[i].setPrefab(syncPlayerPrefab);
				networkPlayers[i].setParent(WorldCanvas);
				networkPlayers[i].updateObject();
			}
		}


		if(curRenderFrame.lvlSync.inLobby){
			//Sync Lobby Data
			syncLobbyData networkLobby = curRenderFrame.lvlSync.getLobby();

			//Make sure both of these two UI gameObject are always active in the scene!
			lobbyTimer.text = "Lobby: "+ Mathf.RoundToInt(networkLobby.lobbyStartTimer);
			playerCountUI.text = "Players: \n" + networkLobby.playersLeft + "/" + networkLobby.playersMax; //I added a new line before the player count number...

		} else if(curRenderFrame.lvlSync.inGame){
			//Sync Level Data
			//Show the player's alive count here...
			//Maybe a pass a global map clock that will control the moving death wall...
			syncLevelData networkLvl = curRenderFrame.lvlSync.getGameLvl();
			
			lobbyTimer.text = "Zone: " + Mathf.RoundToInt(networkLvl.circleUpdateTimer);
			playerCountUI.text = "Players: " + networkLvl.playersLeft + "/" + networkLvl.playersMax;
			TempRoundData.zoneTime = networkLvl.circleUpdateTimer;

			// Only redraw the circle if it's center has changed!
			if(TempRoundData.safeCenter.x != networkLvl.safeCenter.x || TempRoundData.safeCenter.y != networkLvl.safeCenter.y || TempRoundData.safeCenter.z != networkLvl.safeCenter.z){
				TempRoundData.safeCenter = networkLvl.safeCenter; //reset the cache.
				TempRoundData.safeDist = networkLvl.safeDist;
				TempRoundData.NextSafeCenter = networkLvl.NextSafeCenter; //reset the cache.
				TempRoundData.NextSafeDist = networkLvl.NextSafeDist;
				
				TempRoundData.maxZoneTime = networkLvl.circleUpdateTimer; //When the zone changes, set reset the max of the timer to match the start time of the zone from the server.
			}
			
		} else if(curRenderFrame.lvlSync.inEnded) {
			syncEndedData networkEnded = curRenderFrame.lvlSync.getEndedData();
			serverCtrl.serverMessage = "Game Winner: " + networkEnded.winner;
			
			serverCtrl.CloseUDP(); //Stop any incoming UDP packets.
			SceneManager.LoadScene(2);
		}
		

		//Delete all NetworkTrackers that were not updated by server data. That is any that are still in a "Pending" status.
		GameObject[] networkTrackersToDelete = GameObject.FindGameObjectsWithTag("status:Pending");
		for(int i=networkTrackersToDelete.Length-1; i>=0; i--){ //Reverse array to delete from the last to the first index.
			UnityEngine.GameObject.Destroy(networkTrackersToDelete[i].transform.parent.gameObject); //All NetworkTrackers are children of the main network-sync object.
		}
	}



	async Task getMapData() {
		//If there is an old map object, destroy it.
		GameObject oldMap = GameObject.Find("map");
		if(oldMap) { Destroy(oldMap); }

		//Get fresh map data from the server.
		map = await serverCtrl.SendMapData();
		map.GenMap();

		//Set a reference to the map in the tempData class.
		TempRoundData.map = map;
	}

}
