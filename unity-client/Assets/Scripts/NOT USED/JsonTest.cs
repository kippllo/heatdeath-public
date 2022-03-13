using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json; //Added from imported .dll     
//	Downloaded from: https://github.com/JamesNK/Newtonsoft.Json/releases OR https://www.newtonsoft.com/json
//	Import help: https://docs.unity3d.com/Manual/UsingDLL.html
//	https://stackoverflow.com/questions/3142495/deserialize-json-into-c-sharp-dynamic-object


//public class test{
//	public LevelDataClient.LvlType lvlMode;
//	public string str;
//}


public class JsonTest : MonoBehaviour {

	/*
	int getSlashCount(int lvlOfNest2){
		if (lvlOfNest2 == 0){ return 0; }

		int count = getSlashCount(lvlOfNest2-1)*2 +1;
		return count;
	}

	//Double lvlOfNest plus one = how many slashes before the quote.
	public string nestQ(int lvlOfNest){ //nestedQuote

		string q = "";
		
		//lvlOfNest -= 1; //In my algorithm, "lvlOfNest*2 +1" will give the slash count for the next nested level. So just minus one here.
		//int slashCount = lvlOfNest*2 +1;
		int slashCount = getSlashCount(lvlOfNest);

		for(int i=0; i<slashCount; i++){
			q += "\\"; //Add a slash for each
		}

		q += "\""; //Actually add the quote mark.
		return q;
	}
	*/


	float radius = 10;
	Color32 col = new Color32(255,255,255,255);
	DrawMesh canvas = new DrawMesh();

	Vector3 centerStart = Vector3.zero;
	Vector3 centerEnd = new Vector3(-10,10,0);

	GameObject oldMesh;

	float timer;
	public float timerMax = 1.0f;
	public int segmentCount = 20;

