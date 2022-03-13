using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Keybored.Mobile {


    /// <summary>
    /// This is meant to be used like a button input.
    /// It can be checked to tell if a user has touched inside a certain area on the screen.
    /// </summary>
    public class TouchTap : MonoBehaviour {
        
        public int width;
        public int height;
        public Vector2 startPos;

        bool getTapLastFrame = false;

        bool getTap = false;
        public bool GetTap { //readonly!
            get {return getTap;}
        }

        bool getTapDown = false;
        public bool GetTapDown {
            get {return getTapDown;}
        }

        bool getTapUp = false;
        public bool GetTapUp {
            get {return getTapUp;}
        }
        
        //void Start() {
        //}

        void Update() {

            

            //Reset the tap bool
            getTap = false;
            
            Touch[] touches = TouchLib.getCurrentTouches();
            for (int i = 0; i < touches.Length; i++) {
                Vector2 screenPos = touches[i].position;

                bool validX = screenPos.x >= startPos.x && screenPos.x <= startPos.x+width;
                bool validY = screenPos.y >= startPos.y && screenPos.y <= startPos.y+height;
                if(validX && validY) {
                    getTap = true;
                }

                //Debug.Log("Touch Tap: " + getTap + "Top Pos: " + screenPos.ToString());
            }




            //Debug.Log("getTap: " + getTap);
            //if(getTapUp) { //Debug remove later
            //    Debug.Log("getTapUp: " + getTapUp);  //Debug remove later
            //}
            
            //UNDO//Debug.Log("getTapUp: " + getTapUp);

            
            //"getTapDown" is only true the first frame that "getTap" is true.
            //getTapDown = (getTap && !getTapDown && !getTapLastFrame) ? true : false;
            getTapDown = (getTap && !getTapLastFrame) ? true : false;

            //"getTapUp" is only true the first frame that "getTap" is false.
            //UNDO//getTapUp = (!getTap && !getTapUp) ? true : false;
            getTapUp = (!getTap && getTapLastFrame) ? true : false;


            getTapLastFrame = getTap;

            
        }
    }

}