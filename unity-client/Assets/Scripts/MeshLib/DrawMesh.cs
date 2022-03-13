using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DrawMesh {

	public List<DrawMeshTriangle> triangles;
	public float zLayer;
	public int segmentCount; //Used for how HD a circle is!

	Material mat; //Caching this does not stop the memory leak, but it is better for optimization!


	public DrawMesh(){
		triangles = new List<DrawMeshTriangle>();
		zLayer = 0;
		segmentCount = 8;

		mat = new Material(Shader.Find("Custom/VertColor"));
	}

	/*public DrawMesh(int canvasWidth, int canvasHeight){
		triangles = new List<DrawMeshTriangle>();
	}*/



	public void DrawTriangle(Vector3 vert0, Vector3 vert1, Vector3 vert2){
		Color32 col = new Color32(255,255,255,255);
		DrawTriangle(vert0, vert1, vert2, col);
		//DrawMeshTriangle tri = new DrawMeshTriangle(vert0, vert1, vert2, col);
		//triangles.Add(tri);
	}
	public void DrawTriangle(Vector3 vert0, Vector3 vert1, Vector3 vert2, Color32 triColor){
		//Color32 col = new Color32(255,255,255,255);
		DrawMeshTriangle tri = new DrawMeshTriangle(vert0, vert1, vert2, triColor);
		triangles.Add(tri);
	}


	public void DrawLine(Vector3 pos1, Vector3 pos2, float width, Color32 lineColor){
		//Note: in this line, the width goes out from both sides instead of just out in the positive direction!
		
		Vector3 tri0_vert0 = new Vector3(pos1.x - width/2, pos1.y, zLayer);
		Vector3 tri0_vert1 = new Vector3(pos1.x + width/2, pos1.y, zLayer);
		Vector3 tri0_vert2 = new Vector3(pos2.x - width/2, pos2.y, zLayer);

		Vector3 tri1_vert0 = new Vector3(pos2.x - width/2, pos2.y, zLayer);
		Vector3 tri1_vert1 = new Vector3(pos2.x + width/2, pos2.y, zLayer);
		Vector3 tri1_vert2 = new Vector3(pos1.x + width/2, pos1.y, zLayer);


		DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, lineColor);
		DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, lineColor);

		triangles.Add(tri0);
		triangles.Add(tri1);
	}


	public void DrawSquare(Vector3 pos1, float width, float height, Color32 lineColor){
		// "pos1" is the bottom left have corner of the square.

		Vector3 tri0_vert0 = new Vector3(pos1.x, pos1.y, zLayer);
		Vector3 tri0_vert1 = new Vector3(pos1.x + width, pos1.y, zLayer);
		Vector3 tri0_vert2 = new Vector3(pos1.x, pos1.y + height, zLayer);

		Vector3 tri1_vert0 = new Vector3(pos1.x + width, pos1.y + height, zLayer);
		Vector3 tri1_vert1 = new Vector3(pos1.x + width, pos1.y, zLayer);
		Vector3 tri1_vert2 = new Vector3(pos1.x, pos1.y + height, zLayer);


		DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, lineColor);
		DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, lineColor);

		triangles.Add(tri0);
		triangles.Add(tri1);
	}


	/*public void RemoveSquare(Vector3 pos1, float width, float height, Color32 lineColor){
		// "pos1" is the bottom left have corner of the square.
		Vector3 tri0_vert0 = new Vector3(pos1.x, pos1.y, zLayer);
		Vector3 tri0_vert1 = new Vector3(pos1.x + width, pos1.y, zLayer);
		Vector3 tri0_vert2 = new Vector3(pos1.x, pos1.y + height, zLayer);

		Vector3 tri1_vert0 = new Vector3(pos1.x + width, pos1.y + height, zLayer);
		Vector3 tri1_vert1 = new Vector3(pos1.x + width, pos1.y, zLayer);
		Vector3 tri1_vert2 = new Vector3(pos1.x, pos1.y + height, zLayer);

		int triCount = triangles.Count;
		for(int t=0; t<triangles; t++){
			//if(triangles[t].position is in square bounds...){

			}
		}


		DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, lineColor);
		DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, lineColor);

		triangles.Add(tri0);
		triangles.Add(tri1);
	}*/


	public void DrawPixelCircle(Vector3 center, float radius, float pixelWidth, float pixelHeight, Color32 circleColor){
		//Make an octagon with more sides...

		//Theta will just be 0-360 for a cirlce...
		
		float twoPi = 2*Mathf.PI;
		float thetaStep = twoPi/(float)segmentCount; //The amount theta is stepped each loop.
		
		for(float th=0; th<segmentCount; th += thetaStep){ //Was: th<twoPi;
			
			float x = radius * Mathf.Cos(th);
			float y = radius * Mathf.Sin(th);
			Vector3 point = new Vector3(x, y, zLayer);

			//Dedbug (debug)
			DrawSquare(point, pixelWidth, pixelHeight, circleColor); //Just testing!
		}

		//int yMax = center.y + radius;
		//int xMax = center.x + radius;
	}


	public void DrawCircleOutline(Vector3 center, float radius, float outlineThickness, Color32 circleColor){
		//Make an octagon with more sides...

		//Theta will just be 0-360 for a cirlce...
		
		const float twoPi = 2*Mathf.PI;
		//float thetaStep = twoPi/(float)segmentCount; //The amount theta is stepped each loop.

		Vector3 lastPosLeft = Vector3.zero;
		Vector3 lastPosRight = Vector3.zero;

		int totalVertCount = segmentCount*2;
		Vector3[] verts = new Vector3[totalVertCount];
		
		for(int i=0; i<segmentCount; i++){ //for(float th=0; th<segmentCount; th += thetaStep){ //Was: th<twoPi;
			
			//Find the current theta based on the number of step it would take to reach 2Pi and step number we are on.
			float th = (twoPi/(float)segmentCount) * i;

			//Point close to circle center.
			float x1 = radius * Mathf.Cos(th);
			float y1 = radius * Mathf.Sin(th);
			Vector3 point1 = new Vector3(x1, y1, zLayer) + center; //Don't forget to transform the point by the center!

			//Point away from circle center.
			float x2 = (radius+outlineThickness) * Mathf.Cos(th);
			float y2 = (radius+outlineThickness) * Mathf.Sin(th);
			Vector3 point2 = new Vector3(x2, y2, zLayer) + center;

			verts[i] = point1;
			verts[i+segmentCount] = point2;
		}


		//Actually Draw the Circle from the points found!
		for(int i=0; i<segmentCount; i++){
			//Define Var to be used.
			Vector3 tri0_vert0 = Vector3.zero;
			Vector3 tri0_vert1 = Vector3.zero;
			Vector3 tri0_vert2 = Vector3.zero;
			Vector3 tri1_vert0 = Vector3.zero;
			Vector3 tri1_vert1 = Vector3.zero;
			Vector3 tri1_vert2 = Vector3.zero;


			//If we are on any other segment besides the last one...
			if(i != segmentCount-1){
				//Note: the below side names are only valid for the first square!
				tri0_vert0 = verts[i]; //Top Left
				tri0_vert1 = verts[i+1]; //Bottom Left
				tri0_vert2 = verts[i+segmentCount]; //Top Right

				tri1_vert0 = verts[i+segmentCount]; //Top Right
				tri1_vert1 = verts[i+1]; //Bottom Left
				tri1_vert2 = verts[i+1+segmentCount]; //Bottom Right
			}

			//If we are on the last square segment, loop back to the first verts...
			else{
				tri0_vert0 = verts[i];
				tri0_vert1 = verts[0];
				tri0_vert2 = verts[i+segmentCount];
				tri1_vert0 = verts[i+segmentCount];
				tri1_vert1 = verts[0];
				tri1_vert2 = verts[0+segmentCount];
			}

			
			// Debug Gradient Circle Color
			/*byte r = (byte)(255* i/segmentCount);
			byte g = (byte)(255* i/segmentCount);
			byte b = (byte)(255* i/segmentCount);
			Color32 debugColor = new Color32(r, g, b, 255);
			DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, debugColor);
			DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, debugColor);*/


			DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, circleColor);
			DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, circleColor);
			
			triangles.Add(tri0);
			triangles.Add(tri1);
		}
	}


	public void DrawSquareWithCircleHole(Vector3 sqCenter, float sqWidth, float sqHeight, Vector3 cirlceCenter, float cirlceRadius, Color32 sqColor){
		//NOTE: For this function to properly "segmentCount" must be 6 or higher!
		
		const float twoPi = 2*Mathf.PI;
		Vector3 lastPosLeft = Vector3.zero;
		Vector3 lastPosRight = Vector3.zero;
		int totalVertCount = segmentCount*2;
		Vector3[] verts = new Vector3[totalVertCount];
		
		for(int i=0; i<segmentCount; i++){
			float th = (twoPi/(float)segmentCount) * i;

			float x1 = cirlceRadius * Mathf.Cos(th);
			float y1 = cirlceRadius * Mathf.Sin(th);

			//Add the circle center offset to the inner circle points. Also, add translate by the sqCenter to account for local space. If I don't the cirlce might get cut off in the middle of the square!
			x1 += cirlceCenter.x;
			y1 += cirlceCenter.y;

			//Limit the inner cirlce vert, if they have been moved outside of the square!
			x1 = (x1 > sqWidth/2+sqCenter.x) ? sqWidth/2+sqCenter.x : x1;
			x1 = (x1 < -sqWidth/2-sqCenter.x) ? -sqWidth/2-sqCenter.x : x1;
			y1 = (y1 > sqHeight/2+sqCenter.y) ? sqHeight/2+sqCenter.y : y1;
			y1 = (y1 < -sqHeight/2-sqCenter.y) ? -sqHeight/2-sqCenter.y : y1;

			Vector3 point1 = new Vector3(x1, y1, zLayer);

			//Make these points are outside of the square limits.
			float distBetweenCenters = Vector3Thin.distance(Vector3Thin.FromVector3(cirlceCenter), Vector3Thin.FromVector3(sqCenter));
			//float x2 = (distBetweenCenters*2+cirlceRadius) * Mathf.Cos(th);
			//float y2 = (distBetweenCenters*2+cirlceRadius) * Mathf.Sin(th);
			//Debug.Log("distBetweenCenters: " + distBetweenCenters);
			
			//Vector3 distCorrection = cirlceCenter - sqCenter;
			//float x2 = (cirlceRadius+cirlceCenter.x) * Mathf.Cos(th);
			//float y2 = (cirlceRadius+cirlceCenter.y) * Mathf.Sin(th);
			
			//float x2 = (cirlceRadius*2+sqWidth+cirlceCenter.x) * Mathf.Cos(th);
			//float y2 = (cirlceRadius*2+sqHeight+cirlceCenter.y) * Mathf.Sin(th);

			//float x2 = (cirlceRadius+sqWidth) * Mathf.Cos(th);
			//float y2 = (cirlceRadius+sqHeight) * Mathf.Sin(th);

			float x2 = (cirlceRadius+sqWidth+distBetweenCenters*3) * Mathf.Cos(th); //Was: "distBetweenCenters*4" this number is just a boost to make sure all the outside circle verts start with a position outside of the square!
			float y2 = (cirlceRadius+sqHeight+distBetweenCenters*3) * Mathf.Sin(th);

			x2 += cirlceCenter.x + sqCenter.x; //Base the outer circle off of the circle's center at first too.
			y2 += cirlceCenter.y + sqCenter.y;
			
			//Limit the second verts so they make a square!
			x2 = (x2 > sqWidth/2) ? sqWidth/2 : x2;
			x2 = (x2 < -sqWidth/2) ? -sqWidth/2 : x2;

			y2 = (y2 > sqHeight/2) ? sqHeight/2 : y2;
			y2 = (y2 < -sqHeight/2) ? -sqHeight/2 : y2;

			Vector3 point2 = new Vector3(x2, y2, zLayer); // + sqCenter; //Don't translate the outer circle until here so it is already in the shape of the square. If I translated ot before, I would also need to translate "sqWidth" and "sqHeight" as well.

			verts[i] = point1;
			verts[i+segmentCount] = point2;
		}


		for(int i=0; i<segmentCount; i++){
			Vector3 tri0_vert0 = Vector3.zero;
			Vector3 tri0_vert1 = Vector3.zero;
			Vector3 tri0_vert2 = Vector3.zero;
			Vector3 tri1_vert0 = Vector3.zero;
			Vector3 tri1_vert1 = Vector3.zero;
			Vector3 tri1_vert2 = Vector3.zero;

			if(i != segmentCount-1){
				tri0_vert0 = verts[i];
				tri0_vert1 = verts[i+1];
				tri0_vert2 = verts[i+segmentCount];

				tri1_vert0 = verts[i+segmentCount];
				tri1_vert1 = verts[i+1];
				tri1_vert2 = verts[i+1+segmentCount];
			}

			else{
				tri0_vert0 = verts[i];
				tri0_vert1 = verts[0];
				tri0_vert2 = verts[i+segmentCount];
				tri1_vert0 = verts[i+segmentCount];
				tri1_vert1 = verts[0];
				tri1_vert2 = verts[0+segmentCount];
			}

			DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, sqColor);
			DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, sqColor);
			
			triangles.Add(tri0);
			triangles.Add(tri1);
		}



	}


	public void DrawCircleOutlineInBox(Vector3 sqCenter, float sqWidth, float sqHeight, Vector3 cirlceCenter, float cirlceRadius, float circleOutlineThickness, Color32 sqColor){
		//Same as the above function, but does nott draw the square visibly.
		const float twoPi = 2*Mathf.PI;
		Vector3 lastPosLeft = Vector3.zero;
		Vector3 lastPosRight = Vector3.zero;
		int totalVertCount = segmentCount*2;
		Vector3[] verts = new Vector3[totalVertCount];
		
		for(int i=0; i<segmentCount; i++){
			float th = (twoPi/(float)segmentCount) * i;

			float x1 = cirlceRadius * Mathf.Cos(th);
			float y1 = cirlceRadius * Mathf.Sin(th);

			//Add the circle center offset to the inner circle points. Also, add translate by the sqCenter to account for local space. If I don't the cirlce might get cut off in the middle of the square!
			x1 += cirlceCenter.x;
			y1 += cirlceCenter.y;

			//Limit the inner cirlce vert, if they have been moved outside of the square!
			x1 = (x1 > sqWidth/2+sqCenter.x) ? sqWidth/2+sqCenter.x : x1;
			x1 = (x1 < -sqWidth/2-sqCenter.x) ? -sqWidth/2-sqCenter.x : x1;
			y1 = (y1 > sqHeight/2+sqCenter.y) ? sqHeight/2+sqCenter.y : y1;
			y1 = (y1 < -sqHeight/2-sqCenter.y) ? -sqHeight/2-sqCenter.y : y1;

			Vector3 point1 = new Vector3(x1, y1, zLayer);


			float x2 = (cirlceRadius+circleOutlineThickness) * Mathf.Cos(th);
			float y2 = (cirlceRadius+circleOutlineThickness) * Mathf.Sin(th);

			x2 += cirlceCenter.x; // + sqCenter.x; //Base the outer circle off of the circle's center at first too.
			y2 += cirlceCenter.y; // + sqCenter.y;
			
			x2 = (x2 > sqWidth/2) ? sqWidth/2 : x2;
			x2 = (x2 < -sqWidth/2) ? -sqWidth/2 : x2;
			y2 = (y2 > sqHeight/2) ? sqHeight/2 : y2;
			y2 = (y2 < -sqHeight/2) ? -sqHeight/2 : y2;

			Vector3 point2 = new Vector3(x2, y2, zLayer); // + sqCenter; //Don't translate the outer circle until here so it is already in the shape of the square. If I translated ot before, I would also need to translate "sqWidth" and "sqHeight" as well.

			verts[i] = point1;
			verts[i+segmentCount] = point2;
		}


		for(int i=0; i<segmentCount; i++){
			Vector3 tri0_vert0 = Vector3.zero;
			Vector3 tri0_vert1 = Vector3.zero;
			Vector3 tri0_vert2 = Vector3.zero;
			Vector3 tri1_vert0 = Vector3.zero;
			Vector3 tri1_vert1 = Vector3.zero;
			Vector3 tri1_vert2 = Vector3.zero;

			if(i != segmentCount-1){
				tri0_vert0 = verts[i];
				tri0_vert1 = verts[i+1];
				tri0_vert2 = verts[i+segmentCount];

				tri1_vert0 = verts[i+segmentCount];
				tri1_vert1 = verts[i+1];
				tri1_vert2 = verts[i+1+segmentCount];
			}

			else{
				tri0_vert0 = verts[i];
				tri0_vert1 = verts[0];
				tri0_vert2 = verts[i+segmentCount];
				tri1_vert0 = verts[i+segmentCount];
				tri1_vert1 = verts[0];
				tri1_vert2 = verts[0+segmentCount];
			}

			DrawMeshTriangle tri0 = new DrawMeshTriangle(tri0_vert0, tri0_vert1, tri0_vert2, sqColor);
			DrawMeshTriangle tri1 = new DrawMeshTriangle(tri1_vert0, tri1_vert1, tri1_vert2, sqColor);
			
			triangles.Add(tri0);
			triangles.Add(tri1);
		}
	}




	//public void DrawHollowCircle(Vector3 center, Vector3 radius, float thickness, Color32 circleColor){}
	//public void DrawSquareWithCircleHole(Vector3 center, Vector3 radius, float thickness, Color32 circleColor){}


	public void clear(){
		//triangles.Clear(); //Maybe remove this did not fix the memory leak... //Don't forget to clear lists when you're done with them to prevent memory leaks!
		triangles = new List<DrawMeshTriangle>();
	}

	public Mesh RenderToMesh(){
		//----------------------------Memory leak Start
		List<Mesh> meshes = new List<Mesh>();

		int triCount = triangles.Count;
		for(int i=0; i<triCount; i++){
			Mesh m = new Mesh();
			m.vertices = triangles[i].getExportVerts();
			m.triangles = triangles[i].getExportOrder();
			m.colors32 = triangles[i].getExportColors();
			meshes.Add(m);
		}

		//----------------------------Memory leak End

		//Later consolidate all three of these loops into one if possible.
		Mesh exportMesh = MeshLib.consolidateMeshes(meshes);
		exportMesh.RecalculateBounds(); //exportMesh = MeshLib.RecalculateMeshBounds(exportMesh);
		
		//debug testing... (Maybe remove the bottom two lines...)
		for(int i=0; i<meshes.Count; i++){
			//meshes[i].Clear(false);
			//Debug.Log("vertices: " + meshes[i].vertices.Length);
			GameObject.Destroy(meshes[i]);
		}
		meshes.Clear(); //debug testing
		//!!!Center the mesh origin!!!
		
		//Investigate: exportMesh.CombineMeshes();
		//exportMesh.RecalculateNormals();
		//MeshLib.c

		//testing to stop mem leak
		//meshes.Clear();

		//The below line can be used to check for Mesh memory leaks! Help: https://docs.unity3d.com/ScriptReference/Resources.FindObjectsOfTypeAll.html
		//Debug.Log( Resources.FindObjectsOfTypeAll<Mesh>().Length ); //Keep this line as a comment!

		if(triCount*3 > 65535){
			Debug.LogWarning("Unity mesh vert limit hit!\nVerts: " + "65537" + "\nLimit: 65535");
		}

		
		return exportMesh;

		//Mesh debug = new Mesh(); //debug remove
		//return debug;


		//NOTE: Don't weld mesh verts because we want each triangle to have full color!
	}

	public GameObject RenderToGameObject(string name = "DrawMesh"){
		Mesh ms = RenderToMesh();
		
		GameObject meshGameObject = new GameObject(name); //Note: New GameObject always spawns at (0,0,0).
		meshGameObject.AddComponent<MeshFilter>().sharedMesh = ms;
		
		//testing to see if this is the memory leak... //Material mat = new Material(Shader.Find("Custom/VertColor")); //Be sure that "Custom/VertColor" shader is included in the "Always Included Shaders" array in "ProjectSettings/Graphics". Help: https://docs.unity3d.com/ScriptReference/Shader.Find.html
		meshGameObject.AddComponent<MeshRenderer>().sharedMaterial = mat; //meshGameObject.AddComponent<MeshRenderer>().material = mat;
		meshGameObject.AddComponent<AutoCleanMeshRender>(); //This script will take care of any memory leak that might happen when the gameObject is destroyed.

		return meshGameObject;




		//Use the below to test and make sure the memory leak is in the "RenderToMesh" function.
		//Mesh ms = RenderToMesh();
		//GameObject meshGameObject = new GameObject(name); //Note: New GameObject always spawns at (0,0,0).
		//meshGameObject.AddComponent<MeshFilter>().mesh = ms;
		//testing to see if this is the memory leak... //Material mat = new Material(Shader.Find("Custom/VertColor")); //Be sure that "Custom/VertColor" shader is included in the "Always Included Shaders" array in "ProjectSettings/Graphics". Help: https://docs.unity3d.com/ScriptReference/Shader.Find.html
		//meshGameObject.AddComponent<MeshRenderer>().material = mat;
		//return meshGameObject;
	}











	private Mesh RenderToMeshEditorSafe(){ //Maybe make protected...
		List<Mesh> meshes = new List<Mesh>();
		int triCount = triangles.Count;
		for(int i=0; i<triCount; i++){
			Mesh m = new Mesh();
			m.vertices = triangles[i].getExportVerts();
			m.triangles = triangles[i].getExportOrder();
			m.colors32 = triangles[i].getExportColors();
			meshes.Add(m);
		}

		Mesh exportMesh = MeshLib.consolidateMeshes(meshes);
		exportMesh.RecalculateBounds();

		for(int i=0; i<meshes.Count; i++){
			GameObject.DestroyImmediate(meshes[i]); //GameObject.Destroy(meshes[i]);
		}
		meshes.Clear();

		if(triCount*3 > 65535){
			Debug.LogWarning("Unity mesh vert limit hit!\nVerts: " + "65537" + "\nLimit: 65535");
		}

		return exportMesh;
	}

	public void RenderToVertexHelper(ref VertexHelper vh){ //was out...
		vh.Clear();
		Mesh ms = RenderToMeshEditorSafe();

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

		GameObject.DestroyImmediate(ms);

		//vh is now ready and set with this rendered mesh.

	}

}
