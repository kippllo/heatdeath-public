using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public static class MeshLib {

	public static Vector3 defaultNullPos {
		get {
			return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity); //every time return a new instance of this?
		}
	}

	public static Mesh GenCube(cube c) {
		Mesh ms = new Mesh();
		Vector3[] verts = new Vector3[8];
		int[] tris = new int[36];
        Color32[] col = new Color32[8];

		//Add Verts
        Vector3 botLeft = c.pos.ToVector3(); //Bottom left corner.
        Vector3 topLeft = c.pos.ToVector3() + new Vector3(0, c.sizeY, 0);
        Vector3 botRight = c.pos.ToVector3() + new Vector3(c.sizeX, 0, 0);
        Vector3 topRight = c.pos.ToVector3() + new Vector3(c.sizeX, c.sizeY, 0);

        Vector3 botLeft2 = c.pos.ToVector3() + new Vector3(0, 0, c.sizeZ); //Mirror point with depth of cube. I.e. the back of the cube in the Z-direction.
        Vector3 topLeft2 = c.pos.ToVector3() + new Vector3(0, c.sizeY, c.sizeZ);
        Vector3 botRight2 = c.pos.ToVector3() + new Vector3(c.sizeX, 0, c.sizeZ);
        Vector3 topRight2 = c.pos.ToVector3() + new Vector3(c.sizeX, c.sizeY, c.sizeZ);

		verts[0] = botLeft;
		verts[1] = topLeft;
		verts[2] = botRight;
		verts[3] = topRight;
        verts[4] = botLeft2;
		verts[5] = topLeft2;
		verts[6] = botRight2;
		verts[7] = topRight2;

		//Add Triangles
		tris[0] = 0; //Front Face of cube
		tris[1] = 1;
		tris[2] = 2;
		tris[3] = 1;
		tris[4] = 3;
		tris[5] = 2;

        tris[6] = 6; //Back Face of cube
		tris[7] = 5;
		tris[8] = 4;
		tris[9] = 6;
		tris[10] = 7;
		tris[11] = 5;

        tris[12] = 2; //Right Face of cube
		tris[13] = 3;
		tris[14] = 7;
		tris[15] = 2;
		tris[16] = 7;
		tris[17] = 6;

        tris[18] = 4; //Left Face of cube
		tris[19] = 5;
		tris[20] = 0;
		tris[21] = 5;
		tris[22] = 1;
		tris[23] = 0;

        tris[24] = 6; //Bottom Face of cube
		tris[25] = 4;
		tris[26] = 2;
		tris[27] = 4;
		tris[28] = 0;
		tris[29] = 2;

        tris[30] = 7; //Top Face of cube
		tris[31] = 3;
		tris[32] = 5;
		tris[33] = 5;
		tris[34] = 3;
		tris[35] = 1;

        //Grab the vert colors from the cube object.
        col[0] = c.vertColors[0].ToColor32();
        col[1] = c.vertColors[1].ToColor32();
        col[2] = c.vertColors[2].ToColor32();
        col[3] = c.vertColors[3].ToColor32();
        col[4] = c.vertColors[4].ToColor32();
        col[5] = c.vertColors[5].ToColor32();
        col[6] = c.vertColors[6].ToColor32();
        col[7] = c.vertColors[7].ToColor32();

		//Apply arrays to mesh.
		ms.vertices = verts;
		ms.triangles = tris;
        ms.colors32 = col;

		//Calculate Normals
		ms.RecalculateNormals();

		return ms;
	}


	public static Mesh GenPlane(int xMax, int yMax, float zPos) {
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
		/*ms.vertices = defaultVerts;
		ms.triangles = tris;
		ms.normals = norms;
		ms.colors32 = colors;*/

		ms.RecalculateNormals();

		return ms;
	}



	public static Mesh consolidateMeshes(List<Mesh> meshes){
        Mesh ms = new Mesh();
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector3> norms = new List<Vector3>();
        List<Color32> cols = new List<Color32>();

        for(int m=0; m<meshes.Count; m++ ){
            Mesh curr = meshes[m];

            for (int i=0; i < curr.triangles.Length; i++) {
                tris.Add(curr.triangles[i] + verts.Count);
            }
            norms.AddRange(curr.normals);
            verts.AddRange(curr.vertices);
            cols.AddRange(curr.colors32);

			//GameObject.DestroyImmediate(curr); //debug testing
        }

        ms.vertices = verts.ToArray();
        ms.triangles = tris.ToArray();
        ms.normals = norms.ToArray();
        ms.colors32 = cols.ToArray();

		//Testing to stop mem leak
		/*verts.Clear();
        tris.Clear();
        norms.Clear();
        cols.Clear();*/

        return ms;
    }


	public static Mesh CloneMesh(Mesh ms){
		return Object.Instantiate(ms);
		//Maybe later compare this speed with the speed of copying all the mesh arrays and bounds...
	}






	//MAKE A CENTER FUNCTION BY USING THE "translateMesh" AND "RecalculateMeshBounds" FUNCTIONS!


	public static Mesh translateMesh(Mesh ms, Vector3 dist) { //Note: you will need to call "RecalculateMeshBounds" after this. But future Rhett, don't make this function call "RecalculateMeshBounds" at the end becauase the user may want to scale, translate, and then fix the mesh bounds once afterwards to save CPU time!
		Mesh newMs = Object.Instantiate(ms); //Clone the old mesh...
		Vector3[] verts = newMs.vertices;
		//int[] tris = newMs.triangles;
		//Vector3[] norms = newMs.normals;
        //Color32[] col = newMs.colors32;

		for(int i=0; i<verts.Length; i++){
			//Vector3 newPos = verts[i] + dist;
			//verts[i] = newPos;
			verts[i] += dist;
		}

		newMs.vertices = verts;
		return newMs;
	}

	public static Mesh scaleMesh(Mesh ms, Vector3 scale) { //Note: you will need to call "RecalculateMeshBounds" after this. But future Rhett, don't make this function call "RecalculateMeshBounds" at the end becauase the user may want to scale, translate, and then fix the mesh bounds once afterwards to save CPU time!
		Mesh newMs = Object.Instantiate(ms);
		Vector3[] verts = newMs.vertices;

		for(int i=0; i<verts.Length; i++){
			Vector3 scaledVect = new Vector3(verts[i].x *scale.x, verts[i].y *scale.y, verts[i].z *scale.z);
			verts[i] = scaledVect;
		}

		newMs.vertices = verts;
		return newMs;
	}

	public static Mesh RecalculateMeshBounds(Mesh ms){
		Mesh newMs = Object.Instantiate(ms);
		Vector3[] verts = newMs.vertices;

		//Largest psotion value to a default based a real vert position.
		float xMax=verts[0].x;
		float yMax=verts[0].y;
		float zMax=verts[0].z;
		

		Bounds b = new Bounds();
		b.center = new Vector3(0,0,0); //Just make the bound's center the origin of the mesh...

		for(int i=0; i<verts.Length; i++){
			//Find the largest positions in the mesh pos or neg...
			xMax = ( Mathf.Abs(verts[i].x) >= xMax ) ? Mathf.Abs(verts[i].x) : xMax;
			yMax = ( Mathf.Abs(verts[i].y) >= yMax ) ? Mathf.Abs(verts[i].y) : yMax;
			zMax = ( Mathf.Abs(verts[i].z) >= zMax ) ? Mathf.Abs(verts[i].z) : zMax;
		}

		b.extents = new Vector3(xMax, yMax, zMax); //The new mesh bounds will exend to the largest vert positions on the map. Which will allow the camera to render it in the correct position!
		newMs.bounds = b;
		
		return newMs;
	}


	public static void SetMeshToVertexHelper(ref VertexHelper vh, Mesh ms){
		vh.Clear();

		Vector3[] verts = ms.vertices;
		Color32[] cols = ms.colors32;
		int[] tris = ms.triangles;
		for(int i=0; i<verts.Length; i++){
			UIVertex uiVert = UIVertex.simpleVert;
			uiVert.position = verts[i];
			uiVert.color = cols[i];
			vh.AddVert(uiVert);
		}

		for(int i=0; i<tris.Length; i+=3){
			vh.AddTriangle( tris[i], tris[i+1], tris[i+2] );
		}

		////GameObject.DestroyImmediate(ms); //this line was the problem...

		//vh is now ready and set with this rendered mesh.
	}


}