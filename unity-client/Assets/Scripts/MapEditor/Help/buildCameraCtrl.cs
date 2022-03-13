using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildCameraCtrl : MonoBehaviour {


	public float FlySpeed = 0.2f;
	public float DefaultFlySpeed = 0.0f;
	public float RotateSpeed = 1.25f;

	public bool FullyFree = true;


	// Use this for initialization
	void Start () {
		DefaultFlySpeed = FlySpeed;

		Cursor.lockState = CursorLockMode.Locked; //Locks mouse to the center of the window. //Could also use "CursorLockMode.Confined" to keep the mouse in our window, but allow movement.
		//Cursor.visible = false; //hides mouse
	}
	
	// Update is called once per frame
	void Update () {

		float moveRotHor = Input.GetAxis("Mouse X");
		float moveRotVert = Input.GetAxis("Mouse Y");


		if(Input.GetKeyDown("escape")) {
			//The nice thing aboutt Cursor.lockState is that other scripts can read it too. Meaning it is basically doing the same thing as a static variable.
			Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.Confined : CursorLockMode.Locked;
		}

		if(Input.GetKeyDown("f"))
		{
			if(FullyFree == false)
			{
				FullyFree = true;
			}

			else if(FullyFree == true)
			{
				FullyFree = false;
			}
		}

		if(Input.GetKeyDown("g"))
		{
			if(FlySpeed == DefaultFlySpeed)
			{
				FlySpeed = DefaultFlySpeed/4;
			}

			else
			{
				FlySpeed = DefaultFlySpeed;
			}
		}


		transform.RotateAround(transform.position, Vector3.up, moveRotHor * RotateSpeed);

		if(FullyFree == false)
		{
			if((transform.eulerAngles.x >= 0 && transform.eulerAngles.x <= 11) || (transform.eulerAngles.x >= 340 && transform.eulerAngles.x <= 360) )
			{
				transform.Rotate(-moveRotVert * RotateSpeed, 0, 0);
			}
			else if((transform.eulerAngles.x >= 11 && transform.eulerAngles.x <= 200) && -moveRotVert < 0)
			{
				transform.Rotate(-moveRotVert * RotateSpeed, 0, 0);
			}

			else if((transform.eulerAngles.x <= 340 && transform.eulerAngles.x >= 70) && -moveRotVert > 0)
			{
				transform.Rotate(-moveRotVert * RotateSpeed, 0, 0);
			}

		}


		if(FullyFree == true)
		{
			transform.Rotate(-moveRotVert * RotateSpeed, 0, 0);
		}

		if(Input.GetKey("w"))
		{
			gameObject.transform.Translate(0, 0, FlySpeed);
		}

		if(Input.GetKey("s"))
		{
			gameObject.transform.Translate(0, 0, -FlySpeed);
		}

		if(Input.GetKey("d"))
		{
			gameObject.transform.Translate(FlySpeed, 0, 0);
		}

		if(Input.GetKey("a"))
		{
			gameObject.transform.Translate(-FlySpeed, 0, 0);
		}

		if(Input.GetKey("space"))
		{
			gameObject.transform.Translate(0, FlySpeed, 0);
		}

		if(Input.GetKey("left shift"))
		{
			gameObject.transform.Translate(0, -FlySpeed, 0);
		}


	}
}
