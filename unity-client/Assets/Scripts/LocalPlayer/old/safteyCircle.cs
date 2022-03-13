using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//
//		NOTE: I'm pretty sure this script is not used!
//

public class safteyCircle : MonoBehaviour {

	public Color32 dangerColor;
	public Color32 nextCircleColor;
	[Tooltip("vertDefinition^2 is how many verts each circle mesh will have.")]
	public int vertDefinition = 250;
	[Tooltip("The thickness of the outline for the 'next' circle to draw")]
	public float lineThickness = 5.0f;

	//private Vector3[] vertCache;
	private Mesh planeCache;

	void Start () {
		//Vector3Thin cent = new Vector3Thin(0,0,0);
		//float dist = 50.0f;
		//Mesh ms = genMapPlane(500, 500);
		//ms = updateColors(ms, cent, dist);
		//GetComponent<MeshFilter>().mesh = ms;
		
		/*Vector3Thin cent = new Vector3Thin(0,0,0); //debug
		float dist = 50.0f; //debug
		drawSafteyCircle(cent, dist); //debug
		drawNextCircle(cent, dist); //debug
		*/

		//Debug!!!
		/*Vector3 test1 = new Vector3(1,2,3);
		Vector3 test2 = test1;
		test1.x = 100;
		Debug.Log("x1: " + test1.x + " x2: " + test2.x);*/
		
		planeCache = null; //Set to a easy false check value.
	}
	

	void Update () {
	}


	public void drawSafteyCircle(Vector3Thin center, float dist, int mapWidth, int mapHeight){
		
		//Mesh ms = genMapPlane(mapWidth, mapHeight);
		//Mesh ms = (planeCache != null) ? MeshLib.CloneMesh(planeCache) : genMapPlane(mapWidth, mapHeight); //grab teh mesh cache is possible.
		
		Mesh ms;
		if(planeCache != null){
			ms = MeshLib.CloneMesh(planeCache);
			//Debug.Log("Here!");
		} else {
			ms = genMapPlane(mapWidth, mapHeight);
			planeCache = MeshLib.CloneMesh(ms); //Save a copy of the new mesh to the cache!
		}
		ms = updateDangerColors(ms, center, dist);
		
		GetComponent<MeshFilter>().mesh = ms;
	}



	private Mesh updateDangerColors(Mesh ms, Vector3Thin center, float dist){
		Vector3[] verts = ms.vertices;
		Color32[] colors = ms.colors32;
		for(int i=0; i<verts.Length; i++){
			if( Vector3Thin.distance( center, Vector3Thin.FromVector3(verts[i]) ) < dist ){
				//Make a transparent circle where the saftey area is!
				colors[i].a = 0;

				//Move the vert pos back so the trianlge edge fade will not appear!
				Vector3 distVect = verts[i] - center.ToVector3();
				verts[i] += distVect;
			} else {
				colors[i] = dangerColor; //Set the danger color.
			}
		}

		ms.vertices = verts;
		ms.colors32 = colors;
		ms.RecalculateNormals();
		return ms;
	}



	public void drawNextCircle(Vector3Thin center, float dist, int mapWidth, int mapHeight){
		//Mesh ms = genMapPlane(mapWidth, mapHeight);
		
		Mesh ms;
		if(planeCache != null){
			ms = MeshLib.CloneMesh(planeCache);
		} else {
			ms = genMapPlane(mapWidth, mapHeight);
			planeCache = MeshLib.CloneMesh(ms); //Save a copy of the new mesh to the cache!
		}

		ms = updateNextZoneColors(ms, center, dist);
		GetComponent<MeshFilter>().mesh = ms;
	}


	private Mesh updateNextZoneColors(Mesh ms, Vector3Thin center, float dist){
		Vector3[] verts = ms.vertices;
		Color32[] colors = ms.colors32;

		//float lineThickness = 5.0f;
		for(int i=0; i<verts.Length; i++){
			bool lessThanDist = Vector3Thin.distance( center, Vector3Thin.FromVector3(verts[i]) ) < dist;
			bool greaterThanLineThicknessDist = Vector3Thin.distance( center, Vector3Thin.FromVector3(verts[i]) ) > (dist-lineThickness);
			
			if( lessThanDist && greaterThanLineThicknessDist ) {
				colors[i] = nextCircleColor;
			} else if( lessThanDist ){
				colors[i].a = 0; //This would be the center of the inner circle.
				Vector3 distVect = verts[i] - center.ToVector3();
				verts[i] += distVect;
			}
			else {
				colors[i].a = 0; //This would be everything outside of the circle.
				Vector3 distVect = verts[i] - center.ToVector3();
				verts[i] -= distVect;
			}
		}

		ms.vertices = verts;
		ms.colors32 = colors;
		ms.RecalculateNormals();
		return ms;
	}






