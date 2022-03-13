using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.IO.Compression;
using System;

public class EditorCtrl : MonoBehaviour {


	public float gridStep;
	public static Color32Thin color;
	public Vector3Thin size;
	public static Vector3Thin originalSize;
	
	public GameObject backdrop;
	public static int width;
	public static int height;

	public GameObject templateCubeOutline;
	public GameObject templateCubeWorld;
	public Material cubeMat;

	private List<GameObject> cubes = new List<GameObject>();
	private List<GameObject> spawnPoints = new List<GameObject>();
	enum PlaceItem {Cube, SpawnPoint};
	PlaceItem placeItem = PlaceItem.Cube;

	private GameObject outlineObj;
	public Camera EditorCam;

	private bool gridOn = true;
	public static bool blnBuildMode;

	public int CubesCount{
		get {
			return cubes.Count;
		}
	}

	void Start () {
		gridStep = 1;
		color = new Color32Thin(255,255,255,255);
		originalSize = new Vector3Thin(1,1,1);
		size = new Vector3Thin(originalSize.x*gridStep,originalSize.y*gridStep,originalSize.z*gridStep);
		blnBuildMode = true;

		width = 100;
		height = 100;
		//UNDO//genBackdrop();
		
		//Spawn a visable cursor for the player...
		genCursor();


		//Gen a Foundation Cube
		//GenCubeGameObject(new cube(0,0,0, 1,1,1, 255,255,255,255) );

		//Load map save file
		MapData mapData = readMapFile();
		renderMapFromData(mapData);
	}
	
