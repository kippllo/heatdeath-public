//Warning disable help: https://answers.unity.com/questions/21796/disable-warning-messages.html
// And: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives/preprocessor-pragma-warning
#pragma warning disable CS4014 //Turn off the warning: "The statement is not awaited and execution of current method continues before the call is completed."
using UnityEngine;

// HttpClient wrong version help: https://stackoverflow.com/questions/52457487/how-to-update-c-sharp-version-in-visual-studio-code
//		https://forum.unity.com/threads/how-do-you-change-the-target-c-version-for-visual-studio.596440/
//      https://stackoverflow.com/questions/9611316/system-net-http-missing-from-namespace-using-net-4-5
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System;

using System.IO; //Help: https://stackoverflow.com/questions/39191950/how-to-compress-a-byte-array-without-stream-or-system-io
using System.IO.Compression;
//using System.Dynamic;
using System.Net;


//using UnityEngine.SceneManagement; //Add for new UDP loop...
//using System.Net.Sockets; //Testing maybe remove later...
using System.Collections.Generic;
using System.Threading;

public static class serverCtrl {

	static HttpClient client;
	static UDPController udpServer;

	public static int gameID;
	public static int syncID;
	private static int syncNextObjID;
	private static int ObjIDLimit = 4; //This number will be the max digits allowed after after the "syncID". For example, ObjIDLimit = 4, means "syncID" + 0000. That's 9999 objects allowed per player.    ObjIDLimit = 10, means "syncID" + 0000000000. That's 9999999999 objects allowed per player.

	public static string urlPrefix;
	public static string urlPrefixDefault;
	public static string username;

	public static bool tryReconnect;
	public static bool shouldDisconnect;

	public static LevelDataClient.LvlType levelState;

	public static string serverMessage; //This is a general var to hold server messages like "Disconnected" or "Winner: kip".

	public static int packetCountUp; //Upload
	public static int packetCountDown; //Download
	public static bool saveReconn; //Debug remove in final build, or move to settings menu...

	private static bool flgUDPLoop;
	private static GameSync gameSync;
	//private static IPEndPoint serverConnData;

	public static List<byte[]> syncResBuffer;
	public static readonly int maxBufferSize = 7;
	private static Thread UDPTaskThread;


	public static void reset() {
		ClientSettings.ReadSettings(); //Make sure the newest settings are read.
		urlPrefixDefault = "http://" + ClientSettings.defaultIP + ":" + ClientSettings.defaultPort;
		
		client = new HttpClient();
		client.Timeout = TimeSpan.FromSeconds(30); //Set the timeout to be 30 seconds.

		CloseUDP();
		udpServer = new UDPController(); //start fresh with a new UDP server!

		//Reset UDP cached vars to null. (Maybe move this inside of "CloseUDP()"...)
		gameSync = null;
		//serverConnData = null;

		gameID = -1;
		syncID = -1;
		syncNextObjID = 0;
		tryReconnect = false;
		username = "Not_A_Name";
		saveReconn = false;
		shouldDisconnect = false;
		levelState = LevelDataClient.LvlType.Lobby;

		flgUDPLoop = false; //Stop the UDP loop.
		packetCountUp = 0; //Add these, I think I've always needed them...
		packetCountDown = 0;

		syncResBuffer = new List<byte[]>();
	}

	public static void CloseUDP(){
		flgUDPLoop = false; //Stop the UDP loop.
		if(UDPTaskThread != null) UDPTaskThread.Abort(); //Shutdown the UDP thread.
		UDPTaskThread = null;

		//Close any old UDP server.
		if(udpServer != null){
			udpServer.Close();
			udpServer = null; //Set the controller to null as well! Note: "udpServer.udpListen" has already been set to null in the above function.
		}
	}

	public static void disconnect(string errMess) {
		serverMessage = errMess;
		
		gameID = -1;
		syncID = -1;
		shouldDisconnect = true;

		if(saveReconn){
			reconnectData reconn = new reconnectData(gameID, syncID, urlPrefix);
			saveReconnectFile(reconn);
		}
	}

	public static bool isConnected() {
		return (gameID != -1 && syncID != -1);
	}

