using static Backend.GameObj.StringFormat; //For using the backends's string format functions...
//using Newtonsoft.Json;

//[JsonObject]
public class ServerInfo { //This is because "ExpandoObject" was not working for iOS...
	public string clientVer;
	public string maintenanceMessage;
	public ServerInfo(string clientVerIN, string maintenanceMessageIN){
		clientVer = clientVerIN;
		maintenanceMessage = maintenanceMessageIN;
	}

	public ServerInfo(): this("", "") {} //iOS wants public constructor that don't take any parameters...
}

class responseMessage {
	public bool success;
	responseMessage(){
		this.success = false;
	}
}


public class OnlineCount { //Changed to public for iOS, and public constructor.
	public int onlinePlayers;
	public OnlineCount(){
		onlinePlayers = 0;
	}
}


public class Connection {
	public int gameID;
	public int syncID;
	public Connection(){
		this.gameID = -1;
		this.syncID = -1;
	}
}


public class syncServerData {
	public playerObj[] players;
	public bulletObj[] bullets;
	public LevelDataClient lvlSync; //public syncLevelData lvlSync;

	public syncServerData(){
		players = new playerObj[0]; //set zero values for no parameters. This is fine because the server data is parse into one of these and will not use this constructor. (Maybe use two constructors later)...
		bullets = new bulletObj[0];
	}

	playerObj getPlayer(int ind){
		return players[ind];
	}

	bulletObj getBullet(int ind){
		return bullets[ind];
	}

	public playerObj[] getPlayers(){
		return players;
	}

	public bulletObj[] getBullets(){
		return bullets;
	}
}


public class sendClientData {
	public int gameID;
	public int syncID;

	public string localPlayer;
	public string localBullets;


	public sendClientData(int gameIDIn, int syncIDIn) { //This is for when the the code needs to request data from the server, but not update the client data on the server...
		gameID = gameIDIn;
		syncID = syncIDIn;
		localPlayer = "";
		localBullets = "";
	}

	public sendClientData(int gameIDIn, int syncIDIn, playerObj playerObjIn, bulletObj[] bulletObjsIn) {
		gameID = gameIDIn;
		syncID = syncIDIn;

		localPlayer = playerObjIn.ToString(); //"Replace" must be called here because this string JSON will be nested inside another JSON.
		localBullets = convertBullets(bulletObjsIn);
	}


	string convertBullets(bulletObj[] bulletObjsIn) {
		if (bulletObjsIn.Length == 0) {
			return "";
		}

		string str = ""; //Don't put the bracket so the server has less string work to do!
		for (int i = 0; i < bulletObjsIn.Length; i++) {
			if (i == bulletObjsIn.Length - 1) {
				str += bulletObjsIn[i].ToString(); //Don't put a comma behind the last index in the array!
			} else {
				str += bulletObjsIn[i].ToString() + ",";
			}
		}
		return str; //Don't put the bracket so the server has less string work to do!
	}

}