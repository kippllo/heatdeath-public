using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class PlayerTestSceneCtrl : MonoBehaviour {

	public GameObject localPlayerPrefab;
	public GameObject WorldCanvas;
	MapData map;

	async void Start () {
		getMapData();

		TempRoundData.reset(); //Reset the temp data holder... 

		await Task.Delay(10); //wait a sec to spawn player so the mini map can be initialized.

		//Spawn player at a random spawn point
		GameObject localPlayerObj = Instantiate(localPlayerPrefab, WorldCanvas.transform);
		
		if(map.spawnPoints.Length > 0){ //Only spawn at a spawnPoint if there are spawn points set!
			int randInd = UnityEngine.Random.Range(0, map.spawnPoints.Length);
			localPlayerObj.transform.position = map.spawnPoints[randInd].ToVector3();
		}
	}

	void Update () {
		if(Input.GetButtonDown("Quit")){ //if(Input.GetKeyDown("q")){ //Input.GetKeyDown("escape")
			//Debug.Log("SceneManager.sceneCount:" + SceneManager.sceneCount);
			//SceneManager.UnloadSceneAsync(0);
			SceneManager.LoadScene(3); //Load back to map editor...
		}
	}


	void getMapData() {
		//Read the map save file.
		map = EditorCtrl.readMapFile();
		map.GenMap();
	}

}