	public static async Task<int> getOnlineCount() {
		try{
			string jsonGetStr = await client.GetStringAsync(urlPrefixDefault + "/api/onlinePlayers");
			OnlineCount countRes = JsonConvert.DeserializeObject<OnlineCount>(jsonGetStr, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
			
			return countRes.onlinePlayers;
		} catch(Exception err){
			Debug.Log(err);
			return -1; //There is no connection to main server.
		}
	}

	public static async Task<ServerInfo> GetServerInfo() {
		// Note for some reason "JsonConvert.DeserializeObject" does not return a dynamic object when using "portable-net40+win8+wpa81+wp8+sl5" version of JSON.Net.
		// The below code includes a work around...
		try{
			string jsonGetStr = await client.GetStringAsync(urlPrefixDefault + "/api/serverInfo");
			ServerInfo infoObj = JsonConvert.DeserializeObject<ServerInfo>(jsonGetStr, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});

			return infoObj;
			
		} catch(Exception err){
			Debug.Log(err);

			ServerInfo errObj = new ServerInfo("-1.0", "Cannot connect to server. Check internet connection.");
			return errObj;
		}
	}


	//Note: the game tells if this function succeeds by the "gameID" not being equal to "-1".
	public static async Task getSyncID() {
		if(tryReconnect){
			await reconnectToServer();
		} else {
			try{
				string jsonGetStr = await client.GetStringAsync(urlPrefix + "/api/requestID");
				Connection newConnection = JsonConvert.DeserializeObject<Connection>(jsonGetStr, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
				gameID = newConnection.gameID;
				syncID = newConnection.syncID;

				if(saveReconn){
					deleteReconnectFile(); //Delete the old reconnection file, so a fresh one will be saved!
				}
			} catch(Exception err){
				Debug.Log(err);
				gameID = -1;
				syncID = -1;
			}
		}
	}

	//Return the next objID and update the internal counter.
	public static int getNextObjID(){
		//Add overflow handeling later, maybe make it loop to negative numbers... (Like 19999 loops to -10000).
		int nextID = syncNextObjID++; //Grab the next SubID then increment it.
		string strID = ""+ nextID;
		int objID = int.Parse(syncID + strID.PadLeft(ObjIDLimit, '0'));
		return objID;
	}



	private static void cacheUDPSettings(){
		if(gameSync == null){ //if(gameSync == null || serverConnData == null){
			//Cache the values for later use.
			gameSync = GameObject.Find("GameSync").GetComponent<GameSync>();
			//IPAddress ipAdd = Dns.GetHostAddresses(ClientSettings.defaultIP)[0];
			//serverConnData = new IPEndPoint(ipAdd, ClientSettings.defaultPort);
		}
		//If there is already cache do nothing.
	}

	public static void udpSyncSend(byte[] netFrame) {
		//Cache the settings vars to improve UDP Speed...
		cacheUDPSettings(); //Find a way to only call once...

		try{
			////udpServer.udpListen.Send(netFrame, netFrame.Length, serverConnData);
			udpServer.udpListen.Send(netFrame, netFrame.Length);
			packetCountUp++; //Update the Debug packetCounter...
		} catch(Exception err){
			Debug.Log(err);
		}
	}

	
	public static void startUDPLoop() { //public static void startUDPLoop() {
		cacheUDPSettings();
		Thread listen = new Thread(new ThreadStart(udpSyncListenLoop));
        listen.Start();
		UDPTaskThread = listen; //Save the thread so it can be aborted later...
	}

	public static void udpSyncListenLoop() {
		flgUDPLoop = true; //Start the loop. (The loop is ended in other functions.)

		while(flgUDPLoop){
			try{
				IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0); //This var is filled by the next line's function...
				syncResBuffer.Add(udpServer.udpListen.Receive(ref RemoteIpEndPoint)); // Note: This method should only listen for data from the IPEndPoint set in the "udpServer" constructor. Which is "UDPController.Constructor".
			} catch(Exception err){
				Debug.Log(err);
			}
		}
	}

