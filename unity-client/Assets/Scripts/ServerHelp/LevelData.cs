//using UnityEngine;
using Newtonsoft.Json;

//Note: this is just shadow of the server side class, which actually uses inheritance. Only these properties are needed on the client side.
public class syncLevelData {
	public int playersLeft;
	public int playersMax;
	public Vector3Thin safeCenter;
	public float safeDist;
	public float circleUpdateTimer;
	public Vector3Thin NextSafeCenter;
	public float NextSafeDist;
}

public class syncLobbyData {
	public float lobbyStartTimer;
	public int playersLeft;
	public int playersMax;
}


public class syncEndedData {
	public string winner;
}

public class LevelDataClient{
	public enum LvlType {Lobby, GameLvl, Ended};
	public LvlType lvlType;
	public string JSONLvl;
	
	public LevelDataClient(){
		lvlType = LvlType.Lobby;
		JSONLvl = "";
	}

	public bool inLobby{
		get{
			return (lvlType == LvlType.Lobby);
		}
	}
	public bool inGame{
		get{
			return (lvlType == LvlType.GameLvl);
		}
	}
	public bool inEnded{
		get{
			return (lvlType == LvlType.Ended);
		}
	}

	public syncLobbyData getLobby(){
		return JsonConvert.DeserializeObject<syncLobbyData>(JSONLvl, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
	}
	public syncLevelData getGameLvl(){
		return JsonConvert.DeserializeObject<syncLevelData>(JSONLvl, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
	}
	public syncEndedData getEndedData(){
		return JsonConvert.DeserializeObject<syncEndedData>(JSONLvl, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
	}
}