using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class miniMapPlayerScale : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		float mapScale = MiniMapCtrl.orthographicZoom*0.15f; // 15% of the zoom size.
		mapScale = Mathf.Clamp(mapScale, 2, 15); //Clamp to appropriate sizes.
		transform.localScale = new Vector3(mapScale,mapScale,mapScale);

		//Keep the mesh centered on rotation! The "-0.361f" will be different for each mesh!
		float meshOffset = -0.361f;
		transform.localPosition = new Vector3(meshOffset * mapScale, 0, 0);
	}
}
