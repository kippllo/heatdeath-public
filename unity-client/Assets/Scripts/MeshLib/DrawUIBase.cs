using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DrawUIBase : Graphic {
    private Mesh _mesh;
    public Mesh mesh {
        get{ return _mesh;}
        set { 
            DestroyImmediate(_mesh); //Clean up the old mesh...

            _mesh = value;
            //_mesh = MeshLib.CloneMesh(value);
            //DestroyImmediate(value);

            UpdateGeometry(); //redraw this shape based on the new mesh.
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh) {
        // "vh.Clear();" inst part of the below function...
        MeshLib.SetMeshToVertexHelper(ref vh, mesh);
    }

    protected override void OnDestroy(){
        // This is a work around to prevent mesh memory leaks!
        // Otherwise you would need to call the below to prevent a mesh memory leak:
        //      DestroyImmediate(drawUIBase.GetComponent<DrawUIBase>().mesh);
        //      Destroy(drawUIBase);
        
        //Note: "OnDestroy" is only called on game objects that have previously been active.
        //  Keep this in mind so you don't cause a mesh memory leak.
        DestroyImmediate(_mesh);
    }

}