	private Mesh updateBothColors(Mesh ms, Vector3Thin center, float dist){
		
		Vector3[] verts = ms.vertices;
		Vector3[] vertsOldPos = ms.vertices;
		Color32[] colors = ms.colors32;

		for(int i=0; i<verts.Length; i++){
			
			bool lessThanDist = Vector3Thin.distance( center, Vector3Thin.FromVector3(verts[i]) ) < dist;
			//bool greaterThanLineThicknessDist = Vector3Thin.distance( center, Vector3Thin.FromVector3(verts[i]) ) > (dist-lineThickness);


			if( lessThanDist ){
				//Make a transparent circle where the saftey area is!
				colors[i].a = 0;

				//Move the vert pos back so the trianlge edge fade will not appear!
				Vector3 distVect = verts[i] - center.ToVector3();
				verts[i] += distVect;
			} else {
				colors[i] = dangerColor; //Set the danger color.
			}
		}

		ms.vertices = verts;
		ms.colors32 = colors;
		ms.RecalculateNormals();
		return ms;
	}





	private Mesh genMapPlane(int width, int height) {
		/*width++; //"+1" to include the last unit!
		height++;

		Mesh[] squares = new Mesh[width * height];
		int vertDefinition = 5; //25; //Don't make more than 100!

		for(int y=0; y < height; y++){
			for (int x=0; x < width; x++) {
				int vertIndex = x + y * width;
				Mesh m = genMesh(vertDefinition, vertDefinition);
				m = MeshLib.scaleMesh(m, new Vector3(1.0f/(float)vertDefinition, 1.0f/(float)vertDefinition, 1.0f/(float)vertDefinition));
				
				m = MeshLib.translateMesh(m, new Vector3(-0.5f, -0.5f, 0)); //Center the mesh
				m = MeshLib.translateMesh(m, new Vector3(x, y, 0)); //Move the mesh to right position!
				squares[vertIndex] = m;
			}
		}

		Mesh msExport = MeshLib.consolidateMeshes(new List<Mesh>(squares));

		return msExport;*/

		//int vertDefinition = 250; //100; //Was 200... //250 is about max without hitting the vert limit.
		float zPos = -1.0f;

		Mesh m = MeshLib.GenPlane(vertDefinition, vertDefinition, zPos);
		float ScaleWidth = (float)width/(float)vertDefinition; //grab the percecnt difference.
		float ScaleHeight = (float)height/(float)vertDefinition;
		Vector3 scale = new Vector3( ScaleWidth, ScaleHeight, 1); //Vector3 scale = new Vector3( Mathf.Clamp(ScaleWidth, 1, ScaleWidth), Mathf.Clamp(ScaleHeight, 1, ScaleHeight), 1);
		
		m = MeshLib.translateMesh(m , new Vector3(-vertDefinition/2, -vertDefinition/2, 0)); //Center the mesh on the gameObject.
		m = MeshLib.scaleMesh(m, scale);

		//Note! Since the mesh is so big, we need to recalculate the bounds of the mesh of it will render properly!
		// See: https://answers.unity.com/questions/489604/mesh-not-showing-in-camera.html
		// Could instead use a SkinnedMeshRenderer: https://docs.unity3d.com/Manual/class-SkinnedMeshRenderer.html
		// Mesh.bounds: https://docs.unity3d.com/ScriptReference/Mesh-bounds.html
		/****Bounds b = new Bounds();
		b.center = new Vector3(0,0,0);
		b.extents = new Vector3(width/2,height/2, zPos); //The mesh will extend half its dimensions in X & Y.
		m.bounds = b;****/

		m = MeshLib.RecalculateMeshBounds(m);

		return m;
	}