	// Use this for initialization
	void Start () {
		
		
		//DrawMesh canvas = new DrawMesh();

		//Vector3 pos1 = new Vector3(0,0.5f,0);
		//Vector3 pos2 = new Vector3(-1,-4,0);
		//Vector3 pos3 = new Vector3(0.5f,0,0);

		/*
		Vector3 pos1 = new Vector3(0,0,0);
		Vector3 pos3 = new Vector3(0,1,0);
		Vector3 pos2 = new Vector3(1,0,0);

		Vector3 pos2 = new Vector3(0,0,0);
		Vector3 pos1 = new Vector3(0,1,0);
		Vector3 pos3 = new Vector3(1,0,0);
		

		Vector3 pos2 = new Vector3(0,0,0);
		Vector3 pos3 = new Vector3(0,1,0);
		Vector3 pos1 = new Vector3(1,0,0);

		
		Vector3 pos3 = new Vector3(0,0,0);
		Vector3 pos2 = new Vector3(0,1,0);
		Vector3 pos1 = new Vector3(1,0,0);

		Vector3 pos3 = new Vector3(0,0,0);
		Vector3 pos1 = new Vector3(0,1,0);
		Vector3 pos2 = new Vector3(1,0,0);

		*/
		
		//canvas.DrawTriangle(pos1, pos2, pos3);

		//Vector3 pos0 = new Vector3(0,0,0);
		//Vector3 pos1 = new Vector3(25,10,0);

		//Vector3 pos0 = new Vector3(15,15,0);
		//Vector3 pos1 = new Vector3(-10,10,0);

		//Color32 col = new Color32(255,255,255,255);
		
		//canvas.segmentCount = segmentCount; //20; //6;
		oldMesh = new GameObject();
		
		//canvas.DrawPixelCircle(pos1, 25, 5, 5, col);
		//canvas.DrawCircleOutline(pos0, 10, 5, col);

		//canvas.DrawSquareWithCircleHole(pos0, 50, 50, pos1, 10, col);
		//canvas.DrawSquareWithCircleHole(pos0, 10, 10, pos1, 5, col);
		
		//canvas.RenderToGameObject();

		/*int[] test = new int[3] {0,1,2};
		string str = JsonConvert.SerializeObject(test, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		Debug.Log(str);

		int[] deser = JsonConvert.DeserializeObject<int[]>(str);
		Debug.Log(deser[2]);*/

		/*
		MapData map = new MapData();
		map.width = 100;
		map.height = 100;

		map.cubes = new cube[3];
		map.cubes[0] = new cube(1,1,1, 1,1,1, 0,255,0,255);
		map.cubes[1] = new cube(10,5,20, 5,5,5, 0,0,255,255);
		map.cubes[2] = new cube(-11,-11,-11, 11,11,11, 0,0,255,100);

		map.GenMap();
		*/

		/*
		cube c = new cube(0,0,0, 0.5f,0.5f,0.5f);
		MapData map = new MapData();

		GameObject plane = new GameObject(); //Note: New GameObject always spawns at (0,0,0).
		plane.AddComponent<MeshFilter>();
		plane.GetComponent<MeshFilter>().mesh = map.GenCube(c);
		plane.AddComponent<MeshRenderer>();
		*/
		


		//string str = "{ \"uploadSuccess\": true, \"dataSync\": \"{ 'players': [], 'bullets': [], 'lvlSync': {\\\\\"lvlType\\\\\":0,\\\\\"JSONLvl\\\\\":\\\\\"{\\\\\\\"lobbyStartTimer\\\\\\\":1.881}\\\\\"}}\" }";
		//syncPackage resPackage = JsonConvert.DeserializeObject<syncPackage>(str);
		//Debug.Log("HERE");

		/*
		Debug.Log("START");
		Debug.Log(nestQ(0));
		Debug.Log(nestQ(1));
		Debug.Log(nestQ(2));
		Debug.Log(nestQ(3));
		Debug.Log(nestQ(4));
		Debug.Log(nestQ(5));
		Debug.Log(nestQ(6));
		Debug.Log("END");


		test nested = new test();
		nested.lvlMode = LevelDataClient.LvlType.GameLvl;
		//nested.str = "\"Hello!\"";
		nested.str = "Hello!";
		//string nestedJson = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

		nested.str = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		nested.str = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		nested.str = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		nested.str = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		nested.str = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		string nestedJson = JsonConvert.SerializeObject(nested, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

		syncPackage obj = new syncPackage();
		obj.dataSync = nestedJson;
		string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		Debug.Log("json: " + json);

		//string str = "{ \"uploadSuccess\": true, \"dataSync\": \"{ 'players': [], 'bullets': [], 'lvlSync': {\\\\\"lvlType\\\\\":0,\\\\\"JSONLvl\\\\\":\\\\\"{\\\\\\\"lobbyStartTimer\\\\\\\":1.881}\\\\\"}}\" }";
		//string str = "{ \"uploadSuccess\": true, \"dataSync\": \"{ 'players': [], 'bullets': [], 'lvlSync': {'lvlType':0,'JSONLvl': '{\\\\\\\"lobbyStartTimer\\\\\\\":1.881}'}}\" }";

		//string str = $@"{{ {nestQ(0)}uploadSuccess{nestQ(0)}: true, {nestQ(0)}dataSync{nestQ(0)}: {nestQ(0)}{{ {nestQ(1)}players{nestQ(1)}: [], {nestQ(1)}bullets{nestQ(1)}: [], {nestQ(1)}lvlSync{nestQ(1)}: {{{nestQ(2)}lvlType{nestQ(2)}:0,{nestQ(2)}JSONLvl{nestQ(2)}: {nestQ(2)}{{ {nestQ(3)}lobbyStartTimer{nestQ(3)}:1.881}} {nestQ(2)} }}}} {nestQ(0)} }}";
		string str = "{" + nestQ(0) + "uploadSuccess" + nestQ(0) + ": true, \"dataSync\": \"{ 'players': [], 'bullets': [], 'lvlSync': {\\\\\"lvlType\\\\\":0,\\\\\"JSONLvl\\\\\":\\\\\"{\\\\\\\"lobbyStartTimer\\\\\\\":1.881}\\\\\"}}\" }";
		Debug.Log("str: " + str);
		syncPackage resPackage = JsonConvert.DeserializeObject<syncPackage>(str);
		Debug.Log("HERE");

		
		//To Json-------------------
		objIDTest names = new objIDTest();
		names.names.Add (0, "Jake");
		string test = JsonConvert.SerializeObject(names);
		Debug.Log (test);


		//From Json-----------------
		dynamic convObj = JsonConvert.DeserializeObject(test);
		Debug.Log(convObj);    //print( convObj.GetType() );
		Debug.Log(convObj.names["0"]);
		//Debug.Log(JsonUtility.ToJson(names.names));


		//From Json (non-dynamic)-----------------
		objIDTest convObjNonDyna = JsonConvert.DeserializeObject<objIDTest>(test);
		Debug.Log(convObjNonDyna);
		Debug.Log(convObjNonDyna.names[0]);


		vectTest v = new vectTest();
		v.pos.x = 55;
		//test = JsonConvert.SerializeObject(v); //Doesn't work with vector3 bc it has a another normalized vertor3 inside of it. Instead use the line below.
		test = JsonConvert.SerializeObject (v, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); //Help: https://stackoverflow.com/questions/13510204/json-net-self-referencing-loop-detected        Object Initializers Help: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers
		Debug.Log(test);

		var vJson = JsonConvert.DeserializeObject<vectTest>(test);
		Debug.Log(vJson.pos.x);
		Debug.Log(vJson.pos.GetType());
		*/


		//string resString = "{\"uploadSuccess\":false,\"dataSync\":\"{ \\\"players\\\": [{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":10000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":20000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":30000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":40000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":50000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":60000}], \\\"bullets\\\": []}\"}";
		//string resString = "{\"uploadSuccess\":false,\"dataSync\":{ \\\"players\\\": [{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":10000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":20000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":30000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":40000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":50000},{\\\"hp\\\":10,\\\"position\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"rotation\\\":{\\\"x\\\":0,\\\"y\\\":0,\\\"z\\\":0,\\\"magnitude\\\":0,\\\"sqrMagnitude\\\":0},\\\"objID\\\":60000}], \\\"bullets\\\": []}}";
		//syncPackage resPackage = JsonConvert.DeserializeObject<syncPackage>(resString);


		//string syncData = "{\"players\":[{\"objID\":null,\"destroyed\":false,\"hp\":null,\"position\":null,\"rotation\":null},{\"objID\":null,\"destroyed\":false,\"hp\":null,\"position\":null,\"rotation\":null},{\"objID\":null,\"destroyed\":false,\"hp\":null,\"position\":null,\"rotation\":null},{\"objID\":null,\"destroyed\":false,\"hp\":null,\"position\":null,\"rotation\":null},{\"objID\":null,\"destroyed\":false,\"hp\":null,\"position\":null,\"rotation\":null},{\"objID\":null,\"destroyed\":false,\"hp\":null,\"position\":null,\"rotation\":null}],\"bullets\":[]}";
		//syncServerData syncDataDecode = JsonConvert.DeserializeObject<syncServerData>(syncData);

		//string syncData = "{\"players\":[],\"bullets\":[]}";
		//syncServerData syncDataDecode = JsonConvert.DeserializeObject<syncServerData>(syncData);
		//print (syncDataDecode.players.Length);


		//sendClientData clientData = new sendClientData(0, 0, new playerObj(0,0,Vector3.zero,Vector3.zero), new bulletObj[0]);
		//string json = JsonConvert.SerializeObject(clientData, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });


		//Test with nested class arrays---------------
		/*
		playerObj[] pObjs = new playerObj[3];
		pObjs[0] = new playerObj (1, Vector3.left, Vector3.left); pObjs[1] = new playerObj (2, Vector3.right, Vector3.right); pObjs[2] = new playerObj (3, Vector3.up, Vector3.up);

		bulletObj[] bObjs = new bulletObj[3];
		bObjs[0] = new bulletObj(Vector3.down, Vector3.down); bObjs[1] = new bulletObj(Vector3.zero, Vector3.zero);

		sendClientData sendData = new sendClientData (123, 99, pObjs[0], bObjs);
		string sendData_JSON = JsonConvert.SerializeObject (sendData, new JsonSerializerSettings(){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		Debug.Log(sendData_JSON);

		sendClientData sendData_FromJson = JsonConvert.DeserializeObject<sendClientData>(sendData_JSON);
		Debug.Log(sendData_FromJson.localPlayer.position);
		Debug.Log(sendData_FromJson.localBullets[0].position);
		Debug.Log(sendData_FromJson.localBullets[2]); //Null
		*/

		/*
		serverCtrl.syncID = 1; //debug
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		Debug.Log("getNextObjID: " + serverCtrl.getNextObjID());
		*/
	}
	
