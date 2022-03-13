/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Added
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class clickBuild : MonoBehaviour {

	public GameObject[] BuildObjs;
	public int selectedBuildObj;

	GameObject lastOutlineObj;
	[Tooltip("Must be a material with the rendering mode set to transparent.")]
	public Material transparentColor;

	public float gridStep = 1;

	public gameWorld currentWorld = new gameWorld();

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {


		//Draw the build cursor...
		RaycastHit pointRay1;
		int layerMaskRegColliderOut = 1 << 8; //Layermask help here:https://answers.unity.com/questions/8715/how-do-i-use-layermasks.html
		if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out pointRay1, Mathf.Infinity)){ //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out pointRay1, Mathf.Infinity, layerMaskRegColliderOut)){
			//Clear old frame's obj...
			Destroy(lastOutlineObj);


			Mesh hitMesh = pointRay1.collider.gameObject.GetComponent<MeshFilter>().mesh;
			Vector3[] verts = hitMesh.vertices;
			int[] tris = hitMesh.triangles;
			Vector3[] norms = hitMesh.normals;

//			print ("Triangles");
//			for (int i = 0; i < tris.Length; i++) {
//				print (tris[i].ToString());
//			}
//
//			print ("Normals");
//			for (int i = 0; i < norms.Length; i++) {
//				print (norms[i].ToString());
//			}

			print (verts[tris[pointRay1.triangleIndex*3]]);
			print (verts[tris[pointRay1.triangleIndex*3 + 1]]);
			print (verts[tris[pointRay1.triangleIndex*3 + 2]]);


			//Method for Normal calculation found here: https://www.khronos.org/opengl/wiki/Calculating_a_Surface_Normal
			Vector3[] hitTri = new Vector3[] {verts[tris[pointRay1.triangleIndex*3]], verts[tris[pointRay1.triangleIndex*3 + 1]], verts[tris[pointRay1.triangleIndex*3 + 2]]};
			Vector3 U = new Vector3 (hitTri[1].x - hitTri[0].x, hitTri[1].y - hitTri[0].y, hitTri[1].z - hitTri[0].z);
			Vector3 V = new Vector3 (hitTri[2].x - hitTri[0].x, hitTri[2].y - hitTri[0].y, hitTri[2].z - hitTri[0].z);
			Vector3 correctionVectorReCal = new Vector3 ((U.y * V.z) - (U.z * V.y), (U.z * V.x) - (U.x * V.z), (U.x * V.y) - (U.y * V.x));
			print ("New correctionVector: " + correctionVectorReCal.ToString());

//			Vector3 correctionVector = norms [pointRay1.triangleIndex];
//			print ("correctionVector: " + correctionVector.ToString());
			//pointRay1.triangleIndex
			float ObjSizeFromOrgin = 0.5f;
			//Vector3 fixedPos = pointRay1.point + new Vector3(correctionVectorReCal.x *ObjSizeFromOrgin, correctionVectorReCal.y *ObjSizeFromOrgin, correctionVectorReCal.z *ObjSizeFromOrgin); //Vector3 fixedPos = pointRay1.point + correctionVectorReCal;
			Vector3 fixedPos = pointRay1.point + (correctionVectorReCal * ObjSizeFromOrgin);
			print("RegPos: " + pointRay1.point.ToString());
			print("fixedPos: " + fixedPos.ToString());

			//Look here for help with collider layermasking: https://gamedev.stackexchange.com/questions/138530/unity-array-index-is-out-of-range-raycast-triangleindex

			lastOutlineObj = Instantiate(BuildObjs[selectedBuildObj], gridRound(fixedPos, gridStep), Quaternion.identity); //lastOutlineObj = Instantiate(BuildObjs[selectedBuildObj], pointRay1.point, Quaternion.identity);
			//lastOutlineObj = Instantiate(BuildObjs[selectedBuildObj], gridRound(pointRay1.point, gridStep), Quaternion.identity); //lastOutlineObj = Instantiate(BuildObjs[selectedBuildObj], pointRay1.point, Quaternion.identity);


			//Make color transparent
			Color transparentTexture = new Color(lastOutlineObj.GetComponent<MeshRenderer>().material.color.r, lastOutlineObj.GetComponent<MeshRenderer>().material.color.g, lastOutlineObj.GetComponent<MeshRenderer>().material.color.b, 0.5f); //Set a new transparent color with the same RGB as the regular texture.
			lastOutlineObj.GetComponent<MeshRenderer> ().material = transparentColor; //Set the cursorObj's material to one that can be turn transparent.
			lastOutlineObj.GetComponent<MeshRenderer>().material.color = transparentTexture; //Then change it to transpaernt.

			//Disable colliders so, they won't be hit be the raycast...
			if(lastOutlineObj.GetComponent<BoxCollider>()){
				lastOutlineObj.GetComponent<BoxCollider>().enabled = false;
			} else if(lastOutlineObj.GetComponent<SphereCollider>()){
				lastOutlineObj.GetComponent<SphereCollider>().enabled = false;
			}
			if(lastOutlineObj.GetComponent<MeshCollider>()){
				lastOutlineObj.GetComponent<MeshCollider>().enabled = false;
			}

			if (lastOutlineObj.GetComponent<Rigidbody>()) {
				lastOutlineObj.GetComponent<Rigidbody> ().constraints = RigidbodyConstraints.FreezeAll;
			}

			if (lastOutlineObj.GetComponent<worldObjCtrl> ()) {
				lastOutlineObj.GetComponent<worldObjCtrl>().enabled = false;
			}

		}


		//Place the buildObj if you player clicks...
		if (Input.GetMouseButtonDown (0) && lastOutlineObj && Cursor.lockState == CursorLockMode.Locked) { //"Cursor.lockState == CursorLockMode.Locked" is so you would place blocks when the menu is open!
			//Add the new obj to the currentWorld class for saving as well!
//			[SerializeField]
			GameObject block = Instantiate(BuildObjs[selectedBuildObj], lastOutlineObj.transform.position, Quaternion.identity); //currentWorld.buildObjs.Add( Instantiate(BuildObjs[selectedBuildObj], lastOutlineObj.transform.position, Quaternion.identity) );
			//currentWorld.buildObjs.Add(block);

			block.GetComponent<worldObjCtrl>().indexInWorldList = currentWorld.ObjID.Count; //This updated the worldListIndex of the current block. Must be called before "serializeBlock".
			currentWorld.serializeBlock(block);
		}


		//Place lots buildObj if you player holds...
		if (Input.GetMouseButton (1) && lastOutlineObj && Cursor.lockState == CursorLockMode.Locked) { //"Cursor.lockState == CursorLockMode.Locked" is so you would place blocks when the menu is open!
			//Add the new obj to the currentWorld class for saving as well!
			//currentWorld.buildObjs.Add( Instantiate(BuildObjs[selectedBuildObj], lastOutlineObj.transform.position, Quaternion.identity) );
			GameObject block = Instantiate(BuildObjs[selectedBuildObj], lastOutlineObj.transform.position, Quaternion.identity);
//			currentWorld.serializeBlock(block);
			block.GetComponent<worldObjCtrl>().indexInWorldList = currentWorld.ObjID.Count; //This updated the worldListIndex of the current block. Must be called before "serializeBlock".
			currentWorld.serializeBlock(block);
		}



		//Change buildObj via scrollwheel next!

		//Change the select buildObj based on player input.
		if (Input.GetKey ("0")) {
			selectedBuildObj = 0;
		}

		if (Input.GetKey ("1")) {
			selectedBuildObj = 1;
		}

		if (Input.GetKey ("2")) {
			selectedBuildObj = 2;
		}

