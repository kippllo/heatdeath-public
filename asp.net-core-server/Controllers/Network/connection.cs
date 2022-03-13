using System;
using System.Net;
//using static gameBackendTest.GameObj.StringFormat;
using Newtonsoft.Json;
using System.Collections.Generic;

//using gameBackendTest.Controllers;
//using System.Net.Sockets;

using Keybored.BackendServer.GameEngine;
using Keybored.BackendServer.Logging;
using Keybored.BackendServer.Settings;


namespace Keybored.BackendServer.Network {

    public class connection {
		
		public IPHash ipHash;
		public IPEndPoint ipData;

		protected Game game; //The parent "Game" object that this connection belongs to.
		public int index;
		public string player; //This is needed to cache the player string, but bullets are different for each player, so caching them would not be helpful.

		public playerObj playerObject;
		public bulletObj[] bulletObjects;
		public float maxObjDist; //This is the max distance in which network object will be sent to the client for rendering in the game scene.

		public long lastSyncTime; //This is only on the server, it is not present on the client. Note it should always be set with: lastSyncTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

		public List<sendClientData> syncSendBuffer = new List<sendClientData>();
		public static int maxBufferSize = 8;

		
		private float finalFrameTimer = 0f; // Testing maybe keep
		private float finalFrameTimerMax = 0.075f; //This is in seconds.  //1000; //There is no DT in the function that uses this... //0.25f; // Testing maybe keep
		private long finalFrameLastTime;


		public connection(Game gameIN, int indexIN, string connectionIP) {
			game = gameIN;
			index = indexIN;
			player = "";
			playerObject = new playerObj(); //If you access a property something that might be null, it should be set to a new Object in the constructor.
			bulletObjects = new bulletObj[0];
			maxObjDist = ServerSettings.objRenderDist;

			// Give an initial grace period to give the player, extra time to download and render the map when they first connect before they get a timeout and the syncID is recycled.
			long extraTime = (long)ServerSettings.initialConnectionGracePeriod*1000; // "ServerSettings.initialConnectionGracePeriod" must be converted to milliseconds.
			lastSyncTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() + extraTime; // From the current time, make the "lastSyncTime" be some many seconds in the future. This will let the connection not be dropped in the regular amount of time.
			
			ipHash = new IPHash(connectionIP); //Save a hashed copy of the IP address.

			if(ServerSettings.LANProfile) maxBufferSize = 5; //Reduce the buffer delay size on LAN networks...
		}

		
		
		public virtual void updateObjects(string playerData, string bulletsData) {
			//Save the changes to the play object. Which are used for checking and filtering.
			playerObject = playerObj.FromString(playerData);
			
			//Save changes to bullets array
			bulletObjects = JsonConvert.DeserializeObject<bulletObj[]>("["+ bulletsData +"]", new JsonSerializerSettings{
				TypeNameHandling = TypeNameHandling.None
			});
			
			player = playerData; //Not sure I need these...
		}

		private float timeSinceSync { //Time in seconds since the last time this connection was synced.
			get {
				return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastSyncTime)/1000.0f;
			}
		}

		public virtual bool isActive { //Time in seconds since the last time this connection was synced.
			get {
				float secondTimeout = ServerSettings.connectionDropTime;
				return timeSinceSync < secondTimeout;
			}
		}

		public bool isPlayerAlive {
			get {
				return (playerObject.hp > 0 && isActive);
			}
		}

