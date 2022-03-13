using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TempRoundData {

	//This class is a holder that will contain vars from the server that will be ready by the local to help game play.

	public static Vector3Thin safeCenter; //Maybe later move these to a static class that just hold temp round data...
	public static float safeDist;
	public static Vector3Thin NextSafeCenter;
	public static float NextSafeDist;

	//Work around for now...
	public static float zoneTime;
	public static float maxZoneTime;

	public static Vector3 dangerCenter; //These are not sent from the server, but calculated locally and cached here.
	public static float dangerRadius; //They are calculated in the "ZoneCtrl" script.


	public static readonly float compressionZoneDamage = 5.0f;

	public static MapData map; //Maybe move map out of GameSync and into here only later...
	
	public static GameObject playerLowResCam;
	public static int camResMin;
	public static int camResMax;

	
	//Test vars to check against for different flags.
	public static bool serverHasSetSafeZone{
		get{
			if(safeCenter == null){ return false;} //return early if the pos is not set!

			Vector3Thin defaultVert = new Vector3Thin(-1,-1,-10);
			bool vertsSame = safeCenter.x == defaultVert.x && safeCenter.y == defaultVert.y && safeCenter.z == defaultVert.z;
			return !vertsSame;
		}
	}


	public static void reset() {
		safeCenter = new Vector3Thin(-1,-1,-10); //A bogus value so it will immediately cache a new version from the server.
		safeDist = 9999.0f; //Make sure the whole map starts in the safe zone!
		NextSafeCenter = new Vector3Thin(-1,-1,-10);
		NextSafeDist = 9999.0f;

		dangerCenter = Vector3.zero;
		dangerRadius = 99990.0f;

		map = new MapData();

		playerLowResCam = GameObject.FindGameObjectWithTag("playerLowResCam");
		camResMin = 100;
		camResMax = 250;
	}

	public static bool inLobby {
		get{
			return serverCtrl.levelState == LevelDataClient.LvlType.Lobby;
		}
	}
}
