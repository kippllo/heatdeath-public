using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Keybored.FPS {

    /// <summary>
    /// This class is a "MonoBehaviour" which uses its "update" method to track FPS stats.
    /// It then updates the static proprties inside of the "FPSLib".
    /// A GameObject with this script can be instantiated in any scene by calling "FPSLib.FPSInit()".
    /// </summary>
    public class FPSTracker : MonoBehaviour {

        int avgFrameAmount = 200; //The number of frame that will be counted by the avg function.

        List<int> avgFPSArr = new List<int>();
        public int avgFPS {
            get{
                int sum = 0;
                int count = avgFPSArr.Count;
                for(int i=0; i<count; i++){
                    sum += avgFPSArr[i];
                }

                return sum/count;
            }
        }

        Tracker FPSTrackerFrameCount;



        void Start() {
            FPSTrackerFrameCount = new Tracker(avgFrameAmount);
        }


        void Update() {
            //Set the realtime FPS
            FPSLib.SetFPS( Mathf.RoundToInt(1 / Time.unscaledDeltaTime) );

            //Update the AvgFPS
            avgFPSArr.Add(FPSLib.FPS);
            if(avgFPSArr.Count >= avgFrameAmount){ //if(avgFPSArr.Count >= 100){
                int halfCount = avgFrameAmount/2 -1; //Minus 1 because zero-based index.
                avgFPSArr.RemoveRange(0, halfCount); //Remove the first half of the indices to keep the RAM usage down...
            }

            FPSLib.SetAvgFPS(avgFPS);

            //Update the FrameCount Method...
            FPSTrackerFrameCount.UpdateTracker(Time.frameCount);
            FPSLib.SetAvgFPSFrameCount( FPSTrackerFrameCount.avgTrackerValue );
        }
    }
}