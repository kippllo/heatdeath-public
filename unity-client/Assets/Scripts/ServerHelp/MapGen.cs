using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class cube{
    //These should be the position bottom left corner.
    public Vector3Thin pos;

    public float x {
        get{
            return pos.x;
        }
        set{
            pos.x = value;
        }
    }
    public float y {
        get{
            return pos.y;
        }
        set{
            pos.y = value;
        }
    }
    public float z {
        get{
            return pos.z;
        }
        set{
            pos.z = value;
        }
    }

    public Vector3Thin size;
    public float sizeX {
        get{
            return size.x;
        }
        set{
            size.x = value;
        }
    }
    public float sizeY {
        get{
            return size.y;
        }
        set{
            size.y = value;
        }
    }
    public float sizeZ {
        get{
            return size.z;
        }
        set{
            size.z = value;
        }
    }

    //For now all cubes will be one color, but if I want to change this later just make a function that sets these 8 Color32Thin separately.
    public Color32Thin[] vertColors; //There will always be 8 vertices in a cube.
    

    public cube(){
        pos = new Vector3Thin(0,0,0);
        size = new Vector3Thin(1,1,1);
        setAllVertColor(255,255,255,255);
    }

    public cube(float x, float y, float z, float sX, float sY, float sZ){
        pos = new Vector3Thin(x,y,z);
        size = new Vector3Thin(sX,sY,sZ);
        setAllVertColor(255,255,255,255);
    }

    public cube(float x, float y, float z, float sX, float sY, float sZ, byte r, byte g, byte b, byte a){
        pos = new Vector3Thin(x,y,z);
        size = new Vector3Thin(sX,sY,sZ);
        setAllVertColor(r,g,b,a);
    }

    public cube(float x, float y, float z, float sX, float sY, float sZ, Color32Thin vertColorIN){
        pos = new Vector3Thin(x,y,z);
        size = new Vector3Thin(sX,sY,sZ);
        setAllVertColor(vertColorIN.r, vertColorIN.g, vertColorIN.b, vertColorIN.a);
    }

    public void setAllVertColor(byte r, byte g, byte b, byte a){
        //Set the array back to a fresh start
        vertColors = new Color32Thin[8];

        Color32Thin c = new Color32Thin(r,g,b,a);

        //Unwrapped loop
        vertColors[0] = c;
        vertColors[1] = c;
        vertColors[2] = c;
        vertColors[3] = c;
        vertColors[4] = c;
        vertColors[5] = c;
        vertColors[6] = c;
        vertColors[7] = c;
    }

}



public class MapData {

    public int width; //Outside wall bounds will be at these limits and at (0,0,0).
    public int height;

    public cube[] cubes; //These will be added to the main mesh and they will be used to spawn box colliders
    public Vector3Thin[] spawnPoints;
    //Could set other objects like enemy positions, items, and buildings here just like the cubes!

    //Don't need to convert an array, because this whole class will be serialized and sent from the server. So just deserialize it.
    public MapData(){
        width = 0;
        height = 0;
        cubes = new cube[0];
        spawnPoints = new Vector3Thin[0];
    }

