using UnityEngine;


/// <summary> This class is added by <see cref="DrawMesh.RenderToGameObject(string)"/> to stop a mesh-memory leak if when the gameObejct is destroyed.</summary>
public class AutoCleanMeshRender : MonoBehaviour {
    
    void OnDestroy(){
        //Debug.Log("Total Mesh Count: " + Resources.FindObjectsOfTypeAll<Mesh>().Length); //This line will log how many total meshes are loaded. If the number keeps getting bigger there is a memory leak some where in mesh code.

        MeshFilter mf = GetComponent<MeshFilter>();
        if(mf) { //Don't try to destroy the mesh if there is no MeshFilter!
            Destroy(mf.sharedMesh);
        }
    }
}
