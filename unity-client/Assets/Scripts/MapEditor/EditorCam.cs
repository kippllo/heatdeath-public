using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCam : MonoBehaviour {

	//Cam vars
	public float FlySpeed = 0.1f;
	//public float DefaultFlySpeed = 0.0f;
	float RotateSpeed = 1.2f;
	public GameObject moveCam;
	

	void Start(){
		//Will need to undo these changes on scene exit!
		//DefaultFlySpeed = FlySpeed;
		Cursor.lockState = CursorLockMode.Locked; //Locks mouse to the center of the window. //Could also use "CursorLockMode.Confined" to keep the mouse in our window, but allow movement.
		//The mouse auto-hides...
		Cursor.visible = false; //hides mouse
	}

	void Update(){

		float moveRotHor = Input.GetAxis("Mouse X");
		float moveRotVert = Input.GetAxis("Mouse Y");
		

		//if(Input.GetKeyDown("escape")) {
			//Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.Confined : CursorLockMode.Locked;
			Cursor.lockState = (EditorCtrl.blnBuildMode) ? CursorLockMode.Locked : CursorLockMode.None; //CursorLockMode.Confined;
			//EditorCtrl.blnBuildMode = !EditorCtrl.blnBuildMode;
			Cursor.visible = !EditorCtrl.blnBuildMode;
		//}


		if(EditorCtrl.blnBuildMode){
			moveCam.transform.RotateAround(moveCam.transform.position, Vector3.up, moveRotHor * RotateSpeed);
			//Add math.clamp...
			moveCam.transform.Rotate(-moveRotVert * RotateSpeed, 0, 0);
		
			if(Input.GetKey("w"))
			{
				moveCam.transform.Translate(0, 0, FlySpeed);
			}

			if(Input.GetKey("s"))
			{
				moveCam.transform.Translate(0, 0, -FlySpeed);
			}

			if(Input.GetKey("d"))
			{
				moveCam.transform.Translate(FlySpeed, 0, 0);
			}

			if(Input.GetKey("a"))
			{
				moveCam.transform.Translate(-FlySpeed, 0, 0);
			}

			if(Input.GetKey("space"))
			{
				moveCam.transform.Translate(0, FlySpeed, 0);
			}

			if(Input.GetKey("left shift"))
			{
				moveCam.transform.Translate(0, -FlySpeed, 0);
			}

		}
	}
}
