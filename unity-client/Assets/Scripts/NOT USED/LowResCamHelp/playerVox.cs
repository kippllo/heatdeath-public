using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerVox : MonoBehaviour {

	//You don't have to use multiple animators, but you can...
	public VoxelAnimator bodyTopAni;
	public VoxelAnimator bodyBotAni;
	public VoxelAnimator whipAni;

	public int idle;
	public int walk; //The index of the walk animation...

	// Use this for initialization
	void Start () {
		//bodyTopAni = GetComponent<VoxelAnimator>();
		////voxAni.play(0); //Play the first animation. "VoxelAnimator" will do this automaticall even if I don't call this.
		//voxAni.pause();
	}
	
	// Update is called once per frame
	void Update () {

		/*
		if (Input.GetKeyDown ("d")) {
			bodyBotAni.play(walk);
			transform.rotation = Quaternion.Euler(0, 0, 0);
		}
			

		if (Input.GetKey ("d")) {
			bodyBotAni.playFromCurrent();
			//bodyBotAni.playFromCurrent(walk);
		}

		if (Input.GetKeyUp("d")) {
			bodyBotAni.play(idle);
		}

		if (Input.GetKeyDown ("a")) {
			bodyBotAni.play(walk);
			transform.rotation = Quaternion.Euler(0, 180, 0);
		}if (Input.GetKey ("a")) {
			bodyBotAni.playFromCurrent();
		}if (Input.GetKeyUp("a")) {
			bodyBotAni.play(idle);
		}
		*/

		//If you put the idle animation check above the over in the update, the animations won't flicker off when releasing on key while holding another.
		if (Input.GetKeyUp("d") || Input.GetKeyUp("a")) {
			bodyBotAni.play(idle);
		}

		if (Input.GetKey ("d")) {
			bodyBotAni.play(walk);
			transform.rotation = Quaternion.Euler(0, 0, 0);
		}
		if (Input.GetKey ("a")) {
			bodyBotAni.play(walk);
			transform.rotation = Quaternion.Euler(0, 180, 0);
		}



		
	}
}