	/*public Mesh genMesh(int xMax, int yMax, float zPos) {
		Vector3[] defaultVerts; // Will hold the default position for the new mesh.
		//int screenVertexReduction = 10; //When constructing the plane-mesh this will be use to reduce the amount of verts made. (Screen.height/screenVertexReduction by Screen.width/screenVertexReduction) is the amount of vertices made.
		//int xMax = 100;
		//int yMax = 100;

		//Vector3 camMin;
		//Vector3 camMax;
		Color32 vertColor = new Color32(255,255,255,255);

		//float zPos = -1.0f;


		Mesh ms = new Mesh();

		//xMax = Screen.width / screenVertexReduction;
		//yMax = Screen.height/screenVertexReduction;

		defaultVerts = new Vector3[yMax * xMax]; // This will hold all the verts. Note that it is not a local var, we will use it in the update.
		int[] tris = new int[ (defaultVerts.Length -yMax -xMax +1)*6 ]; // "-yMax -xMax +1" because we want to skip the last row and column of vertices (we don't want triangles hanging off the screen), but "+1" because they both share a common vertex, the bottom-right corner. Without the "+1" we would be lacking the space for the last vertex's two triangles. "*6" because each vert will have two triangles, each triangle has three points.
		Vector3[] norms = new Vector3[defaultVerts.Length]; // One normal for every vert.
		Color32[] colors = new Color32[defaultVerts.Length]; // Vertex color, one for each vert.

		//camMin = cam.ScreenToWorldPoint ( new Vector3(0, 0, 1) ); // Cached here for speed.
		//camMax = cam.ScreenToWorldPoint( new Vector3(Screen.width, Screen.height, 1) ); // Cached here for speed.

		// Generate the procedural mesh.
		for (int y=0; y < yMax; y++) {
			// Cache vars here before second loop for improved speed.
			int globalIndexYCorrection = y * xMax; // This will help transform a vert's position from (X,Y) coordinates into an index number. Example: y=2, xMax=4. startingXIndexOnRowTwo=2*4=8.
			//Color32 vertColor = dangerColor; //Color32.Lerp(lowColor, highColor, (float)y/(yMax-1)); // In "(float)y/(yMax-1)" the float casts are needed to return a decimal value instead of an int. "yMax-1" because just like the "y < ymax" this needs to stop at one less than the max for the same reason!

			// "(float)y/(yMax-1)" The float cast is needed to make sure the answer is not rounded to an int.
			// "yMax-1" because just like the "y < ymax" this needs to stop at one less than the max for the same reason!
			// "(camMax.y-camMin.y)" will get the distance from the world-space beginning position of the screen to the world-space ending position of the screen. This position will always be a positive number, even if though the beginning pos is a negative.
			// "+camMin.y" First know "camMin.y" will be a negative number (something like: -5, If it were positive we'd have to subtract it here instead of adding it.) This is will be added on the end to transform the value from 0-to-distance to min-to-max. Example: min=-5, max=5, distance=5-(-5)=10. To transform a distance-scale value from 0 to 10 into a min-to-max scale value of -5 to 5, just add the min (which is negative) to the distance: 10+(-5) = 5. Another example: 7+(-5)=2.
			// So, the formula is really: Scalar * distance + transformer
			//float yCorrection = ((float)y/(yMax-1)) * (camMax.y-camMin.y) +camMin.y;
			float yCorrection = ((float)y/(yMax-1)) * yMax;

			for (int x=0; x < xMax; x++) {
				int vertIndex = x + globalIndexYCorrection; // Add the X offest to the row correction number. Example: globalIndexYCorrection=8, x=5. trueVertIndex=8+5=13. Because "8" is the starting index of this row, and "5" is the X offset.

				// Generate the procedural mesh's vertices.
				//float xCorrection = ((float)x/(xMax-1)) * (camMax.x-camMin.x) +camMin.x;
				float xCorrection = ((float)x/(xMax-1)) * xMax;
				Vector3 vertPos = new Vector3(xCorrection, yCorrection, zPos); // Z=1 because I want the mesh in front of the camera.
				//Vector3 vertPos = new Vector3(x, y, 1);
				defaultVerts[vertIndex] = vertPos;

				// Triangulate the vertices
				if (y != yMax - 1 && x != xMax - 1) { // Don't make any triangles on the last row or column!
					// Think of the four verts below as the four points of a square.
					int relFirst = x + (y * xMax ); // This is the first index (the index in the defaultVerts array) of the two triangles to be made. It is also the top-left corner of the square.
					int relSecond = x + (y * xMax ) + 1; // Top-right corner. "+1" because we need it to be one vert over on the X axis.
					int relThird = x + ( (y+1) * xMax ) + 1; // Bottom-Right corner. "y+1" because it needs to be one row lower than the top corners.
					int relForth = x + ( (y+1) * xMax ); // Bottom-Left corner.

					// Add the first triangle.
					tris[(relFirst - y)*6] = relThird; // "-y" because on every row there is one vertex we don't use (the last vertex of the row because we don't want on triangles on the end). This "-y" will subtract the correct amount of verts (one for each row) from our index counter, which is "relFirst". "*6" because we want to reserve six indices for every vertex. These indices will make up the two triangles.
					tris[(relFirst - y)*6 +1] = relSecond;
					tris[(relFirst - y)*6 +2] = relFirst;

					// Add the second triangle.
					tris[(relFirst - y)*6 +3] = relThird;
					tris[(relFirst - y)*6 +4] = relFirst;
					tris[(relFirst - y)*6 +5] = relForth;
				}

				// Generate the normals for all the triangles.
				norms[vertIndex] = new Vector3 (0, 0, 0); // This vector tells what direction of vert is facing outward.

				// Generate vertex color.
				colors[vertIndex] = vertColor; //dangerColor; // Cached color because it's the same for the whole Y-row.
			}
		}

		// Set the data to the mesh var.
		ms.vertices = defaultVerts;
		ms.triangles = tris;
		ms.normals = norms;
		ms.colors32 = colors;

		ms.RecalculateNormals();

		return ms;
	}*/



}