	void Update () {

		//Refactor the size: //MOVE LATER MAYBE!!!!!!!!!
		size = new Vector3Thin(originalSize.x*gridStep,originalSize.y*gridStep,originalSize.z*gridStep);

		//Draw the build cursor...
		RaycastHit pointRay1;
		if (Physics.Raycast(EditorCam.ScreenPointToRay(Input.mousePosition), out pointRay1, Mathf.Infinity) &&blnBuildMode){ //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out pointRay1, Mathf.Infinity, layerMaskRegColliderOut)){
			//Clear old frame's obj...
			Destroy(outlineObj);


			Mesh hitMesh = pointRay1.collider.gameObject.GetComponent<MeshFilter>().mesh;
			Vector3[] verts = hitMesh.vertices;
			int[] tris = hitMesh.triangles;
			Vector3[] norms = hitMesh.normals;

			Vector3 correctionVectorReCal;

			//Method for Normal calculation found here: https://www.khronos.org/opengl/wiki/Calculating_a_Surface_Normal
			try{
				Vector3[] hitTri = new Vector3[] {verts[tris[pointRay1.triangleIndex*3]], verts[tris[pointRay1.triangleIndex*3 + 1]], verts[tris[pointRay1.triangleIndex*3 + 2]]};
				Vector3 U = new Vector3 (hitTri[1].x - hitTri[0].x, hitTri[1].y - hitTri[0].y, hitTri[1].z - hitTri[0].z);
				Vector3 V = new Vector3 (hitTri[2].x - hitTri[0].x, hitTri[2].y - hitTri[0].y, hitTri[2].z - hitTri[0].z);
				correctionVectorReCal = new Vector3 ((U.y * V.z) - (U.z * V.y), (U.z * V.x) - (U.x * V.z), (U.x * V.y) - (U.y * V.x));
			} catch (IndexOutOfRangeException err) {
				//	Note: For some reason this error will fire like 1/8 times the scene loads. Only in the first frame.
				//		I think it has something to do with "verts[tris[pointRay1.triangleIndex*3]]" some how not having the correct indices 
				//		because an object is in the process of being deleted. Like maybe the first backdrop or something...
				//		For now just catch the error here... Maybe fix later...
				Debug.Log(err);
				correctionVectorReCal = new Vector3(1,0,0); //Just make up a vector to go off of as a work around...
			}

			////float ObjSizeFromOrgin = gridStep/2; //float ObjSizeFromOrgin = 0.5f;
			//float ObjSizeFromOrgin = 1; //DEBUG remove this line!

			//worldCube cubeScript = pointRay1.collider.gameObject.GetComponent<worldCube>();
			
			float sign = (getActiveAxis(correctionVectorReCal) >= 0) ? 1 : -1;
			int axisInd = getActiveAxisInd(correctionVectorReCal);
			
			Vector3 distToAdd = Vector3.zero; ////(sign*cubeScript.size/2); //Vector3 distToAdd = (-sign*cubeScript.size/2); //Half of the "looked at" cube's size.
			//JUST ADD THE BELOW LINE!
			distToAdd += ( sign*size.ToVector3()/2 ); //distToAdd += ( -sign*size.ToVector3()/2 ); //Half of the size of the cube we are placing...
			

			Vector3 dist = Vector3.zero;
			dist[axisInd] += distToAdd[axisInd];
			
			correctionVectorReCal = addToActiveAxis(correctionVectorReCal, distToAdd); //Subtract half the "looked at" cube's size from the distance to the new cube.


			/* START HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! THE LINE BELOW!! */
			Vector3 fixedPos = pointRay1.point; //pointRay1.collider.transform.position; // + dist;
			fixedPos[axisInd] += dist[axisInd];
			////fixedPos[axisInd] = dist[axisInd];
			//UNDO MAYBE//Vector3 fixedPos = pointRay1.point + (correctionVectorReCal * ObjSizeFromOrgin);
			
			if(gridOn){
				fixedPos = gridRound(fixedPos, gridStep);
			}
			

			Color32Thin outlineColor = new Color32Thin(0, 0, 0, 100); //Color32Thin outlineColor = new Color32Thin(color.r, color.b, color.g, 100);
			//outlineColor.a = 100; //Make the ourline color transparent
			cube outlineCube = new cube(fixedPos.x,fixedPos.y,fixedPos.z, size.x,size.y,size.z, outlineColor);

			outlineObj = GenCubeGameObject(outlineCube); //outlineObj = Instantiate(templateCubeOutline, gridRound(fixedPos, gridStep), Quaternion.identity);
			outlineObj.GetComponent<MeshCollider>().enabled = false;
			

			if (Input.GetMouseButtonDown(0) && outlineObj && blnBuildMode) {

				if(placeItem == PlaceItem.Cube){
					cube worldCube = new cube(0,0,0, 1,1,1, color);
					worldCube.x = fixedPos.x;
					worldCube.y = fixedPos.y;
					worldCube.z = fixedPos.z;
					worldCube.sizeX = size.x;
					worldCube.sizeY = size.y;
					worldCube.sizeZ = size.z;

					GameObject cubeObj = GenCubeGameObject(worldCube);
					cubes.Add(cubeObj);
				} else if(placeItem == PlaceItem.SpawnPoint){
					spawnPoints.Add( GenSpawnPointGameObject(fixedPos) );
				}
			}

			if(Input.GetMouseButtonDown(1) && blnBuildMode){
				
				/*
				if(placeItem == PlaceItem.Cube){
					cubes.Remove(pointRay1.collider.gameObject);
				} else if(placeItem == PlaceItem.SpawnPoint){
					spawnPoints.Remove(pointRay1.collider.gameObject);
				}
				*/

				//Just go ahead and try to find the object in both list. It will only be found in one...
				cubes.Remove(pointRay1.collider.gameObject);
				spawnPoints.Remove(pointRay1.collider.gameObject);

				Destroy(pointRay1.collider.gameObject);
			}

			//Add spawnPoint placement
			//Add cube & spawnPoint removal.
		} else{
			//Clear the outline cube...
			Destroy(outlineObj);

			genBackdrop(); //Maybe mvoe...
		}


		//Scrolling adjust grid size...
		float inputScroll = Input.GetAxis ("Mouse ScrollWheel");
		if(inputScroll != 0){
			gridStep += (inputScroll > 0) ? 0.1f : -0.1f; //Add pos or neg depending on the user scroll.
			gridStep = Mathf.Clamp(gridStep, 0.1f, 100.0f); //Limit the max and min grid size.

			size = new Vector3Thin(originalSize.x*gridStep,originalSize.y*gridStep,originalSize.z*gridStep);
			//size = new Vector3Thin(size.x*gridStep, size.y*gridStep, size.z*gridStep);
			//Debug.Log("gridStep: " + gridStep);
		}

		if(Input.GetMouseButtonDown(2)){
			gridOn = !gridOn;
		}

		if(Input.GetKeyDown("p")){
			placeItem = (placeItem == PlaceItem.Cube) ? PlaceItem.SpawnPoint : PlaceItem.Cube; 
		}
		

	}