	// Update is called once per frame
	void Update () {

		
		timer += Time.deltaTime;
		if(timer >= timerMax){
			timer -= timerMax;

			float lerpNumb = 0.01f;
			canvas.segmentCount = segmentCount;
			
			radius = Mathf.Lerp(radius, 0.1f, lerpNumb);

			Vector3 pos0 = new Vector3(15,15,0);
			//Vector3 pos1 = new Vector3(-10,10,0);

			centerStart = Vector3.Lerp(centerStart, centerEnd, lerpNumb);

			
			Destroy(oldMesh);
			canvas.clear();

			canvas.DrawSquareWithCircleHole(pos0, 50, 50, centerStart, radius, col);
			oldMesh = canvas.RenderToGameObject();
		}


		/*
		canvas.clear();
		//float lerpAmount = 0.5f; //TempRoundData.zoneTime/TempRoundData.maxZoneTime;
		TempRoundData.dangerRadius = 30.0f; //Mathf.Lerp(TempRoundData.NextSafeDist, TempRoundData.safeDist, lerpAmount);
		TempRoundData.dangerCenter = Vector3.zero; //Vector3.Lerp(TempRoundData.NextSafeCenter.ToVector3(), TempRoundData.safeCenter.ToVector3(), lerpAmount);
		canvas.DrawSquareWithCircleHole(Vector3.zero, 100, 100, TempRoundData.dangerCenter, TempRoundData.dangerRadius, new Color32(255,0,0,150) );
		var dangerZoneGameObject = canvas.RenderToGameObject();
		dangerZoneGameObject.layer = 10; //Set the layer to render on both the mini map and normal player camera.

		canvas.clear();
			//Draw the next saftey circle only once per timer reset.
		float outlineThickness = 1;// TempRoundData.NextSafeDist * -0.025f; //Outline thickness is 5% of the outline circle radius. Negative os that the outline thickness goes towards the inside of the cirlce instead of expanding the circle outward...
		outlineThickness = Mathf.Clamp(outlineThickness, -8, -4);
		
		canvas.DrawCircleOutlineInBox(Vector3.zero, 100, 100, TempRoundData.dangerCenter, TempRoundData.dangerRadius/2, outlineThickness, new Color32(0,0,255,150));
		
		var safeZoneGameObject = canvas.RenderToGameObject(); //render the safe zone to a different game Object than the dangerZone so that the danger zone can be shown in the game world.
		safeZoneGameObject.layer = 12; //Set the layer to only render on the mini map.
		*/

	}
}


class objIDTest {
	public Dictionary<int, string> names;

	public objIDTest(){
		names = new Dictionary<int, string>();
	}
}


class vectTest {
	public Vector3 pos;

	public vectTest(){
		pos = Vector3.zero;
	}
}