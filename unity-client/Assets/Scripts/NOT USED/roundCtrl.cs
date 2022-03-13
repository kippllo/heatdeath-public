using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roundCtrl : MonoBehaviour {

	public lowResCam camCtrl;
	public screenBoundsMesh edgeCtrl;

	public float timer = 0;
	public float timerMax = 1.0f;
	float shrinkSpeed; // = 0.01f; //Was: 0.1f
	public float minDims = 100;

	public float camZoom = 10;
	int camWidth = 1280;
	int camHeight = 720;

	public bool lowRes = true;
	public int lowResScale = 4;
	Camera cam;
	// Use this for initialization
	void Start () {
		cam = Camera.main;

		if (lowRes) {
			//camCtrl.renText.filterMode = FilterMode.Point;
			camCtrl.pixelPerfectScreen(camWidth/lowResScale, camHeight/lowResScale, camWidth, camHeight); //camCtrl.pixelPerfectScreen(133, 75, camWidth, camHeight);
			//camCtrl.pixelPerfectScreen(75, 75, camWidth, camHeight);

			int maxDim = (camWidth >= camHeight) ? camWidth : camHeight; //int maxDim = (camCtrl.width >= camCtrl.height) ? camCtrl.width : camCtrl.height;
			shrinkSpeed = 3 / (maxDim - minDims); //Find the perfect shrink speed so the player can't go through the walls.
			cam.orthographicSize = camZoom;

		} else {
			//camCtrl.renText.filterMode = FilterMode.Trilinear;
			//Maybe randomly gen the starting dimensions later...
			camCtrl.width = camWidth;
			camCtrl.height = camHeight;

			int maxDim = (camWidth >= camHeight) ? camWidth : camHeight;
			shrinkSpeed = 3 / (maxDim - minDims); //Find the perfect shrink speed so the player can't go through the walls.
			cam.orthographicSize = camZoom;

			camCtrl.regularScreen();
		}

		edgeCtrl.genScreenEdgeMesh(true, true, true, true);

	}
	
	// Update is called once per frame
	void Update () {

		timer += Time.deltaTime;
		if(timer >= timerMax) {
			timer -= timerMax;

			//Shrink the screen dimensions at the same rate that the camera zoom is shrinking.
			camZoom = Mathf.Lerp(camZoom, 1, shrinkSpeed);
			camWidth = (int)Mathf.Lerp(camWidth, minDims, shrinkSpeed);
			camHeight = (int)Mathf.Lerp(camHeight, minDims, shrinkSpeed);

			if (lowRes) {
				//camCtrl.renText.filterMode = FilterMode.Point;
				camCtrl.pixelPerfectScreen (camWidth/lowResScale, camHeight/lowResScale, camWidth, camHeight); //camCtrl.pixelPerfectScreen (133, 75, camWidth, camHeight);
				//camCtrl.pixelPerfectScreen (75, 75, camWidth, camHeight);
			} else {
				//camCtrl.renText.filterMode = FilterMode.Trilinear;
				camCtrl.resizeScreen(camWidth, camHeight);
			}

			cam.orthographicSize = camZoom;


			//cam.transform.rotation = Quaternion.Euler(0,0,0); //Trick the rotation to be flat when the edge collider is spawned, then set the rotation back for rendering.
			edgeCtrl.genScreenEdgeMesh(true, true, true, true); //Re-gen the edge colliders.
			//cam.transform.rotation = Quaternion.Euler(-10,0,0);

		}
		
	}
}
