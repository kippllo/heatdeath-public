using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; //For math...


namespace Keybored.Mobile {

    public static class TouchLib {
        
        public static Touch[] getCurrentTouches() {
            int touches = Input.touchCount;
            Touch[] currentTouches = new Touch[touches];

            if (touches > 0) {
                for (int i = 0; i < touches; i++) {
                    currentTouches[i] = Input.GetTouch(i);
                }
            }
            return currentTouches;
        }


        public static float distance(Vector3 p1, Vector3 p2){
            //Formula Help: https://www.varsitytutors.com/hotmath/hotmath_help/topics/distance-formula-in-3d
            double dist = Math.Sqrt( Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2) + Math.Pow(p2.z - p1.z, 2) );
            return (float)dist;
        }

        public static float ScreenScale {
            get{
                float res = Screen.width * Screen.height;
                float baseRes = 1280f * 720f; //float baseRes = 1920f * 1080f;
                float screenScale = res / baseRes;

                //float screenScale =  baseRes / res; //Testing which way is better! This line allows the user to multiply instead of divide by the scale.

                //Debug.Log("Res Scale: " + screenScale);
                //Debug.Log("Res Diff: " + (res / baseRes) );

                return screenScale;
            }
        }

        /// <summary>
        /// This vector is the specific X scale and Y scale based on the base resolution of 1280x720.
        /// Use this if you need a perfect scale for an measurement. Like if you need to scale a drag control.
        /// If you are only scaling UI it is probably easier to use "ScreenScale".
        /// </summary>
        public static Vector2 ScreenScaleVector {
            get{
                Vector2 screenScale = new Vector2(Screen.width / 1280f, Screen.height / 720f);
                return screenScale;
            }
        }

    }
}