using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Keybored.Mobile;

//PS4 Controller help: https://www.reddit.com/r/Unity3D/comments/1syswe/ps4_controller_map_for_unity/

public class charCtrl : MonoBehaviour {

	public int objID;
	public float hp = 100; //10-3-19: Changed from int to float...
	private int hpMax = 100; //Used only to set the enemy hp ui.

	public float speed = 5.0f;
	Vector3 backupRot;
	Vector3 lastMouseRot;

	public float fireTimer = 0;
	public float fireTimerMax = 0.25f;

	public Transform spawnPoint;
	public GameObject bullet;

	public Text usernameUI;
	public Slider hpUI;

	CharacterController ctrl;
	Camera cam;

	public ParticleSystem partsysFire;
	public ParticleSystem partsysDamage;
	private bool partsysFireEmitNetTracker;
	private bool partsysDamageEmitNetTracker;	
	private bool flgSoundDeathNetTracker;
	
	private string rightJoyStickYAxis = "aim_YAxis"; //This is a quick work around for mac controller support. Later make a custom inputManager! Unity doesn't allow me to edit their Input Manager in C# code...

	
	AudioSource aud;
	//public AudioClip soundShoot; // Note: I'm using "public" instead of "[SerializeField]" so that unity will not throw the "warning CS0649: Field 'charCtrl.soundShoot' is never assigned to, and will always have its default value null" warning.
	//public AudioClip soundDamage;
	//public AudioClip soundDeath;


	//Mobile control stuff...
	TouchJoystick TouchMove;
	TouchJoystick TouchAim;

	void Start () {
		//Get this object's Server ID.
		objID = serverCtrl.getNextObjID();
		gameObject.name = ""+ objID;

		ctrl = GetComponent<CharacterController>();

		//Setup the HP slider based on vars...
		hpUI.maxValue = hpMax;

		//Setup the fireTimer to start correctly.
		fireTimer = fireTimerMax * 0.6f; //Was *0.75f

		//Update username UI
		usernameUI.text = serverCtrl.username;

		cam = Camera.main;

		rightJoyStickYAxis = ""+rightJoyStickYAxis; //I'm putting this here to avoid compile warning for mobile. Really fix later...
		#if !UNITY_STANDALONE_WIN //#if UNITY_STANDALONE_OSX
			rightJoyStickYAxis = "aim_YAxis_Mac";
		#endif

		#if !UNITY_IOS && !UNITY_ANDROID
			//Hide the touch UI...
			GameObject.FindGameObjectWithTag("TouchCtrls").SetActive(false);
		#else
			TouchMove = GameObject.Find("TouchJoystickMove").GetComponent<TouchJoystick>();
			TouchAim = GameObject.Find("TouchJoystickAim").GetComponent<TouchJoystick>();
		#endif
		

		// Audio Source Setup
		aud = GetComponent<AudioSource>();
		aud.loop = false;
		aud.playOnAwake = false;

	}
	

