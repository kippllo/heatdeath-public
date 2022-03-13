using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Keybored.Mobile {


    /// <summary>
    /// This is meant to be used like a mouse scrollwheel inout.
    /// It can be checked to see the change in the pinch amount.
    /// It will tell both positive and negative zoom values.
    /// </summary>
    public class PinchZoom : MonoBehaviour {
        
        public bool fullscreen = false;
        public int boundsWidth;
        public int boundsHeight;
        public Vector2 boundsStartPos;

        float getZoom;
        public float GetZoom { //readonly!
            get {return getZoom;}
        }

        public float oldDist;


        //MAYBE ADD SCREEN SCALE OPTION


        //void Start() {         
        //}

        void Update() {
            //Reset the export value.
            getZoom = 0f;
            
            Touch[] touches = TouchLib.getCurrentTouches();
            //We are going to check only the first two touches within the bounds.

            if(touches.Length == 2){ //WAS: "if(touches.Length >= 2){" But I changed it because this script only read the first two touches anyway. I'll have to make a more advanced version later if I want multiple touch inputs happening at the same time...
                Vector2 touchPos1 = touches[0].position;
                Vector2 touchPos2 = touches[1].position;

                bool pos1ValidX = touchPos1.x >= boundsStartPos.x && touchPos1.x <= boundsStartPos.x+boundsWidth;
                bool pos1ValidY = touchPos1.y >= boundsStartPos.y && touchPos1.y <= boundsStartPos.y+boundsHeight;

                bool pos2ValidX = touchPos2.x >= boundsStartPos.x && touchPos2.x <= boundsStartPos.x+boundsWidth;
                bool pos2ValidY = touchPos2.y >= boundsStartPos.y && touchPos2.y <= boundsStartPos.y+boundsHeight;

                if( (pos1ValidX && pos1ValidY && pos2ValidX && pos2ValidY) || fullscreen ){ //Only check the input if the touches are within bounds.
                    
                    float curDist = TouchLib.distance(touchPos1, touchPos2);

                    //If the old distance is not set, set it to the current distance.
                    oldDist = (oldDist == 0) ? curDist: oldDist;

                    getZoom = curDist - oldDist; //Find the zoom value based on the distance change between the two points.

                    oldDist = curDist; //Update the old distance for the next frame.
                }
            } else {
                oldDist = 0; //If the user is not zooming, reset the "oldDist" to its initial value. This will stop a dramatic zoom when the user starts zooming again.
            }

            //Debug.Log("Touch Zoom: " + getZoom);
        }
    }

}