    public GameObject GenMap(){
        GameObject map = new GameObject("map"); //Note: New GameObject always spawns at (0,0,0).

        //Gen Mesh
        List<Mesh> mapMeshes = new List<Mesh>(); //Holds all meshes for that map, will later be consolidated into single mesh.

        //Make the border meshes.
        float thickness = 0.5f;
        float zHeight = 5.0f; //How tall the border walls are in the up direction. Should just be tall enough for player to not leave.
        Color32Thin wallColor = new Color32Thin(255,0,0,255);
        cube leftWall = new cube(0,0,0, thickness, height, zHeight, wallColor); //Walls of plains, zero units thick.
        cube rightWall = new cube(width,0,0, thickness, height, zHeight, wallColor);
        cube botWall = new cube(0,0,0, width, thickness, zHeight, wallColor); //Remember, all cube positions start at the left hand corner, not the center of the cube. So no need for "width/2" in the position or anything!
        cube topWall = new cube(0,height,0, width+thickness, thickness, zHeight, wallColor); //"width+thickness" will ifx the top-right corner issue. In which it had an odd looking blank space.

        ////mapMeshes.Add( MeshLib.GenCube(leftWall) );
        ////mapMeshes.Add( MeshLib.GenCube(rightWall) );
        ////mapMeshes.Add( MeshLib.GenCube(botWall) );
        ////mapMeshes.Add( MeshLib.GenCube(topWall) );

        Vector3 wallTranslateVect = new Vector3(-width/2, -height/2, -zHeight/2); //Vector3 wallTranslateVect = new Vector3(-width/2, -height/2, 0);
        Mesh leftWallTranlated = MeshLib.translateMesh( MeshLib.GenCube(leftWall), wallTranslateVect );
        Mesh rightWallTranlated = MeshLib.translateMesh( MeshLib.GenCube(rightWall), wallTranslateVect );
        Mesh botWallTranlated = MeshLib.translateMesh( MeshLib.GenCube(botWall), wallTranslateVect );
        Mesh topWallTranlated = MeshLib.translateMesh( MeshLib.GenCube(topWall), wallTranslateVect );
        mapMeshes.Add( leftWallTranlated );
        mapMeshes.Add( rightWallTranlated );
        mapMeshes.Add( botWallTranlated );
        mapMeshes.Add( topWallTranlated );
        //Just leave the walls uncentered.


        //Add the map's regular cubes.
        for(int c=0; c<cubes.Length; c++){

            Mesh m = MeshLib.GenCube(cubes[c]);
            Vector3 translateVect = new Vector3(-cubes[c].sizeX/2, -cubes[c].sizeY/2, -cubes[c].sizeZ/2); //Center the cube.
		    m = MeshLib.translateMesh(m, translateVect);
            
            mapMeshes.Add( m );
            //mapMeshes.Add( MeshLib.GenCube(cubes[c]) );
        }

        
        //Consolidate meshes
        Mesh consolMesh = MeshLib.consolidateMeshes(mapMeshes);
        
        //Attach Mesh to gameObject.
		map.AddComponent<MeshFilter>().mesh = consolMesh;
        Material mat = new Material(Shader.Find("Custom/VertColor")); //Be sure that "Custom/VertColor" shader is included in the "Always Included Shaders" array in "ProjectSettings/Graphics". Help: https://docs.unity3d.com/ScriptReference/Shader.Find.html
		map.AddComponent<MeshRenderer>().material = mat;

        map.AddComponent<mapCtrl>(); //Add the local collision script...
        map.layer = 10; //Set the culling layer to "map", which is layer 10 in this case.


        //Gen colliders
        for(int m=0; m<mapMeshes.Count; m++ ){
            GameObject temp = new GameObject("temp");
            temp.AddComponent<MeshFilter>().mesh = mapMeshes[m];
            temp.AddComponent<MeshRenderer>(); //This is needed for the collider to auto resize to the mesh...
            BoxCollider boxCollid = temp.AddComponent<BoxCollider>();

            BoxCollider boxCollidMap = map.AddComponent<BoxCollider>();

            Vector3 translateVect = Vector3.zero;
            if(m > 3){ //Skip the first 4 index bceause this transform doesn't need to be applied to default walls.
                //  Note: All cubes placed by the map editor must translate their box collider just like they did their mesh!
                //      I'm not sure why the box collider don't auto adjust to the translated mesh positions. A mesh collider does auto adjust.
                //      So, for now just fix the box collider below:
                translateVect = new Vector3(-boxCollid.size.x/2, -boxCollid.size.y/2, -boxCollid.size.z/2);
            } else {
                //If we are on the walls, apply their proper translation.
                translateVect = wallTranslateVect;
            }

            boxCollidMap.center = boxCollid.center + translateVect; //Copy collider stats to the one on the map object...
            boxCollidMap.size = boxCollid.size;

            Object.Destroy(temp); //I have to call destroy like this becaause I'm not in a "MonoBehaviour", i think.
        }

        return map;
    }

}