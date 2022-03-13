using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using TMPro;

public class EditorUI : MonoBehaviour {

	public Canvas cubeCanvas;
	public TMP_InputField sizeInput;
	public TMP_InputField colorInput;
	public TMP_InputField worldSizeInput;
	public TMP_InputField flySpeedInput;


	void Start () {
		sizeInput.text = "{x:1, y:1, z:1}";
		colorInput.text = "{r:255, g:255, b:255, a:255}";
		worldSizeInput.text = "{w:" + EditorCtrl.width + ", h:" + EditorCtrl.height + "}";
		flySpeedInput.text = "{s:" + GameObject.Find("Camera (MapControls)").GetComponent<EditorCam>().FlySpeed + "}";
	}
	

	void Update () {

		if(Input.GetKeyDown("escape") && cubeCanvas.gameObject.activeInHierarchy){
			//TMP has a settings called "Restore On ESC Key". Make sure this is false.

			Vector3Thin s;
			Color32Thin c;
			int w;
			int h;
			float speed;

			try{
				s = JsonConvert.DeserializeObject<Vector3Thin>(sizeInput.text, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
				if(s == null){ throw new Exception("'s' is null."); }
			} catch(Exception err){
				Debug.Log(err);
				s = new Vector3Thin(1,1,1);
				sizeInput.text = "{x:1, y:1, z:1}";
			}

			try{
				c = JsonConvert.DeserializeObject<Color32Thin>(colorInput.text, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
				if(c == null){ throw new Exception("'c' is null."); }
			} catch(Exception err){
				Debug.Log(err);
				c = new Color32Thin(255,255,255,255);
				colorInput.text = "{r:255, g:255, b:255, a:255}";
			}

			try{
				//Use dynamic object dserialization to grab the width and height set by player...
				worldSizeJSON dyna = JsonConvert.DeserializeObject<worldSizeJSON>(worldSizeInput.text, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
				w = (dyna.w != 0) ? dyna.w : 100;
				h = (dyna.h != 0) ? dyna.h : 100;
				//w = dyna.w ?? 100;
				//h = dyna.h ?? 100;
			} catch(Exception err){
				Debug.Log(err);
				w = 100;
				h = 100;
				worldSizeInput.text = "{w:" + w + ", h:" + h + "}";
			}
			
			try{
				speedJSON dyna2 = JsonConvert.DeserializeObject<speedJSON>(flySpeedInput.text, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None});
				speed = (dyna2.s != 0) ? dyna2.s : 0.1f;
				//speed = dyna2.s ?? 0.1f;
			} catch(Exception err){
				Debug.Log(err);
				speed = 0.1f;
				flySpeedInput.text = "{s:" + speed + "}";
			}
			
			
			EditorCtrl.originalSize = s;
			EditorCtrl.color = c;
			EditorCtrl.width = w;
			EditorCtrl.height = h;
			GameObject.Find("Camera (MapControls)").GetComponent<EditorCam>().FlySpeed = speed; //Save the refence to a var later...
			GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().genBackdrop(); //Call the function to regenerate the backdrop with new dimensions.
		}
		
		cubeCanvas.gameObject.SetActive(!EditorCtrl.blnBuildMode); //Switch the Active bool after the parameter are set above...

		//Control the blnBuildMode switch down here...
		if(Input.GetKeyDown("escape")){
			EditorCtrl.blnBuildMode = !EditorCtrl.blnBuildMode;
		}

	}

	public void exitNoSave(){
		SceneManager.LoadScene(0);
	}

	public void saveAndExit(){
		
		//Save world to file!
		GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().saveMapData();

		SceneManager.LoadScene(0);
	}

	public void playTest(){
		//Save world to file!
		GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().saveMapData();

		//Load the play test scene.
		SceneManager.LoadScene(4);
	}

	public void resetWorld(){
		
		GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().resetWorld();
		sizeInput.text = "{x:1, y:1, z:1}";
		colorInput.text = "{r:255, g:255, b:255, a:255}";
		worldSizeInput.text = "{w:" + EditorCtrl.width + ", h:" + EditorCtrl.height + "}";
		EditorCtrl.blnBuildMode = true;

	}

	public void loadJSONMap(){
		GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().renderMapFromJSON(Application.persistentDataPath + "/map/map.json");
		worldSizeInput.text = "{w:" + EditorCtrl.width + ", h:" + EditorCtrl.height + "}"; //Reset the world size in the UI to match the size loaded from the JSON...

		Debug.Log("CubesCount: " + GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().CubesCount + " / 8187");
	}

	public void exportJSONMap(){
		GameObject.Find("Camera (MapControls)").GetComponent<EditorCtrl>().exportJSONMapData();
	}

}



public class speedJSON {
	public float s;
}

public class worldSizeJSON {
	public int w;
	public int h;
}