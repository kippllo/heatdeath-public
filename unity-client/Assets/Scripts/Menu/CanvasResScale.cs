using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasResScale : MonoBehaviour {
    
    const int widthPC = 1280;
    const int heightPC = 720;

    const int widthMobile = 800;
    const int heightMobile = 480;

    CanvasScaler scaler;

    void Start() {
        scaler = gameObject.GetComponent<CanvasScaler>();

        //Setup the "CanvasScaler" base settings so it will work with screen size scale.
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        //There are two different scale ratios. One for PC, one for Mobile.
        #if !UNITY_ANDROID && !UNITY_IOS
            scaler.referenceResolution = new Vector2(widthPC, heightPC);
        #else
            scaler.referenceResolution = new Vector2(widthMobile, heightMobile);
        #endif

    }


    //void Update() {
    //}
}
