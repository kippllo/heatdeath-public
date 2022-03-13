using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Keybored.FPS {

    /// <summary>
    /// This Library track FPS stats and stores them in public static properties for any other script to access.
    /// I must be initialized in a scene by calling "FPSLib.FPSInit()".
    /// </summary>
    public static class FPSLib {

        /// <summary>
        /// This Library track FPS stats and stores them in public static properties for any other script to access.
        /// </summary>
        public static GameObject FPSInit(){
            //Spawn an object with an "Update" method that will track FPS.
            GameObject FPSTracker = new GameObject("FPS Tracker");
            FPSTracker.AddComponent<FPSTracker>();
            return FPSTracker;
        }

        
        //--------------------------
        //--------------------------Getters for reading stats. Readonly--------------------------
        //--------------------------
        
        
        private static int _FPS;
        private static int _AvgFPS;
        private static int _AvgFPSFrameCount;
        
        /// <value>
        /// The realtime FPS based on the current single-frame data.
        /// </value>
        public static int FPS {
            get {return _FPS;}
        }

        /// <value>
        /// The average FPS based on the last few frames' data.
        /// </value>
        public static int AvgFPS {
            get {return _AvgFPS;}
        }

        /// <value>
        /// The average FPS based on Time.frameCount.
        /// </value>
        public static int AvgFPSFrameCount {
            get {return _AvgFPSFrameCount;}
        }



        //--------------------------
        //--------------------------Methods for setting stats. --------------------------
        //             It'd be nice if C# had internal access level for namespace, but it does not.
        //             So, these are public...
        //--------------------------
        public static void SetFPS(int curFPS){
            _FPS = curFPS;
        }

        public static void SetAvgFPS(int curAvgFPS){
            _AvgFPS = curAvgFPS;
        }

        public static void SetAvgFPSFrameCount(int curAvgFPSFrameCount){
            _AvgFPSFrameCount = curAvgFPSFrameCount;
        }
        


    }
}
