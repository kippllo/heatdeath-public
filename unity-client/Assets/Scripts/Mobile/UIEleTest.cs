//Help:
//  https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.Graphic.html#UnityEngine_UI_Graphic_s_Mesh
//  https://docs.unity3d.com/2018.1/Documentation/ScriptReference/UI.Graphic.html
//  https://docs.unity3d.com/2018.1/Documentation/ScriptReference/UI.Graphic.OnPopulateMesh.html
//  https://docs.unity3d.com/2018.1/Documentation/ScriptReference/UI.VertexHelper.html


using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;

public class UIEleTest : Graphic //UIBehaviour, ICanvasElement
{

    public float radius = 50f;
    public float outlineThickness = 5f;
    public int segmentCount = 30;
    
    DrawMesh meshCanvas;


    /* protected override void Start() {
        meshCanvas = new DrawMesh();
        meshCanvas.segmentCount = 30;

        Vector2 joystickCenter = gameObject.GetComponent<RectTransform>().position;
        Color32 outlineCol = new Color32(255,255,255, 255);
        //float touchZoneDisplayBoundScaled = touchZoneDisplayBound*UIScale;
        float touchZoneDisplayBoundScaled = 5;
        meshCanvas.DrawCircleOutline(joystickCenter, touchZoneDisplayBoundScaled, 1f, outlineCol);

        Mesh uiMesh = meshCanvas.RenderToMesh(); //Check for mesh leak... Might have to destroy...

        s_Mesh = uiMesh;
        SetVerticesDirty();
        //CanvasRenderer uiRend = gameObject.GetComponent<CanvasRenderer>();
        //uiRend.SetMesh(uiMesh);
    } */



    protected override void OnPopulateMesh(VertexHelper vh) {

        meshCanvas = new DrawMesh();
        meshCanvas.segmentCount = segmentCount;

        //Vector2 joystickCenter = gameObject.GetComponent<RectTransform>().position;
        //Color32 outlineCol = new Color32(255,255,255, 255);
        //float touchZoneDisplayBoundScaled = touchZoneDisplayBound*UIScale;

        Color32 outlineCol = color; //Grab the color from the "Graphic" property.
        Vector2 joystickCenter = Vector3.zero;
        //float touchZoneDisplayBoundScaled = 50;
        //float thickness = 5f;
        meshCanvas.DrawCircleOutline(joystickCenter, radius, outlineThickness, outlineCol);

        //Mesh uiMesh = meshCanvas.RenderToMesh();

        vh.Clear();
        meshCanvas.RenderToVertexHelper(ref vh);
        
        //Vector2 fixedSize = new Vector2(2*touchZoneDisplayBoundScaled + 2*thickness, 2*touchZoneDisplayBoundScaled + 2*thickness);

        //Below will resize the rect's width and height to match the circle, but it is erroring right now, so leave it out...
        ////Vector2 fixedSize = new Vector2(2*radius + 2*outlineThickness, 2*radius + 2*outlineThickness);
        ////rectTransform.sizeDelta = fixedSize;
        
    }

    //void Update() {   
    //}
}