	void Update () {

		//Only move the player if the map is not in full screen mode!
		if(!MiniMapCtrl.mapFull){

			// Move the player
			Vector3 MoveCharacter = new Vector3(0, 0, 0);

			#if !UNITY_IOS && !UNITY_ANDROID
				if (Input.GetAxis("move_YAxis") > 0 || Input.GetButton("Up")) {
					MoveCharacter.y += speed;
				}

				if (Input.GetAxis("move_YAxis") < 0 || Input.GetButton("Down")) {
					MoveCharacter.y -= speed;
				}

				if (Input.GetAxis("move_XAxis") > 0 || Input.GetButton("Right")) {
					MoveCharacter.x += speed;
				}

				if (Input.GetAxis("move_XAxis") < 0 || Input.GetButton("Left")) {
					MoveCharacter.x -= speed;
				}

				if (Input.GetButton("Fire")) {
					fireTimer += Time.deltaTime;
					if (fireTimer >= fireTimerMax) {
						fireTimer -= fireTimerMax;
						Instantiate (bullet, spawnPoint.position, spawnPoint.rotation);
						partsysFire.Emit(5);
						partsysFireEmitNetTracker = true;
						aud.PlayOneShot(AudioLib.soundShoot, AudioLib.effectVolume); //Only use "PlayOneShot()" and not "Play()" so I can play multiple sounds from one AudioSource at the same time.
					}
				}
				
				if (Input.GetButtonUp ("Fire")) {
					fireTimer = fireTimerMax * 0.6f; //This way when a player starts to fire the same amount of delay is always present. Note that the "fireTimer" is reset and not "+=" (plus-equaled). That way spamming doesn't make it shoot faster!
				}

				transform.position = new Vector3 (transform.position.x, transform.position.y, 0); //Make sure the Z is fixed at 0!

				ctrl.Move(MoveCharacter * Time.deltaTime);


				//Rotation Control------
				Vector3 aimRot = new Vector3( 0, 0, axisToRot(rightJoyStickYAxis, "aim_XAxis") );
				Vector3 moveRot = new Vector3( 0, 0, axisToRot("move_YAxis", "move_XAxis") );
				Vector3 mouseRot = new Vector3( 0, 0, mouseToRot() );
				float lerpSpeed = 25.0f; //I want the lerp to be faster for the mouse controls.

				//If a controller is plugged in, use the controller to aim. If not use the mouse to aim.
				if(ControllerLib.controllerConnected){
					if(Input.GetAxis("aim_XAxis") != 0 || Input.GetAxis(rightJoyStickYAxis) != 0){
						backupRot = aimRot;
					} else if (Input.GetAxis("move_XAxis") != 0 || Input.GetAxis("move_YAxis") != 0) { //Only update the "backupRot" if move joystick is not zeroed.
						backupRot = moveRot;
					}
				} else {
					backupRot = mouseRot;
					lerpSpeed = lerpSpeed *0.8f; //Make the mouse lerp 20% slow than the PS4 controller.
				}

				if (Input.GetAxis("aim_XAxis") == 0 && Input.GetAxis(rightJoyStickYAxis) == 0) { //If the right joystick is zeroed, set the player's rotation to the last known "backupRot" which is set by the left joystick or mouse position.
					aimRot = backupRot;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(aimRot), lerpSpeed * Time.deltaTime);
			#else
				//Mobile controls---
				/*if (TouchMove.YAxis > 0) {
					MoveCharacter.y += speed;
				}

				if (TouchMove.YAxis < 0) {
					MoveCharacter.y -= speed;
				}

				if (TouchMove.XAxis > 0) {
					MoveCharacter.x += speed;
				}

				if (TouchMove.XAxis < 0) {
					MoveCharacter.x -= speed;
				}*/
				//MoveCharacter.y += speed * TouchMove.YAxis;
				//MoveCharacter.x += speed * TouchMove.XAxis;
				
				float touchY = (TouchMove.YAxis > 0.5f) ? 1f : TouchMove.YAxis; //Round up to 1 so the controls will snap to full speed like when a key is pressed.
				touchY = (touchY < -0.5f) ? -1f : touchY; //Don't forget to snap to negative as well.
				
				float touchX = (TouchMove.XAxis > 0.5f) ? 1f : TouchMove.XAxis;
				touchX = (touchX < -0.5f) ? -1f : touchX;

				MoveCharacter.y += speed * touchY;
				MoveCharacter.x += speed * touchX;

				
				//(Mathf.Abs(TouchMove.YAxis) > 0.5f) ? 1f*Mathf.Sign(TouchMove.YAxis) : TouchMove.YAxis;
				

				if (TouchAim.XAxis > 0 || TouchAim.XAxis < 0 || TouchAim.YAxis > 0 || TouchAim.YAxis < 0) {
					fireTimer += Time.deltaTime;
					if (fireTimer >= fireTimerMax) {
						fireTimer -= fireTimerMax;
						Instantiate (bullet, spawnPoint.position, spawnPoint.rotation);
						partsysFire.Emit(5);
						partsysFireEmitNetTracker = true;
						aud.PlayOneShot(AudioLib.soundShoot, AudioLib.effectVolume);
					}
				}
				
				if (TouchAim.XAxis == 0 && TouchAim.YAxis == 0) {
					fireTimer = fireTimerMax * 0.6f; //This way when a player starts to fire the same amount of delay is always present. Note that the "fireTimer" is reset and not "+=" (plus-equaled). That way spamming doesn't make it shoot faster!
				}

				transform.position = new Vector3 (transform.position.x, transform.position.y, 0); //Make sure the Z is fixed at 0!

				ctrl.Move(MoveCharacter * Time.deltaTime);

				//Mobile rotation controls.
				Vector3 aimRot = new Vector3( 0, 0, touchAxisToRot(TouchAim) );
				Vector3 moveRot = new Vector3( 0, 0, touchAxisToRot(TouchMove) );
				float lerpSpeed = 25.0f;
				if(TouchAim.XAxis != 0 || TouchAim.YAxis != 0){
					backupRot = aimRot;
				} else if (TouchMove.XAxis != 0 || TouchMove.YAxis != 0) { //Only update the "backupRot" if move joystick is not zeroed.
					backupRot = moveRot;
				}

				if (TouchAim.XAxis == 0 && TouchAim.YAxis == 0) { //If the right joystick is zeroed, set the player's rotation to the last known "backupRot" which is set by the left joystick or mouse position.
					aimRot = backupRot;
				}

				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(aimRot), lerpSpeed * Time.deltaTime);
			#endif
		}


		////if(inLobby){} //maybe use this later...

		//If the player is outside of the safe zone, take damage
		if( Vector3Thin.distance( Vector3Thin.FromVector3(TempRoundData.dangerCenter), Vector3Thin.FromVector3(transform.position) ) > TempRoundData.dangerRadius && !TempRoundData.inLobby){
			hp -= TempRoundData.compressionZoneDamage * Time.deltaTime;
			
			//Make the player's camera pixelated if he is outside of the saftey zone.
			TempRoundData.playerLowResCam.GetComponent<InGameResize>().camRes = 100;

		} else if(TempRoundData.playerLowResCam.GetComponent<InGameResize>().camRes != TempRoundData.camResMax) {
			//If the player is not in the danger zone and his camera is set to the lower resolution, reset the resolution to max.
			TempRoundData.playerLowResCam.GetComponent<InGameResize>().camRes = TempRoundData.camResMax;
		}


