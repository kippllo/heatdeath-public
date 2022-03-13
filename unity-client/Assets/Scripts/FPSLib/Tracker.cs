using System.Collections.Generic;
using UnityEngine;


namespace Keybored.FPS {
    //Can be used to track FPS or PPS...
    public class Tracker {
        
        float lastTime;
        int lastCount;
        
        public float trackerValue; //either FPS or PPS...
        int avgAmount = 100; //The limit in the avg calc. ADD TO Constructor!!
        
        List<float> avgArr = new List<float>();
        public int avgTrackerValue {
            get{
                int sum = 0;
                int count = avgArr.Count;

                if (count == 0) {return 0;} //Avoid dividing by zero...

                for(int i=0; i<count; i++){
                    sum += Mathf.RoundToInt(avgArr[i]);
                }

                return sum/count;
            }
        }
        
        public Tracker(){
            lastTime = Time.realtimeSinceStartup;
        }

        public Tracker(int avgAmountIN){
            lastTime = Time.realtimeSinceStartup;
            avgAmount = avgAmountIN;
        }

        public void UpdateTracker( int valueCountUpdate ){
            float countDif = valueCountUpdate - lastCount;
            float timeDt = Time.realtimeSinceStartup - lastTime;

            trackerValue = countDif/timeDt;

            avgArr.Add(trackerValue);
            if(avgArr.Count >= avgAmount){
                int halfCount = avgAmount/2 -1; //Minus 1 because zero-based index.
                avgArr.RemoveRange(0, halfCount); //Remove the first half of the indices to keep the RAM usage down...
            }


            lastCount = valueCountUpdate;
            lastTime = Time.realtimeSinceStartup;
        }

        //Added just for PPS trackers...
        //Only call this on the tracker that is tracking PPS-Download speeds!
        public bool serverTimeout {
            get{
                //This function returns true if the server went down in the middle of a game!
                float packDt = Time.realtimeSinceStartup - lastTime;
                bool blnTimeout = packDt >= 5.0f; //Timeout after 5 seconds.
                return blnTimeout;
            }
        }

    }
}