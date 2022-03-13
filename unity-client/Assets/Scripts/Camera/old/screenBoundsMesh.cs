using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class screenBoundsMesh : MonoBehaviour {

	public float zLength = 1.0f;
	public float wallThickness = 1.0f;
	public Camera cam;

	// Use this for initialization
	void Start () {
		genScreenEdgeMesh(true, true, true, true);
	}

	void update(){
	}

	
	public void genScreenEdgeMesh(bool top, bool right, bool bottom, bool left){
		
		////Camera cam = Camera.main;

		//Destroy any old walls, so new ones being drawn will have a fresh start.
		for (int c = 0; c < transform.childCount; c++) {
			Destroy( transform.GetChild(c).gameObject );
		}

		//Get the screen limits.
		//Vector2 maxXY = cam.ScreenToWorldPoint( new Vector2(Screen.width, Screen.height) );
		//Vector2 zeroXY = cam.ScreenToWorldPoint( new Vector2 (0, 0) );

		//Testing-------------------------------------- START HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		/*
		int width = 256;
		int height = 240;

		int frameWidthZero = (Screen.width / 2) - (width / 2);
		int frameHeightZero = (Screen.height / 2) - (height / 2);
		int frameWidthMax = (Screen.width / 2) + (width / 2);
		int frameHeightMax = (Screen.height / 2) + (height / 2);

		Vector2 maxXY = cam.ScreenToWorldPoint( new Vector2(frameWidthMax, frameHeightMax) ); // camera.ViewportToWorldPoint() USING THIS: https://docs.unity3d.com/ScriptReference/Camera.ViewportToWorldPoint.html
		Vector2 zeroXY = cam.ScreenToWorldPoint( new Vector2 (frameWidthZero, frameHeightZero) );
		*/
		//Testing End----------------------------------

		//Get the screen limits.
		int width = 1; //(1,1) is the ViewPort rect max coordinate. In other words, the max height and width is 1. "cam.pixelWidth" (from what I can tell) is this ViewPort max in regular screenSpace pixels. So it won't work with ".ViewportToWorldPoint()".
		int height = 1;
		Vector2 maxXY = cam.ViewportToWorldPoint( new Vector2(width, height) );
		Vector2 zeroXY = cam.ViewportToWorldPoint( new Vector2 (0, 0) );


		if (top) {
			Vector3 botLeft = new Vector3 (zeroXY.x, maxXY.y, -zLength / 2);
			Vector3 topLeft = new Vector3 (maxXY.x, maxXY.y, -zLength / 2);
			Vector3 botRight = new Vector3 (zeroXY.x, maxXY.y, zLength / 2);
			Vector3 topRight = new Vector3 (maxXY.x, maxXY.y, zLength / 2);
			GameObject edgePlane = GenEdge(topLeft, botLeft, topRight, botRight);
			edgePlane.transform.SetParent(transform);
			edgePlane.name = "topEdge";
			edgePlane.layer = 9; //Set the new gameObject to the edgeCollider layer.
		}

		if (right) {
			Vector3 topRight = new Vector3 (maxXY.x, maxXY.y, -zLength / 2);
			Vector3 botRight = new Vector3 (maxXY.x, zeroXY.y, -zLength / 2);
			Vector3 topLeft = new Vector3 (maxXY.x, maxXY.y, zLength / 2);
			Vector3 botLeft = new Vector3 (maxXY.x, zeroXY.y, zLength / 2);
			GameObject edgePlane = GenEdge(topLeft, botLeft, topRight, botRight);
			edgePlane.transform.SetParent(transform);
			edgePlane.name = "rightEdge";
			edgePlane.layer = 9;
		}

		if (bottom) {
			Vector3 topLeft = new Vector3 (zeroXY.x, zeroXY.y, -zLength / 2);
			Vector3 botLeft = new Vector3 (maxXY.x, zeroXY.y, -zLength / 2);
			Vector3 topRight = new Vector3 (zeroXY.x, zeroXY.y, zLength / 2);
			Vector3 botRight = new Vector3 (maxXY.x, zeroXY.y, zLength / 2);
			GameObject edgePlane = GenEdge(topLeft, botLeft, topRight, botRight);
			edgePlane.transform.SetParent(transform);
			edgePlane.name = "bottomEdge";
			edgePlane.layer = 9;
		}

		if (left) {
			Vector3 topLeft = new Vector3 (zeroXY.x, maxXY.y, -zLength / 2); //Only go back half of zLength so the plane will be centered on the z-axis.
			Vector3 botLeft = new Vector3 (zeroXY.x, zeroXY.y, -zLength / 2); //Could use the following for edge reduction: Vector3 botLeft = new Vector3 (zeroXY.x + edgeReduction, zeroXY.y - edgeReduction, -zLength / 2);
			Vector3 topRight = new Vector3 (zeroXY.x, maxXY.y, zLength / 2); //This time go half the zLength forward instead of backwards.
			Vector3 botRight = new Vector3 (zeroXY.x, zeroXY.y, zLength / 2);
			GameObject edgePlane = GenEdge(topLeft, botLeft, topRight, botRight);
			edgePlane.transform.SetParent(transform);
			edgePlane.name = "leftEdge";
			edgePlane.layer = 9;
		}
	}



	GameObject GenEdge(Vector3 topLeft, Vector3 botLeft, Vector3 topRight, Vector3 botRight) {
		
		Mesh ScreenEdgeFront = GenPlane (topLeft, botLeft, topRight, botRight);

		Vector3 backPlaneTopLeft = topLeft - (ScreenEdgeFront.normals[0] *wallThickness); //Move the new plane behind the current verts, so in the opposite direction of their normals.     Note that the normals of these plane's face inward towards the camera.
		Vector3 backPlaneBotLeft = botLeft - (ScreenEdgeFront.normals[1] *wallThickness);
		Vector3 backPlaneTopRight = topRight - (ScreenEdgeFront.normals[2] *wallThickness);
		Vector3 backPlaneBotRight = botRight - (ScreenEdgeFront.normals[3] *wallThickness);

		Mesh ScreenEdgeBack = GenPlane (backPlaneTopLeft, backPlaneBotLeft, backPlaneTopRight, backPlaneBotRight);

		//Consolidate the two meshes into one.
		Mesh ms = new Mesh();
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector3> norms = new List<Vector3>();

		for (int i = 0; i < ScreenEdgeFront.triangles.Length; i++) {
			tris.Add(ScreenEdgeFront.triangles[i] + verts.Count);
		}
		norms.AddRange (ScreenEdgeFront.normals);
		verts.AddRange (ScreenEdgeFront.vertices);

		for (int i = 0; i < ScreenEdgeBack.triangles.Length; i++) {
			tris.Add(ScreenEdgeBack.triangles[i] + verts.Count); // "+verts.Count" keeps the indices correct! Exapmle: oldInd + CurrentIndexCount = newIndex.
		}
		norms.AddRange (ScreenEdgeBack.normals);
		verts.AddRange (ScreenEdgeBack.vertices);

		ms.vertices = verts.ToArray();
		ms.triangles = tris.ToArray();
		ms.normals = norms.ToArray();


		GameObject plane = new GameObject(); //Note: New GameObject always spawns at (0,0,0).
		plane.AddComponent<MeshFilter>();
		plane.GetComponent<MeshFilter>().mesh = ms;
		plane.AddComponent<MeshCollider>().convex = true; //Add a MeshCollider and turn on its convex for Rigidbody collision.    Note: Adding the "meshCollider" after the mesh is already set keeps us from having to set "plane.GetComponent<MeshCollider>().sharedMesh".

		return plane;
	}



	Mesh GenPlane(Vector3 topLeft, Vector3 botLeft, Vector3 topRight, Vector3 botRight) {
		Mesh ms = new Mesh();

		Vector3[] verts = new Vector3[4];
		int[] tris = new int[6];

		//Add Verts
		verts[0] = topLeft;
		verts[1] = botLeft;
		verts[2] = topRight;
		verts[3] = botRight;

		//Add Triangles
		tris[0] = 2;
		tris[1] = 1;
		tris[2] = 0;

		tris[3] = 3;
		tris[4] = 1;
		tris[5] = 2;

		//Apply arrays to mesh.
		ms.vertices = verts;
		ms.triangles = tris;

		//Calculate Normals
		ms.RecalculateNormals();

		return ms;
	}


}
