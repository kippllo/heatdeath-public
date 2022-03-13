using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Keybored.Mobile;
using Keybored.FPS;

public class FPSCounter : MonoBehaviour {

	public Text fpsUI;
	TouchTap TouchFPS;

	Tracker PPSUp;
	Tracker PPSDownload;

	void Start() {
		MobileStart();

		FPSLib.FPSInit();

		PPSUp = new Tracker(100);
		PPSDownload = new Tracker(100);
	}


	void MobileStart() {
		#if UNITY_IOS || UNITY_ANDROID
			TouchFPS = GameObject.Find("TouchFPS").GetComponent<TouchTap>();
			Vector2 touchZone = new Vector2(0, Screen.height-100); //Bottom left hand corner...
			TouchFPS.startPos = touchZone;
			TouchFPS.width = 250;
			TouchFPS.height = 250;
		#endif
	}


	void Update () {
		fpsUI.text = "Game ID, Network ID: "+ serverCtrl.gameID + ", " + serverCtrl.syncID;
		fpsUI.text += "\nFPS: " + FPSLib.AvgFPS;
		fpsUI.text += "\nPPS Up: " + PPSUp.avgTrackerValue;
		fpsUI.text += "\nPPS Down: " + PPSDownload.avgTrackerValue;
		
		PPSUp.UpdateTracker(serverCtrl.packetCountUp);
		PPSDownload.UpdateTracker(serverCtrl.packetCountDown);


		//Exit round and return to main menu.
		//removed for now, the player can only exit a round by dying... This is because of the minimap...
		if(Input.GetButtonDown("Quit")){
			SceneManager.LoadScene(0);
		}


		if(Input.GetKeyDown("f3")){
			//Toggle the FPS UI
			fpsUI.gameObject.SetActive(!fpsUI.gameObject.activeInHierarchy);
		}

		#if UNITY_IOS || UNITY_ANDROID
			if(TouchFPS.GetTapDown){
				fpsUI.gameObject.SetActive(!fpsUI.gameObject.activeInHierarchy);
			}
		#endif


		if(PPSDownload.serverTimeout){
			serverCtrl.disconnect("Disconnected From Server... \nServer timed out.");
		}
	}
	
}
