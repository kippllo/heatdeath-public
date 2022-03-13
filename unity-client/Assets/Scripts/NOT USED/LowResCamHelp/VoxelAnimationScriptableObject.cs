using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//START HERE: The last frame stays too long, it stays for 2 regualr frames...

[CreateAssetMenu(fileName = "New Voxel Animation", menuName = "Voxel Animation")]
public class VoxelAnimationScriptableObject : ScriptableObject {

	public GameObject[] frames;
	private GameObject frame;
	public bool loop = true; //Should the animation loop...


	public int currentFrameIndVar = -1;
	public int currentFrameInd {
		get {return currentFrameIndVar;}
		set {currentFrameIndVar = value;}
	}

	public float playTimerVar = 0.0f; //Might only need the speed, not the timer...
	public float playTimer {
		get {return playTimerVar;}
		set {playTimerVar = value;}
	}

	public float playSpeedVar = 1.0f;
	public float playSpeed {
		get {return playSpeedVar;}
		set {playSpeedVar = value;}
	}

	//Check if the animation has ended. Will be helpful for some fight games!
	public bool hasEnded {
		get { return (currentFrameInd >= frames.Length); }
	}

	public void nextFrame(GameObject parent) {
		int nextFrame = currentFrameInd + 1;

		if(nextFrame >= frames.Length && loop) {
			nextFrame = 0; //currentFrameInd = -1; //Start on "-1" so "nextFrame" can be called to start on the first frame...
			//Destroy (currentFrame); // Destroying the currentFrame here will fix the problem of the last frame staying in the scene. It'll also set the currentFrame to null just like at the start of the script.
			//return;
		}

		//Only change the frame in the range of our array. This will also freeze the last frame on a "nonLooping" animation.
		if(nextFrame < frames.Length){
			//Only apply the new frame index now that the checks are done!
			currentFrameInd = nextFrame;

			//Destory the current frame a spawn the next one...
			if (frame != null) {
				Destroy(frame);
			}
			frame = Instantiate(frames[nextFrame], parent.transform.position, parent.transform.rotation);
			frame.transform.parent = parent.transform; // Set the parent to the parent of this object.
		}
	}



	public void reset(){
		//Reset variables to defaults.

		if (frame) { //Null check.
			Destroy(frame); //Clear any current frame so we can start at the beginning.
		}

		currentFrameInd = -1; //Start on "-1" so "nextFrame" can be called to start on the first frame...
		playTimer = playSpeed; //This will allow the first frame to appear immediately, without it there would be one blank frame before the first frame of the animation appeared. Setting "playTimer = 0" would make the animation wait one timer cycle before displaying any frames.
	}

	/*
	public void reset(){
		//Reset varables to defaults.
		frame = null; // "stop" must always be called before reset or the current "frame" might be left on screen when this animation ends. Maybe move the "Destroy(frame);" up here later...
		currentFrameInd = -1; //Start on "-1" so "nextFrame" can be called to start on the first frame...
		playTimer = playSpeed; //This will allow the first frame to appear immediately, without it there would be one blank frame before the first frame of the animation appeared. Setting "playTimer = 0" would make the animation wait one timer cycle before displaying any frames.
	}

	public void stop(){
		//Destory the current frame and reset varables.
		if (frame != null) {
			Destroy(frame);
		}
		reset();
	}
	*/

	public void pause(){
		//Destory the current frame and don't reset varables.
		if (frame) { //Null check to avoid errors if "frame == null".
			Destroy(frame); //Maybe don't destroy current frame...
		}
	}

}
