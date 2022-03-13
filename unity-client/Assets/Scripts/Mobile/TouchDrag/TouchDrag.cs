using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Keybored.Mobile {


    /// <summary>
    /// This control will tell the change in a user's finger drag on a screen.
    /// </summary>
    public class TouchDrag : MonoBehaviour {
        
        public int width;
        public int height;
        public Vector2 startPos;
        public bool fullscreen = false;

        private Vector2 screenPositionLastFrame = Vector2.positiveInfinity;

        private Vector2 screenPosition;
        public Vector2 ScreenPosition {
            get {return screenPosition;}
        }

        private Vector2 getDrag;
        public Vector2 GetDrag {
            get {return getDrag;}
        }


        //MAYBE ADD SCREEN SCALE OPTION
        
        //void Start() {
        //}


        void Update() {
            
            //Reset the drag output var.
            getDrag = Vector2.zero;

            Touch[] touches = TouchLib.getCurrentTouches();
            if(touches.Length == 1){ //WAS: "if(touches.Length >= 1){" I changed it so that it would not pan while the user was "pinchToZooming".
                Vector2 touchPos = touches[0].position;

                bool validX = touchPos.x >= startPos.x && touchPos.x <= startPos.x+width;
                bool validY = touchPos.y >= startPos.y && touchPos.y <= startPos.y+height;
                if( (validX && validY) || fullscreen ) {
                    
                    screenPosition = touchPos; //Set the static position var.
                    screenPositionLastFrame = (screenPositionLastFrame.x == Vector2.positiveInfinity.x && screenPositionLastFrame.y == Vector2.positiveInfinity.y) ? screenPosition: screenPositionLastFrame;
                    //screenPositionLastFrame = (screenPositionLastFrame == Vector2.positiveInfinity) ? screenPosition: screenPositionLastFrame; //WAS: screenPosition/1f  // "/1f" sets the last frame's position to a clone of the current's.
                    
                    //getDrag = screenPosition - screenPositionLastFrame; //Find the difference between the two points. A.K.A the drag amount for each axis.
                    getDrag = new Vector2(screenPosition.x - screenPositionLastFrame.x, screenPosition.y - screenPositionLastFrame.y);

                    screenPositionLastFrame = screenPosition; //Was: screenPosition/1f
                }
            } else {
                screenPositionLastFrame = Vector2.positiveInfinity; //Reset the LastFrame's position to its initial value.
            }

            //Debug.Log("getDrag: " + getDrag.ToString());
            //Debug.Log("screenPosition: " + screenPosition.ToString());
            //Debug.Log("screenPositionLastFrame: " + screenPositionLastFrame.ToString());
        }
    }
}