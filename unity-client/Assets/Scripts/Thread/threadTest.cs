using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.Threading;
using System.Threading.Tasks;

using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;


public class threadTest : MonoBehaviour {


    // These need to be moved into a class of their own.
	public delegate void loopFunct<T>(T ind); //Delegate schema must be defined before it is used as a variable type.
	// We need to wait for the threads to complete. It is okay to block the main thread once all sub worker threads are started.
	void threadLoop<T>(IList<T> arr, loopFunct<T> funct){ // "arr" can be a list or array. See: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generics-and-arrays
		int threadCount = 4; //Automatically limit this to at or below the number of logical CPU cores. This will avoid "context Switching".
		int len = arr.Count/4; //I will need to check for any left over with mod?
		int startOffset = 0;
		Task[] workerTasks = new Task[threadCount];
		//List<Task> workerTasks = new List<Task>();
		//List<Task<loopFunct<T>>> workerTasks = new List<Task<loopFunct<T>>>();

		for(int t=0; t<threadCount; t++){
			Task worker = Task.Run(()=>{
				int localOffset = startOffset+0; //Cache the offset so it would update with closure.
				for(int i=0; i<len; i++){ //for(int i=startOffset; i<len; i++){
					
					if(i+localOffset < arr.Count){ //This is "if" is for debug, remove later! I think so how the "startOffset" is messing up this index...
						funct(arr[i+localOffset]);
					}
				}
			});
        	//worker.Start();

			workerTasks[t] = worker; //workerTasks.Add(worker);
			startOffset += len; //So, each thread will do new work where the old one left off!
		}

		//Debug.Log("workerTasks: " + workerTasks.Length);
		//Debug.Log("workerTasks[0].Status: " + workerTasks[0].Status);

		// Block the main thread until all worker thread are done.
		Task.WaitAll(workerTasks);

		//Thread.Sleep(1000); //debug remove later!
		
		/*for(int i=0; i<len; i++){
			funct(arr[i]);
		}*/
	}



    // Start is called before the first frame update
    void Start() {

        List<cube> cubes = new List<cube>(){new cube(1,1,1,1,1,1), new cube(2,1,1,1,1,1), new cube(3,1,1,1,1,1), new cube(4,1,1,1,1,1)};

        DebugTimer t1 = new DebugTimer();

		threadLoop<cube>(cubes, (cubeInd)=>{ // Note: It totally works to use anonymous functions in the schema of the delegate. Doing so we don't even need to define the anon functions as that delegate type!
			Debug.Log("foo");
			cube rtnCubeObj = GenCubeGameObject(cubeInd);
			cubes.Add(rtnCubeObj); //Note: "cubes" is passed by closure!
		});
        
        t1.stopTimer("Editor gen time: ");
    }

    // This is a "test work" function.
    cube GenCubeGameObject(cube cube) {
        return new cube(cube.x +1, cube.y +1, cube.z +1, cube.sizeX, cube.sizeY, cube.sizeZ);
    }

    
}












// My conculsion for today seems to be that Unity is not a multithread engine. It likely won't be until they finish changing everyything in the 2020 version...
// The Unity API just won't have any threas expect the main one and will throw expection if called in other threads. (At least in the editor, not sure if runtime does the same...)
// Thus, I should only use any extra thread code (either custom or unity C# Jobs) to compute data and not interact with GameObjects.
// In any I would have to convert/interpret the thread's result data into Unity GameObjects.



/*

You needs these for Unity Jobs:
	using UnityEngine.Jobs;
	using Unity.Collections;
	using Unity.Jobs;



Below is the over head to call a job. See: https://docs.unity3d.com/Manual/JobSystemParallelForJobs.html

	NativeArray<int> resTest = new NativeArray<int>(3, Allocator.TempJob);
	resTest[0] = 0; resTest[1] = 1; resTest[2] = 2;
	
	RenderCubes renderJob = new RenderCubes();
	renderJob.result = resTest;

	// Schedule the job with one Execute per index in the results array and only 1 item per processing batch
	JobHandle handle = renderJob.Schedule(resTest.Length, 1);
	handle.Complete();
	resTest.Dispose();

*/

public struct RenderCubes : IJobParallelFor {

	/*
    [ReadOnly]
    public NativeArray<float> a;    
	[ReadOnly]
    public NativeArray<float> b;
    public NativeArray<float> result;
	*/

	public NativeArray<int> result; //public NativeArray<GameObject> result;

    public void Execute(int i) {
        //result[i] = a[i] + b[i];
		int ind = result[i]; //Copy like they said to. See: https://docs.unity3d.com/Manual/JobSystemTroubleshooting.html
		
		GameObject cubeObj = new GameObject("cubeObj");
		cubeObj.name = "Test:" + ind;
    }
}