using System;

public class reconnectData {
	public int gameID;
	public int syncID;
	public string urlPrefix;
	public long saveTime; //The time at which the "reconnectData" was made in unix time.

	public reconnectData(){
		gameID = -1;
		syncID = -1;
		urlPrefix = "";
		saveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - 10; //Set the saveTime to something in the past so "this.isExpired == true".
	}

	public reconnectData(int gameIDIn, int syncIDIn, string urlPrefixIN) {
		gameID = gameIDIn;
		syncID = syncIDIn;
		urlPrefix = urlPrefixIN;
		saveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds(); //Set the save time as the current system time.
	}

	public bool isExpired {
		get{
			float timeLim = 10 * 60; //The expire time in seconds.
			return (DateTimeOffset.Now.ToUnixTimeMilliseconds() - saveTime)/1000.0f > timeLim;
		}
	}
}