	//Save map file methods start------------------------------------------------------
	void renderMapFromData(MapData mapData){

		//Clear any old/current cubes in editor world:
		for(int i=cubes.Count-1; i>=0; i--){
			Destroy(cubes[i]);
		}
		cubes = new List<GameObject>(); //Make sure the var is cleared.

		//Do the same for spawn points:
		for(int i=spawnPoints.Count-1; i>=0; i--){
			Destroy(spawnPoints[i]);
		}
		spawnPoints = new List<GameObject>(); 

		//This renders all blocks in edit mode instead of the play mode that "MapData.GenMap()" would return.
		for(int i=0; i<mapData.cubes.Length; i++){
			GameObject cubeObj = GenCubeGameObject(mapData.cubes[i]);
			cubes.Add(cubeObj);
		}

		for(int i=0; i<mapData.spawnPoints.Length; i++){
			GameObject spawnPointObj = GenSpawnPointGameObject( mapData.spawnPoints[i].ToVector3() );
			spawnPoints.Add(spawnPointObj);
		}

		//Set map size
		width = mapData.width;
		height = mapData.height;
		genBackdrop();
	}

	public void saveMapData(){
		MapData mapData = exportMap();
		saveMapFile(mapData);
	}

	MapData exportMap(){
		MapData export = new MapData();
		
		//Export cubes
		export.cubes = new cube[cubes.Count];
		for(int i=0; i<export.cubes.Length; i++){
			export.cubes[i] = cubes[i].GetComponent<worldCube>().ToCube();
		}

		//Export spawn points
		export.spawnPoints = new Vector3Thin[spawnPoints.Count];
		for(int i=0; i<export.spawnPoints.Length; i++){
			export.spawnPoints[i] = Vector3Thin.FromVector3( spawnPoints[i].transform.position );
		}

		//Export Map dimensions.
		export.width = width;
		export.height = height;
		
		return export;
	}

	public void saveMapFile(MapData mapData) {
		string filepath = Application.persistentDataPath + "/map";
		string filename = "/map.dat";

		if(!Directory.Exists(filepath)){
			Directory.CreateDirectory(filepath);
		}

		string json = JsonConvert.SerializeObject(mapData, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); //Convert the class into a JSON string.
		byte[] strBytes = Encoding.ASCII.GetBytes(json); //Get the string's binary data.
		byte[] compressedBytes = Compress(strBytes); //Compress the binary data so the file size will be smaller and user won't be able to read it.

		File.WriteAllBytes(filepath+filename, compressedBytes);
	}

	private void saveMapJSONFile(MapData mapData) {
		string filepath = Application.persistentDataPath + "/map";
		string filename = "/map.json";

		if(!Directory.Exists(filepath)){
			Directory.CreateDirectory(filepath);
		}

		string json = JsonConvert.SerializeObject(mapData, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); //Convert the class into a JSON string.
		byte[] strBytes = Encoding.ASCII.GetBytes(json); //Get the string's binary data.
		
		//Just skip the compression step!!

		File.WriteAllBytes(filepath+filename, strBytes);
	}

