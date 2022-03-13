using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Keybored.Mobile;

public class spectateCtrl : MonoBehaviour {


	public float speed = 5.0f;
	public GameObject returnToMenuUI;
	
	public GameObject player; //This is set by the player control script at death!

	TouchJoystick TouchMove;
	TouchJoystick TouchAim;


	void Start () {
		gameObject.GetComponent<followPlayer>().enabled = false; //Disable the old cam movement controls.
		returnToMenuUI.SetActive(true);

		//In case the player died in the danger zone, set the camera back to normal resolution.
		TempRoundData.playerLowResCam.GetComponent<InGameResize>().camRes = TempRoundData.camResMax;

		#if UNITY_IOS || UNITY_ANDROID
			TouchMove = GameObject.Find("TouchJoystickMove").GetComponent<TouchJoystick>();
			TouchAim = GameObject.Find("TouchJoystickAim").GetComponent<TouchJoystick>();
		#endif
	}
	

	void Update () {
		if(!MiniMapCtrl.mapFull){
			// Move the player
			Vector3 MoveCharacter = new Vector3(0, 0, 0);

			#if !UNITY_IOS && !UNITY_ANDROID
				if (Input.GetAxis("move_YAxis") > 0 || Input.GetButton("Up")) {
					MoveCharacter.y += speed;
				} if (Input.GetAxis("move_YAxis") < 0 || Input.GetButton("Down")) {
					MoveCharacter.y -= speed;
				} if (Input.GetAxis("move_XAxis") > 0 || Input.GetButton("Right")) {
					MoveCharacter.x += speed;
				} if (Input.GetAxis("move_XAxis") < 0 || Input.GetButton("Left")) {
					MoveCharacter.x -= speed;
				}
				transform.position = new Vector3 (transform.position.x, transform.position.y, 0); //Make sure the Z is fixed at 0!
				transform.Translate(MoveCharacter * Time.deltaTime);
				player.transform.position = transform.position; //This will allow the server to render network object in the view of the spectating player.
			#else
				float touchY = (TouchMove.YAxis > 0.5f) ? 1f : TouchMove.YAxis;
				touchY = (touchY < -0.5f) ? -1f : touchY;
				float touchX = (TouchMove.XAxis > 0.5f) ? 1f : TouchMove.XAxis;
				touchX = (touchX < -0.5f) ? -1f : touchX;
				MoveCharacter.y += speed * touchY;
				MoveCharacter.x += speed * touchX;
				transform.position = new Vector3 (transform.position.x, transform.position.y, 0);
				transform.Translate(MoveCharacter * Time.deltaTime);
				player.transform.position = transform.position;
			#endif
		}
		

	}



	public void backToMenu(){
		SceneManager.LoadScene(0);
	}


}