		//Check for KO via HP...
		if(hp < 1) { //Was: "hp <= 0", but I changed it when I changed the hp to a float, so that we will round down to death because the HP is still a float on the backend.
			hp = 0; //For backend compatibility.
			flgSoundDeathNetTracker = true;

			AudioSource.PlayClipAtPoint(AudioLib.soundDeath, gameObject.transform.position, AudioLib.effectVolume); // Calling this because the gameObject is disabled below.
			
			serverCtrl.deleteReconnectFile();
			TempRoundData.playerLowResCam.GetComponent<spectateCtrl>().player = gameObject; //Set the "spectateCtrl" to move this local player.
			TempRoundData.playerLowResCam.GetComponent<spectateCtrl>().enabled = true; //Turn on spectate cotrols.
			
			gameObject.SetActive(false); //Don't destroy the gameObject because we still need it to send network data!
		}

		//Update HP UI
		hpUI.value = hp;
	}


	float axisToRot(string yAxis, string xAxis) {
		//See link for cartesian to polar coordinates convert: https://www.mathsisfun.com/polar-cartesian-coordinates.html
		float cartY = Input.GetAxis(yAxis); //Will be between -1 and 1.
		float cartX = Input.GetAxis(xAxis);
		float polarTheta = Mathf.Atan2 (cartY, cartX) * (180/Mathf.PI); // "Mathf.Atan2" is like "Mathf.Atan(cartY / cartX)", but takes precautions for X being zero and auto transforms for different quadrants. "*(180/Mathf.PI)" converts from radians to degrees.

		return polarTheta;
	}

	float touchAxisToRot(TouchJoystick touchJoystick) {
		//See link for cartesian to polar coordinates convert: https://www.mathsisfun.com/polar-cartesian-coordinates.html
		float cartY = touchJoystick.YAxis; //Will be between -1 and 1.
		float cartX = touchJoystick.XAxis;
		float polarTheta = Mathf.Atan2 (cartY, cartX) * (180/Mathf.PI); // "Mathf.Atan2" is like "Mathf.Atan(cartY / cartX)", but takes precautions for X being zero and auto transforms for different quadrants. "*(180/Mathf.PI)" converts from radians to degrees.

		return polarTheta;
	}

	float mouseToRot() {
		// Note: When using this script with LowResCamera you MUST make the UI camera the "MainCamera". The camera used below must be the same dimensions as the game window!
		//	It is also important that both the UI and RenderCamera have a position of (0,0,0) [Or share the same position whatever it maybe]. If they don't they will be discrepancies in aim and it will look like the mouse position has been translated by the difference in the two cameras positions.
		Vector3 mousePosCam = cam.ScreenToWorldPoint(Input.mousePosition);
		Vector3 camTranslatedToPlayerPos = mousePosCam - transform.position; //You must translate the postion by the player's position so that the rotation is based off of the player and not the camera position!

		float cartY = camTranslatedToPlayerPos.y;
		float cartX = camTranslatedToPlayerPos.x;

		float polarTheta = Mathf.Atan2 (cartY, cartX) * (180/Mathf.PI);
		return polarTheta;
	}

	void OnTriggerEnter(Collider hit){
		if (hit.gameObject.tag == "syncBullet" && !TempRoundData.inLobby) {
			hp -= 1.0f;
			partsysDamage.Emit(3);
			partsysDamageEmitNetTracker = true;
			aud.PlayOneShot(AudioLib.soundDamage, AudioLib.effectVolume);
		}
	}

	public playerObj toSyncData(){
		//Maybe fix the "Mathf.RoundToInt" on player HP to not be there later... the backend will have to be changed...
		playerObj client = new playerObj (objID, Mathf.RoundToInt(hp), transform.position, transform.rotation.eulerAngles, serverCtrl.username, partsysFireEmitNetTracker, partsysDamageEmitNetTracker, flgSoundDeathNetTracker);
		
		//Turn off network particle only after they have been sent.
		partsysFireEmitNetTracker = false; //if(partsysFireEmitNetTracker) partsysFireEmitNetTracker = false; //partsysFireEmitNetTracker = (partsysFireEmitNetTracker) ? false : partsysFireEmitNetTracker;
		partsysDamageEmitNetTracker = false; //if(partsysDamageEmitNetTracker) partsysDamageEmitNetTracker = false; //partsysDamageEmitNetTracker = (partsysDamageEmitNetTracker) ? false : partsysDamageEmitNetTracker;
		flgSoundDeathNetTracker = false; //if(flgSoundDeathNetTracker) flgSoundDeathNetTracker = false;
		
		return client;
	}

	public void setFromReconnData(playerObj reconnPlayer) {
		hp = reconnPlayer.hp;
		transform.position = reconnPlayer.pos.ToVector3();
		backupRot = reconnPlayer.rot.ToVector3();
		serverCtrl.username = reconnPlayer.username;
	}


}