	public static MapData readMapFile(){
		string filepath = Application.persistentDataPath + "/map";
		string filename = "/map.dat";

		if( Directory.Exists(filepath) && File.Exists(filepath+filename) ){
			byte[] readData = File.ReadAllBytes(filepath+filename); //Read the bytes from the saved file.

			byte[] bytesDecompressed = Decompress(readData); //Decompress the binary data.
			string json = Encoding.ASCII.GetString(bytesDecompressed); //Convert the full binary data to a string JSON.
			
			MapData mapData = JsonConvert.DeserializeObject<MapData>(json, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None}); //Parse the JSON into a object.

			//File.WriteAllBytes(filepath+"/map.json", bytesDecompressed); //This is for debugging, uncomment to write a plain-text JSON of the map.
			
			return mapData;
		} else {
			return new MapData();
		}
	}

	public static MapData readMapFileJSON(string jsonPath){
		// Maybe add the two below line back later.
		// For now we are just always loading: Application.persistentDataPath + "/map/map.json". Which is first passed into "renderMapFromJSON()"
		// Which is called in EditorUI.cs
		//
		//string filepath = Application.persistentDataPath + "/map";
		//string filename = "/map.dat";

		if( File.Exists(jsonPath) ){
			byte[] readData = File.ReadAllBytes(jsonPath); //Read the bytes from the saved file.
			string json = Encoding.ASCII.GetString(readData); //Convert the full binary data to a string JSON.
			MapData mapData = JsonConvert.DeserializeObject<MapData>(json, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None}); //Parse the JSON into a object.
			return mapData;
		} else {
			return new MapData();
		}
	}

	public void renderMapFromJSON(string jsonPath){
		MapData jsonData = EditorCtrl.readMapFileJSON(jsonPath);
		renderMapFromData(jsonData);
	}

	public void exportJSONMapData(){
		MapData mapData = exportMap();
		saveMapJSONFile(mapData);
	}

	static byte[] Compress(byte[] data) {
		MemoryStream output = new MemoryStream();
		using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
		{
			dstream.Write(data, 0, data.Length);
		}
		return output.ToArray();
	}

	static byte[] Decompress(byte[] data) {
		MemoryStream input = new MemoryStream(data);
		MemoryStream output = new MemoryStream();
		using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
		{
			dstream.CopyTo(output);
		}
		return output.ToArray();
	}
	//Save map file methods end------------------------------------------------------



	public void resetWorld(){
		//Clear any old/current cubes in editor world:
		for(int i=cubes.Count-1; i>=0; i--){
			Destroy(cubes[i]);
		}
		cubes = new List<GameObject>(); //Make sure the var is cleared.

		MapData mapData = new MapData();

		//Set map size???
		mapData.width = 100;
		mapData.height = 100;
		
		renderMapFromData(mapData);
	}


	public void genBackdrop(){
		//Clear the old backdrop object.
		Destroy(backdrop);

		//Render a new one.

		// Note: There is some triangle/normals problem that makes the camera clip the cube at certain angles.
		//		For that reason it is easier to use one of unity's cube for a the backdrop. Their cube won't have this problem.
		//cube backdropCube = new cube(0,0,0, width,height,1, 255,255,255,255);
		//backdrop = GenCubeGameObject(backdropCube);

		backdrop = GameObject.CreatePrimitive(PrimitiveType.Cube); //Spawn a cube. Same as: "Right Click->3D Object->Cube" in editor.
		backdrop.transform.localScale = new Vector3(width, height, 1);
		backdrop.transform.position = new Vector3(0,0,1); //Make sure the backdrop is Z=1 so the cubes will be place on the same Z plane as the player.

        Material mat = new Material(Shader.Find("Custom/VertColorOutline")); //Be sure that "Custom/VertColor" shader is included in the "Always Included Shaders" array in "ProjectSettings/Graphics". Help: https://docs.unity3d.com/ScriptReference/Shader.Find.html
		mat.SetFloat("_Outline", 0); //Set the outline to nothing, so just the cube will render...
		mat.SetColor("_Color", new Color(0,0,0,1) );
		mat.SetFloat("_Glossiness", 0);
		mat.SetFloat("_Metallic", 0);
		backdrop.GetComponent<MeshRenderer>().material = mat;
		Destroy( backdrop.GetComponent<BoxCollider>() ); //Remove the old box collider...
		backdrop.AddComponent<MeshCollider>();
		backdrop.name = "BackDrop";

	}


	GameObject genCursor(){
		//Vector3 cursorPoint = EditorCam.ScreenToWorldPoint(Input.mousePosition);
		GameObject cursor = new GameObject("cursor"); //Note: New GameObject always spawns at (0,0,0).
		cursor.transform.SetParent(gameObject.transform); //Adjust position to match parent...
		cursor.transform.localPosition = new Vector3(0,0,0);
		
		Vector3 cursorPoint = cursor.transform.localPosition; //Vector3 cursorPoint = cursor.transform.position;
		cursorPoint.z = cursorPoint.z + 0.35f;
		

        //Gen Mesh
        List<Mesh> meshes = new List<Mesh>();

		
		float length = 0.001f; //0.005f; //0.01f; //0.0025f; //0.0075f; //offset*2; //1.0f; //2*offset
		float offset = length/2; //0.003f; //0.5f;
		float thickness = 0.01f;
		//Color32Thin cursorColor = new Color32Thin(255,0,0,255);

		cube dot = new cube(); //Set floats without constructor to avoid weird round error I made with "(float)Math.Round(xIN, 2);".
		dot.x = cursorPoint.x-offset;
		dot.y = cursorPoint.y + + 0.0025f; //length/4;
		//Debug.Log(dot.y + "=0.0025");
		dot.z = cursorPoint.z-offset;
		dot.sizeX = length;
		dot.sizeY = length;
		dot.sizeZ = thickness;
		dot.setAllVertColor(255,0,0,255);
		meshes.Add( MeshLib.GenCube(dot) );

		//cube horBar = new cube(cursorPoint.x-offset,cursorPoint.y,cursorPoint.z, length*2, length, thickness, cursorColor);
        //cube vertBar = new cube(cursorPoint.x,cursorPoint.y-offset,cursorPoint.z, length, length*2, thickness, cursorColor);
        //meshes.Add( GenCube(horBar) );
        //meshes.Add( GenCube(vertBar) );

		//Consolidate meshes
        Mesh consolMesh = MeshLib.consolidateMeshes(meshes);

		//Attach Mesh to gameObject.
		cursor.AddComponent<MeshFilter>().mesh = consolMesh;
		////cursor.AddComponent<MeshRenderer>(); //testing...
        Material mat = new Material(Shader.Find("Custom/VertColorOutline")); //Be sure that "Custom/VertColor" shader is included in the "Always Included Shaders" array in "ProjectSettings/Graphics". Help: https://docs.unity3d.com/ScriptReference/Shader.Find.html
		mat.SetFloat("_Outline", 0); //Set the outline to nothing, so just the cube will render...
		cursor.AddComponent<MeshRenderer>().material = mat;
		//cursor.AddComponent<MeshRenderer>().material = cubeMat;
		
		return cursor;
	}


	// This function take one Vector [like (0,0,1)], finds the axis that is not zero,
	//	And adds the adjective's value for that axis to the subject.
	Vector3 addToActiveAxis(Vector3 subject, Vector3 adjective){
		string activeAxis = "";
		Vector3 output = new Vector3(subject.x, subject.y, subject.z); //Clone the vector...
		
		activeAxis = (subject.x != 0) ? "x" : (subject.y != 0) ? "y" : (subject.z != 0) ? "z" : "";
		//Debug.Log("activeAxis: "+activeAxis);

		if(activeAxis == "x"){
			output.x += adjective.x;
		} else if(activeAxis == "y"){
			output.y += adjective.y;
		} else if(activeAxis == "z"){
			output.z += adjective.z;
		}
		//If the subject has no active axis (0,0,0) it will not be modified.
		return output;
	}

	float getActiveAxis(Vector3 subject){ //return the value of the active axis of a vector...
		string activeAxis = "";
		float output = 0;

		activeAxis = (subject.x != 0) ? "x" : (subject.y != 0) ? "y" : (subject.z != 0) ? "z" : "";

		if(activeAxis == "x"){
			output = subject.x;
		} else if(activeAxis == "y"){
			output = subject.y;
		} else if(activeAxis == "z"){
			output = subject.z;
		}
		//If the subject has no active axis (0,0,0) it will not be modified.
		return output;
	}

	int getActiveAxisInd(Vector3 subject){ //return the index of the active axis of a vector... 0=x, 1=y, 2=z, -1=none like in (0,0,0)
		int ind = -1;

		ind = (subject.x != 0) ? 0 : (subject.y != 0) ? 1 : (subject.z != 0) ? 2 : -1;
		//int axisInd = (getActiveAxis(correctionVectorReCal) == "x") ? 0 : (getActiveAxis(correctionVectorReCal) == "y") ? 1 : (getActiveAxis(correctionVectorReCal) == "z") ? 2 : -1;
		return ind;
	}


	Vector3 gridRound(Vector3 old, float gridSize){
		// Make a new vector 3 to hold the rounded position.
		Vector3 newPos = new Vector3();

		//Round X -------------------------------------------------------------------------
		//
		// Calculate what just the decimal place is.
		// Note: this format works for both positive and negative numbers.
		// 1.34 - 1 = .34
		// or
		// -1.34 - (-1) = -.34
		float oldXDecimal = old.x - (int)old.x;

		// If the decimal is positive do calculates based on that.
		if (oldXDecimal >= 0) {
			// Example: gridSize is 0.25
			// position to be rounded is 2.64.
			// What I am basically doing is first getting the ".64" only,
			// then I check how many "0.25" can come out of ".64". The answer is two times, with a remainder of ".14".
			// It is important to keep track of how many times "0.25" come out of ".64" so that we can add those back into the final answer after we know what to do with the left over ".14".
			// And what I do with the left over ".14" is simply check if it is bigger than ".25/2". Because if it is bigger than half our gridSize we round it up, else we cut it off to zero.
			float remainder = oldXDecimal % gridSize;
			int currentGridSteps = (int)(oldXDecimal/gridSize);

			// After the currentGridSteps are found, round the remaining amount up to check if it will add another step (>=gridSize/2)
			// or the remainder is too small (<gridSize/2), if this is the case do nothing and this will leave the remainder not rounded up (really rounded down).
			if (remainder >= gridSize / 2) {
				currentGridSteps++;
			}

			//Write the new position to vector 3.
			newPos.x = (int)old.x + (currentGridSteps * gridSize);
		}

		// Else if the decimal is negative do calculates based on that.
		else if (oldXDecimal < 0) {
			// This is the same as before, but these answers will be negative numbers.
			float remainder = oldXDecimal % gridSize;
			int currentGridSteps = (int)(oldXDecimal/gridSize);

			// Calculate if the remainder needs to be rounded up or down.
			// If "-0.14 <= -(0.25/2)" which is really: "-0.14 <= -0.125"
			// Then round "-0.14" up to "-0.25". (Although that "up" is really making the number more negative, it is of a higher negativity. Thus, I say "up")
			// If that is still confusing think of it this way:
			// You would round "-0.5" to be "-1.0" //Or should you round "-0.5" to be "0.0"? The jury is still out, but the TI-84 rounds "-0.5" to be "-1.0" so I will too.
			if (remainder <= -gridSize / 2) {
				currentGridSteps--; //Use minus one because the currentGridSteps will already be negative, which is the way we want it.
			}

			// Write the new position to vector 3.
			// A "+" sign is used here because the "currentGridSteps" is already a negative number and we want our position to get more negative.
			newPos.x = (int)old.x + (currentGridSteps * gridSize);
		}



		//Round Y -------------------------------------------------------------------------
		float oldYDecimal = old.y - (int)old.y;

		if (oldYDecimal >= 0) {
			float remainder = oldYDecimal % gridSize;
			int currentGridSteps = (int)(oldYDecimal/gridSize);

			if (remainder >= gridSize / 2) {
				currentGridSteps++;
			}

			newPos.y = (int)old.y + (currentGridSteps * gridSize);
		}

		else if (oldYDecimal < 0) {
			float remainder = oldYDecimal % gridSize;
			int currentGridSteps = (int)(oldYDecimal/gridSize);

			if (remainder <= -gridSize / 2) {
				currentGridSteps--;
			}

			newPos.y = (int)old.y + (currentGridSteps * gridSize);
		}



		//Round Z -------------------------------------------------------------------------
		float oldZDecimal = old.z - (int)old.z;

		if (oldZDecimal >= 0) {
			float remainder = oldZDecimal % gridSize;
			int currentGridSteps = (int)(oldZDecimal/gridSize);

			if (remainder >= gridSize / 2) {
				currentGridSteps++;
			}

			newPos.z = (int)old.z + (currentGridSteps * gridSize);
		}

		else if (oldZDecimal < 0) {
			float remainder = oldZDecimal % gridSize;
			int currentGridSteps = (int)(oldZDecimal/gridSize);

			if (remainder <= -gridSize / 2) {
				currentGridSteps--;
			}

			newPos.z = (int)old.z + (currentGridSteps * gridSize);
		}

		return newPos;
	}


	private GameObject GenSpawnPointGameObject(Vector3 pos){
		GameObject spawnPointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		spawnPointObj.transform.position = pos;
		spawnPointObj.name = "spawnPointObj";

		//For now, no script is need just use "Vector3Thin.FromVector3()" in export script.

        Material mat = new Material(Shader.Find("Custom/VertColorOutline"));
		mat.SetFloat("_Outline", 0.02f);
		mat.SetColor("_Color", new Color(0,0,0,1) );
		mat.SetFloat("_Glossiness", 0);
		mat.SetFloat("_Metallic", 0);
		spawnPointObj.GetComponent<MeshRenderer>().material = mat;
		Destroy( spawnPointObj.GetComponent<SphereCollider>() ); //Remove the old box collider because it will mess up the raycast block placement...
		spawnPointObj.AddComponent<MeshCollider>();

		return spawnPointObj;
	}


	private GameObject GenCubeGameObject(cube c) {
		GameObject cubeObj = new GameObject("cubeObj"); //Maybe make the name a hash code for the class instance...
		cubeObj.transform.position = new Vector3(c.x, c.y, c.z); //Move the actual gameObject position to match that of the mesh!
		
		//Save all cube data to the GameObject for later use!
		worldCube cubeScript = cubeObj.AddComponent<worldCube>();
		cubeScript.pos = c.pos.ToVector3();
		cubeScript.size = c.size.ToVector3();
		cubeScript.vertColor = c.vertColors[0].ToColor32();
		
		//Debug.Log("cubeObj.transform.position" + cubeObj.transform.position.ToString());

		//Since the gameObject is already translated to the correct position, all mesh verts should be spawned with a base position of (0,0,0).
		c.x = 0; c.y = 0; c.z = 0;
		Mesh m = MeshLib.GenCube(c);

		Vector3 translateVect = new Vector3(-c.sizeX/2,-c.sizeY/2,-c.sizeZ/2);
		m = MeshLib.translateMesh(m, translateVect);

		cubeObj.AddComponent<MeshFilter>().mesh = m;
		//Material mat = new Material(Shader.Find("Custom/VertColorOutline")); //Material mat = new Material(Shader.Find("Custom/VertColorNonEmis"));
		//cubeObj.AddComponent<MeshRenderer>().material = mat;
		cubeObj.AddComponent<MeshRenderer>().material = cubeMat;
		cubeObj.AddComponent<MeshCollider>();

		//Testing
		Material mat = new Material(Shader.Find("Custom/VertColorOutline"));
		//mat.SetFloat("_Outline", Mathf.Clamp(0.02f* c.sizeX, 0.02f, 100.0f) );
		mat.SetFloat("_Outline", 0.02f);
		mat.SetColor("_Color", new Color(0,0,0,1) );
		mat.SetFloat("_Glossiness", 0);
		mat.SetFloat("_Metallic", 0);
		cubeObj.GetComponent<MeshRenderer>().material = mat;
		

		return cubeObj;
	}




	



}