		public string getPlayer(Vector3Thin checkPos) {
			bool withinDist = Vector3Thin.distance(checkPos, playerObject.pos) <= maxObjDist;
			bool shouldSendPlayer = (isActive && withinDist);
			bool playerIsAlive = playerObject.hp > 0;
			bool sendFinalFrame = playerObject.flgSoundDeath; // Send one last network frame if the player has just died.

			if(sendFinalFrame){
				float ffDt = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - finalFrameLastTime)/1000.0f;
				finalFrameTimer += ffDt;
				if(finalFrameTimer > finalFrameTimerMax) playerObject.flgSoundDeath = false; // Set this back to false so the final network frame is only sent once. (Note: This line won't change the above variable's ("sendFinalFrame") value.)
			}
			finalFrameLastTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			
			return (playerIsAlive || sendFinalFrame) && shouldSendPlayer ? player : "";
		}

		public string getInactivePlayer() { //Returns player data even if the player is inactive. Used for reconnecting.
			playerObject.hp /= 2; //Penalize the player half of his current HP if he reconnects to a round. This is a temp fix to the "quit-warping" exploit.
			string playerJSON = JsonConvert.SerializeObject(playerObject, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			//Note: "playerJSON = playerJSON.Replace(q0, q1)" is not needed in this function because the frontend is coded to accept the single quote JSON version.
			return playerJSON;
		}

		public virtual string getBullets(Vector3Thin checkPos) {
			string bulletsInDist = "";

			try{
				for(int i=0; i<bulletObjects.Length; i++){
					if(Vector3Thin.distance(checkPos, bulletObjects[i].pos) <= maxObjDist){
						bulletsInDist += bulletObjects[i].source() + ",";
					}
				}
			} catch(IndexOutOfRangeException err){
				// This is a band-aid fix to the error here.
				// It will catch the error, log it, and continue to run the function below 
				// so the game will return all of the bullets that were successfully stored in "bulletsInDist"
				// before the error occurred.
				FileLogger.logErrorToFile(err.ToString());
			}

			bulletsInDist = (bulletsInDist != "" && isActive) ? bulletsInDist.Substring(0, bulletsInDist.Length-1) : ""; //Remove the last comma! It is best to just add a comma to all bullets and remove the last one here!

			return bulletsInDist;
		}


		// THIS WON'T RETURN OTHER BOT'S BULLETS TO THE CALLING BOT!!!!!!!!!
		// Fix this by checking the "game.bullets" list as well! (But you'll need to also check bot index...)
		//Why is this function not part of the "PLayerBot" class that extends this "connection" class? Because it is called on non-bot connections as well!
		// This function is actaully only called on non-bot players!!!
		public virtual List<bulletObj> getBulletsObjList(Vector3Thin checkPos) { //For AI damage collision check...
			List<bulletObj> rtrnList = new List<bulletObj>();
			rtrnList.Capacity = bulletObjects.Length;

			for(int i=0; i<bulletObjects.Length; i++){
				if(Vector3Thin.distance(checkPos, bulletObjects[i].pos) <= maxObjDist){
					rtrnList.Add(bulletObjects[i]);
				}
			}
			return rtrnList;
		}


		public virtual void addToSendBuffer(sendClientData netFrame){
			lastSyncTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //Update the last sync time.

			syncSendBuffer.Add(netFrame);

			if(syncSendBuffer.Count > maxBufferSize){
				syncSendBuffer.RemoveAt(0); //Remove the oldest frame
			}
		}

		
		public virtual void renderBufferFrame(){ // This method applies the cached buffer frame data to this object so that it will be properly formatted for JSON converting.
			if(syncSendBuffer.Count == 0) return; //If there is no new data, keep the properties currently rendered to this object.
			else if(syncSendBuffer[0] == null){
				syncSendBuffer.RemoveAt(0);
				renderBufferFrame();
				return;
			}

			updateObjects(syncSendBuffer[0].localPlayer, syncSendBuffer[0].localBullets); //Render the data to this connection object's properties.

			if(syncSendBuffer.Count > 1) { //Pop the oldest frame.
				syncSendBuffer.RemoveAt(0);
			}
		}


		public static List<T> wipeBuffer<T>(List<T> buffer, int maxSize){
            int amountOfFramesToKeep = (maxSize/2);
            int countToRemove = buffer.Count - amountOfFramesToKeep; //Keep the most recent half of the buffered data frames.
            
            int[] oldFrameIndices = new int[amountOfFramesToKeep]; //Holds the indices of the oldest frames that should be kept.
            List<T> oldFrames = new List<T>();
            //Note "countToRemove" is the number of old frames left after the new frames are separated.
            for(int i=0; i<amountOfFramesToKeep; i++){ //Note: We are stopping before "i == amountOfFramesToKeep", this is fine because "List.RemoveRange(0, amountOfFramesToKeep)" (or for example "List.RemoveRange(0, 10)") would not remove index 10, but stop at 9. This is because indices 0 through 9 are a total of ten indices.
                oldFrameIndices[i] = countToRemove / amountOfFramesToKeep * i;
                oldFrames.Add(buffer[ oldFrameIndices[i] ]); //I might need to clone the index here...
            }

            buffer.RemoveRange(0, countToRemove); //Remove all old frames.
            
            List<T> fixedFrames = new List<T>();
            fixedFrames.AddRange(oldFrames);
            fixedFrames.AddRange(buffer);
            //Note: I can't use "buffer.AddRange(oldFrames);" because this will add the old frame after the newer frames, which we don't want!


            return fixedFrames;
        }
		
	}

}