	public static List<byte[]> wipeBuffer(List<byte[]> buffer, int maxSize){
		int amountOfFramesToKeep = (maxSize/2);
		int countToRemove = buffer.Count - amountOfFramesToKeep; //Keep the most recent half of the buffered data frames.
		
		int[] oldFrameIndices = new int[amountOfFramesToKeep]; //Holds the indices of the oldest frames that should be kept.
		List<byte[]> oldFrames = new List<byte[]>();
		//Note "countToRemove" is the number of old frames left after the new frames are separated.
		for(int i=0; i<amountOfFramesToKeep; i++){ //Note: We are stopping before "i == amountOfFramesToKeep", this is fine because "List.RemoveRange(0, amountOfFramesToKeep)" (or for example "List.RemoveRange(0, 10)") would not remove index 10, but stop at 9. This is because indices 0 through 9 are a total of ten indices.
			oldFrameIndices[i] = countToRemove / amountOfFramesToKeep * i;
			oldFrames.Add(buffer[ oldFrameIndices[i] ]); //I might need to clone the index here...
		}

		buffer.RemoveRange(0, countToRemove); //Remove all old frames.
		
		List<byte[]> fixedFrames = new List<byte[]>();
		fixedFrames.AddRange(oldFrames);
		fixedFrames.AddRange(buffer);
		//Note: I can't use "buffer.AddRange(oldFrames);" because this will add the old frame after the newer frames, which we don't want!

		return fixedFrames;
	}