//		if (Input.GetKey ("3")) {
//			selectedBuildObj = 3;
//		}

	}



	public void saveGameWorld() { //maybe take a file name here...
		//Create a BinaryFormatter object to handle Serialization.
		BinaryFormatter bf = new BinaryFormatter();
		//Open a file on the "persistentDataPath".
		FileStream file = File.Create(Application.persistentDataPath + "/game.World"); //No mode is needed since we are just creating the file.

		//Write the Serialized class to the file.
		bf.Serialize (file, currentWorld);
		//Close the file.
		file.Close();
	}

	public void loadGameWorld(){ //string filename
		if(File.Exists(Application.persistentDataPath + "/game.World")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open (Application.persistentDataPath + "/game.World", FileMode.Open);

			// This is where the magic happens! "BinaryFormatter.Deserialize" reads our saved class from the file.
			// But it does not know what it is reading, so we need to tell it what type of object it is by using a cast. Which is the "(className)" in front of the "bf.Deserialize".
			gameWorld loadedGameWorld = (gameWorld)bf.Deserialize(file);
			file.Close();

			//Do a for loop instantiating all the gameObjects in "loadedGameWorld.buildObjs".
			for( int i=0; i < loadedGameWorld.ObjID.Count; i+=3){
				//Maybe clear the current "currentWorld" list and add these load objects to it via "currentWorld.buildObjs.Add".
				////GameObject newBlock = Instantiate (BuildObjs[loadedGameWorld.ObjID[i]], loadedGameWorld.worldObjPos[i], loadedGameWorld.worldObjRot[i]);
				////newBlock.GetComponent<worldObjCtrl> ().indexInWorldList = i;

				Vector3 ObjPos = new Vector3 (loadedGameWorld.worldObjPos [i], loadedGameWorld.worldObjPos [i + 1], loadedGameWorld.worldObjPos [i + 2]);
				Vector3 ObjRot = new Vector3 (loadedGameWorld.worldObjRot [i], loadedGameWorld.worldObjRot [i + 1], loadedGameWorld.worldObjRot [i + 2]);

				GameObject newBlock = Instantiate(BuildObjs[loadedGameWorld.ObjID[i]], ObjPos, Quaternion.Euler(ObjRot));
				newBlock.GetComponent<worldObjCtrl>().indexInWorldList = i;
			}
		}
	}

	[Serializable] //Could try this: https://stackoverflow.com/questions/36852213/how-to-serialize-and-save-a-gameobject-in-unity
	public class gameWorld {
		//[SerializeField]
		//public List<GameObject> buildObjs = new List<GameObject>(); //Maybe keep track of each blocks index in this list, inside of that block script?

		// Note: position in list should be keep track of via "i" and in the block script, but not here, there is no need since the placement in the list is its "i".
		public List<int> ObjID = new List<int>();
		public List<float> worldObjPos = new List<float>(); //public List<Vector3> worldObjPos = new List<Vector3>();
		public List<float> worldObjRot = new List<float>(); //public List<Quaternion> worldObjRot = new List<Quaternion>();

		public void serializeBlock(GameObject block){
			// These are setup like the normals/vertices in unity to where the first three indecies all refer to one worldObj.
			this.ObjID.Add(block.GetComponent<worldObjCtrl>().ObjID); //Maybe just make one of these... and just to i+1, i+2, 1+3 for the rest? Maybe not because it would become harder to remove an Obj from the worldArray...
			this.ObjID.Add(block.GetComponent<worldObjCtrl>().ObjID);
			this.ObjID.Add(block.GetComponent<worldObjCtrl>().ObjID);

			this.worldObjPos.Add(block.gameObject.transform.position.x); //this.worldObjPos.Add(new List<float>() {block.gameObject.transform.position.x, block.gameObject.transform.position.y, block.gameObject.transform.position.z} ); //this.worldObjPos.Add(block.gameObject.transform.position);
			this.worldObjPos.Add(block.gameObject.transform.position.y);
			this.worldObjPos.Add(block.gameObject.transform.position.z);

			this.worldObjRot.Add(block.gameObject.transform.rotation.eulerAngles.x);
			this.worldObjRot.Add(block.gameObject.transform.rotation.eulerAngles.y);
			this.worldObjRot.Add(block.gameObject.transform.rotation.eulerAngles.z);
		}



		//If that does not work...
		//List<vector3> buildObjs_pos;
		//List<int> buildObjs_blockID;
		//So on...
		//This was index 1 in all the list will correspond to the first gameObj.
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
//			print(remainder + " " + currentGridSteps);

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
//			print(remainder + " " + currentGridSteps);

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

//			print(remainder + " " + currentGridSteps);

			if (remainder >= gridSize / 2) {
				currentGridSteps++;
			}

			newPos.y = (int)old.y + (currentGridSteps * gridSize);
		}

		else if (oldYDecimal < 0) {
			float remainder = oldYDecimal % gridSize;
			int currentGridSteps = (int)(oldYDecimal/gridSize);
//			print(remainder + " " + currentGridSteps);

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

//			print(remainder + " " + currentGridSteps);

			if (remainder >= gridSize / 2) {
				currentGridSteps++;
			}

			newPos.z = (int)old.z + (currentGridSteps * gridSize);
		}

		else if (oldZDecimal < 0) {
			float remainder = oldZDecimal % gridSize;
			int currentGridSteps = (int)(oldZDecimal/gridSize);
//			print(remainder + " " + currentGridSteps);

			if (remainder <= -gridSize / 2) {
				currentGridSteps--;
			}

			newPos.z = (int)old.z + (currentGridSteps * gridSize);
		}



//		print (old.x + " " + old.y + " " + old.z);
//		print (newPos.x + " " + newPos.y + " " + newPos.z);

		return newPos;
	}


}

*/