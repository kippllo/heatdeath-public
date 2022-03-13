using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelAnimator : MonoBehaviour {

	public VoxelAnimationScriptableObject[] animations;
	VoxelAnimationScriptableObject curAni;
	public int startAnimation = 0; //The animation that will begin on start.
	public bool playOnStart = true;
	bool isPaused = false;


	// Use this for initialization
	void Start () {
		play(startAnimation); //Start playing the first animation...

		//Stop the animation if the user don't want to play on start!
		if (!playOnStart) {
			pause();
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (!isPaused) {
			curAni.playTimer += Time.deltaTime;
			if (curAni.playTimer > curAni.playSpeed) {
				curAni.playTimer -= curAni.playSpeed; //Reset timer...
				curAni.nextFrame(gameObject); //pass the instance of the gameObject this script is attached to.
			}
		}
	}

	public void play(int index, bool restart = false) {

		//If the users is trying to play the same already playing animation, just stay on the current frame of the animation unless they explicitly want to restart the animation.
		if (curAni == animations[index] && !restart) {
			isPaused = false;
			return;
		}

		//Stop old animation
		if (curAni) { //Null check to avoid errors when called in a GameObject's start function. This line is the same as: "if(curAni != null)"
			curAni.pause(); //pause old animation. This will destroy its current frame.
		}

		//Start new animation
		curAni = animations[index];
		curAni.reset(); //Reset to start at the beginning of the animation.

		//Turn pause off.
		isPaused = false;
	}

	//Plays from the current animation's current frame.
	public void playFromCurrent(int index = -1) {
		if (index != -1) { //If the user wants to play a different animation than the one already set.
			if (curAni) {
				curAni.pause();
			}
			curAni = animations[index];
		}

		//Turn pause off, let the update function do the rest!
		isPaused = false;
	}

	//Pause the animation without destroying the current frame...
	public void pause() {
		//curAni.pause(); //Uncomment to destroy the current animation frame when paused...
		isPaused = true;
	}


}