	//This method takes any object type and sends it to the server via http POST @ the urlPath passed.
	//It then returns a string of the JSON-binary response from the server.
	// Note "reqT" is the request type of the object passed into the function.
	// 		"resT" is the response type of the object returned from the server.
	static async Task<resT> postToServer<reqT, resT>(reqT postObj, string urlPath){
		try{
			string json = JsonConvert.SerializeObject(postObj, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			byte[] compressedBytes = Compress(Encoding.ASCII.GetBytes(json));
			var body = new ByteArrayContent(compressedBytes);
			body.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			
			var res = await client.PostAsync(urlPrefix + "/api/" + urlPath, body);

			var resBytes = await res.Content.ReadAsByteArrayAsync(); //Get returned HTTP package.
			byte[] bytesDecompressed = Decompress(resBytes);
			string resString = Encoding.ASCII.GetString(bytesDecompressed);
			resT resPackage = JsonConvert.DeserializeObject<resT>(resString, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
			return resPackage;
		}  catch(Exception err){
			Debug.Log(err);

			if( isConnected() && err.GetType() == typeof(System.Net.Http.HttpRequestException) ) {
				//This means the game was running fine, but the server went down.
				disconnect("Disconnected From Server...");
			}

			resT errObj = (resT) new System.Object();
			return errObj;
		}
	}

	static async Task<resT> postToServer<reqT, resT>(reqT postObj, string urlPath, string urlIP){ //This uses C# Method Overloading: help: https://stackoverflow.com/questions/15479174/how-to-overload-two-methods-with-different-input-parameters    AND: https://www.geeksforgeeks.org/c-sharp-method-overloading/
		try{
			string json = JsonConvert.SerializeObject(postObj, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			byte[] compressedBytes = Compress(Encoding.ASCII.GetBytes(json));
			var body = new ByteArrayContent(compressedBytes);
			body.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
			
			var res = await client.PostAsync(urlIP + "/api/" + urlPath, body);

			var resBytes = await res.Content.ReadAsByteArrayAsync(); //Get returned HTTP package.
			byte[] bytesDecompressed = Decompress(resBytes);
			string resString = Encoding.ASCII.GetString(bytesDecompressed);
			//Debug.Log("resString: " + resString); //Debug remove later!
			resT resPackage = JsonConvert.DeserializeObject<resT>(resString, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
			return resPackage;
		}  catch(Exception err){
			Debug.Log(err);
			resT errObj = (resT) new System.Object();
			return errObj;
		}
	}


	//Move these two function to a data-process Lib later...
	public static byte[] Compress(byte[] data) {
		MemoryStream output = new MemoryStream();
		using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
		{
			dstream.Write(data, 0, data.Length);
		}
		return output.ToArray();
	}

	public static byte[] Decompress(byte[] data) {
		MemoryStream input = new MemoryStream(data);
		MemoryStream output = new MemoryStream();
		using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
		{
			dstream.CopyTo(output);
		}
		return output.ToArray();
	}



	public static async Task reconnectToServer(){
		reconnectData reconn = isReconnectFileValid ? readReconnectFile() : new reconnectData();

		if( !(await testServerConnection(reconn.gameID, reconn.syncID, reconn.urlPrefix)) ){ //Check the connection to make sure the round has not ended on the server-side.
			gameID = -1;
			syncID = -1;
			urlPrefix = "";
		} else { //If we did successfully reconnect to the server, update the server static connection vars.
			gameID = reconn.gameID;
			syncID = reconn.syncID;
			urlPrefix = reconn.urlPrefix;

			if(saveReconn){ // Delete the old reconnection file so a fresh one will be saved!
				deleteReconnectFile();
			}
		}
	}

	public static async Task<playerObj> getReconnData(){
		sendClientData clientData = new sendClientData(gameID, syncID);
		playerObj resPackage = await postToServer<sendClientData, playerObj>(clientData, "reconnectData");
		return resPackage;
	}

	public static async Task<MapData> SendMapData(){
		sendClientData clientData = new sendClientData(gameID, syncID);
		MapData resPackage = await postToServer<sendClientData, MapData>(clientData, "SendMapData");
		return resPackage;
	}

	public static async Task<bool> testServerConnection(int testGameID, int testSyncID, string testUrlIP){ //PASS the "sendClientData" params instead of grabbing them from the static vars...
		try{
			sendClientData clientData;
			responseMessage resPackage;
			bool success = false;

			clientData = new sendClientData(testGameID, testSyncID);
			resPackage = await postToServer<sendClientData, responseMessage>(clientData, "testConnection", testUrlIP);

			if(resPackage.success){
				//Save the reconnection data to a file in case the player drops out for a bit.
				if(saveReconn){
					reconnectData reconn = new reconnectData(testGameID, testSyncID, testUrlIP);
					saveReconnectFile(reconn);
				}

				success = true;
			}

			return success;
		} catch(Exception err){
			Debug.Log(err);
			return false;
		}
	}

	public static void saveReconnectFile(reconnectData reconn) { //private static void saveReconnectFile(reconnectData reconn) {
		string filepath = Application.persistentDataPath + "/settings";
		string filename = "/reconn.dat";

		if(!Directory.Exists(filepath)){
			Directory.CreateDirectory(filepath);
		}

		string json = JsonConvert.SerializeObject(reconn, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); //Convert the class into a JSON string.
		byte[] strBytes = Encoding.ASCII.GetBytes(json); //Get the string's binary data.
		byte[] compressedBytes = Compress(strBytes); //Compress the binary data so the file size will be smaller and user won't be able to read it.

		File.WriteAllBytes(filepath+filename, compressedBytes);
	}

	public static void deleteReconnectFile(){ //Maybe undo this being public later. But for now other scripts need this!
		string filepath = Application.persistentDataPath + "/settings";
		string filename = "/reconn.dat";

		if(File.Exists(filepath+filename)){
			File.Delete(filepath+filename);
		}
	}

	private static reconnectData readReconnectFile(){
		string filepath = Application.persistentDataPath + "/settings";
		string filename = "/reconn.dat";

		if( Directory.Exists(filepath) && File.Exists(filepath+filename) ){
			byte[] readData = File.ReadAllBytes(filepath+filename); //Read the bytes from the saved file.

			byte[] bytesDecompressed = Decompress(readData); //Decompress the binary data.
			string json = Encoding.ASCII.GetString(bytesDecompressed); //Convert the full binary data to a string JSON.
			reconnectData reconn = JsonConvert.DeserializeObject<reconnectData>(json, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None}); //Parse the JSON into a object.
			
			return reconn;
		} else {
			return new reconnectData();
		}
	}

	public static bool hasReconnectFile{
		get {
			string filepath = Application.persistentDataPath + "/settings";
			string filename = "/reconn.dat";
			bool fileExists = Directory.Exists(filepath) && File.Exists(filepath+filename);
			
			return fileExists;
		}
	}

	public static bool isReconnectFileValid{
		get {
			string filepath = Application.persistentDataPath + "/settings";
			string filename = "/reconn.dat";
			bool fileExists = Directory.Exists(filepath) && File.Exists(filepath+filename);
			
			reconnectData reconn = readReconnectFile();
			if(reconn.isExpired){ //Delete the reconnection file if it is expired. Maybe more this somewhere else later...
				deleteReconnectFile();
			}
			
			return fileExists && !reconn.isExpired;
		}
	}

}