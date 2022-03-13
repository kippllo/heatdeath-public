using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class worldCube : MonoBehaviour {

	public Vector3 pos;
	public Vector3 size;
	public Color32 vertColor; //Only one color for now...  //public Color32[] vertColors;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	//Make export classes!!!
	public cube ToCube() {
		cube exportData = new cube(pos.x, pos.y, pos.z, size.x, size.y, size.z, vertColor.r, vertColor.g, vertColor.b, vertColor.a);
		return exportData;
	}

}
