using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;


public class TitleCtrl : MonoBehaviour {

	//[Tooltip("A GameObject to Center in the screen.")]
	//public GameObject CenterModel;
	//public Vector3 centerOffset;

	public newsTicker serverMessageTicker;
	public TMP_Text clientOutdatedUI;

	public TMP_Text pressStartText;
	public TMP_Text copyrightText;

	public TMP_Text onlineCountUI;
	float onlineCountTimer = 0.0f;
	float onlineCountTimerMax = 5.0f;

	public lowResCam lowResCamScrpt;
	public int aniRes = 250; //This is controlled with a Unity animation outside of this script!


	//public Canvas page1;
	//public Canvas page2;
	public Canvas[] pages;
	private int curPageInd;

	private Vector2 screenRes;

	public async Task<bool> isClientOutdated() {
		if(clientOutdatedUI.text != ""){ //If the client version has already been check, return true!
			return true;
		}

		//Else check the server version and try again.
		
		fetchServerMessage();
		await Task.Delay(1000); //Wait a sec for the data to download from the server.
		return clientOutdatedUI.text != ""; //This text will be an empty string if the client is up-to-date.
	}

	
	async void Start () {
		ClientSettings.ReadSettings(); //Read settings so this whole class is up-to-date.

		//Set custom quitting code.
		Application.quitting += AppQuitToDo;

		screenRes =  new Vector2(Screen.width, Screen.height);

		//Debug.Log("\nHERE END\n"); //Debug
		fixQualitySettings();
		//Debug.Log("\nHERE fixQualitySettings\n"); //Debug

		serverCtrl.reset(); //Every time the menu loads, the serverCtrl class needs to be reset.
		////SmartLerp.reset(); //I don't think I'm using smart lerp! comment out this line later!  //After the server connection settings are reset, reset the server ping.

		//Debug.Log("\nHERE serverCtrl.reset\n"); //Debug

		onlineCountTimer = onlineCountTimerMax; //This is so the online count will update in the first frame.
		fetchServerMessage();

		//Debug.Log("\nHERE fetchServerMessage\n"); //Debug

		goToPage(0); // Make sure the first page is showing!

		await Task.Delay(10); //Wait for the lowResCam start function to finish.
		int greatestDimension = (Screen.width >= Screen.height) ? Screen.width : Screen.height; // Base one big square on the largest dimension so that it will be a perfect square, not skewed, and cover the whole screen.
		lowResCamScrpt.pixelPerfectScreen(aniRes,aniRes, greatestDimension, greatestDimension);


		//Mobile UI Start---
		#if UNITY_ANDROID || UNITY_IOS
			float scaledCanvasWidth = copyrightText.canvas.pixelRect.width/copyrightText.canvas.scaleFactor;
			float xOffset = scaledCanvasWidth * 0.02f; //To be safe move the copyright text 2% of the screen to the left. This is to avoid rounds edges like an iPhoneX would have.
			Vector2 offsetFix = new Vector2(copyrightText.rectTransform.localPosition.x -xOffset, copyrightText.rectTransform.localPosition.y); //Move the copyright text 2% to the left. Note: "-xOffset" needs to be subtacted from the current position so it will just move the UI that amount to left of the its starting origin. Rememeber, the UI's localPosition is messed up since it is not centered on the canvas.
			copyrightText.rectTransform.localPosition = offsetFix;
		#endif

		//Debug.Log("\nHERE Start Done\n"); //Debug
	}
	

	void Update () {

		//Grab the online count
		onlineCountTimer += Time.deltaTime;
		if(onlineCountTimer >= onlineCountTimerMax){
			onlineCountTimer -= onlineCountTimerMax;
			updateOnlineCount();
		}


		if(Input.GetKeyDown("escape")){
			Application.Quit();
		}


		if(Input.anyKeyDown && curPageInd == 0){ //Change to the second page only if we're on the first page.
			//Load menu
			//SceneManager.LoadScene(5);
			goToPage(1);
		}

		//Set start text base on if there is a controller plugged in or not...
		pressStartText.text = (ControllerLib.controllerConnected) ? "Press Any Button" : "Press Any Key";

		//Reset the lowResCam based on the changing animation of "aniRes".
		int greatestDimension = (Screen.width >= Screen.height) ? Screen.width : Screen.height;
		lowResCamScrpt.pixelPerfectScreen(aniRes,aniRes, greatestDimension, greatestDimension);


		//The window has changed sizes, reset the news ticker to the dimensions.
		if(screenRes.x != Screen.width || screenRes.y != Screen.height) {
			serverMessageTicker.reset();

			//Save the new resolution.
			screenRes =  new Vector2(Screen.width, Screen.height);
		}


		//Mobile UI Update---
		#if UNITY_ANDROID || UNITY_IOS
			pressStartText.text = "Tap to Start";
			pressStartText.text = (ControllerLib.controllerConnected) ? "Press Any Button" : "Tap to Start";
		#endif
	}



	public async void updateOnlineCount() {
		int count = await serverCtrl.getOnlineCount();
		onlineCountUI.text = "Online: " + count;
	}


	public async void fetchServerMessage() {
		//dynamic serverInfo = await serverCtrl.GetServerInfo();
		ServerInfo serverInfo = await serverCtrl.GetServerInfo();

		//serverInfo

		//Set the server message to the news ticker.
		serverMessageTicker.textUI.text = (serverInfo.maintenanceMessage != "") ? serverInfo.maintenanceMessage + "\n\n" : "";
		
		//serverInfo.Get
		//serverMessageTicker.textUI.text = (serverInfo.Property("maintenanceMessage") != "") ? serverInfo.Property("maintenanceMessage") + "\n\n" : "";
		serverMessageTicker.reset();

		//Set the "Client out of date message"
		//clientOutdatedUI.text = (serverInfo.Property("clientVer") != "-1.0" && serverInfo.Property("clientVer") != Application.version) ? "Client is out of date! Please download the update from itch.io \nYour Ver: " + Application.version + "\nCurrent Ver: " + serverInfo.Property("clientVer") : "";
		clientOutdatedUI.text = (serverInfo.clientVer != "-1.0" && serverInfo.clientVer != Application.version) ? "Client is out of date! Please download the update from itch.io \nYour Ver: " + Application.version + "\nCurrent Ver: " + serverInfo.clientVer : "";
	}


	public void fixQualitySettings(){
		//Limit FPS to 60.    Help: https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
		// Application: https://docs.unity3d.com/ScriptReference/Application.html
		// QualitySettings: https://docs.unity3d.com/ScriptReference/QualitySettings.html
		// Could send microphone data for a voice chat! Help: https://docs.unity3d.com/ScriptReference/Microphone.html
		

		//ADD a check to see if the setting are already correct!
		QualitySettings.SetQualityLevel(0, true); //Set the quality to the lowest level...

		//Debug.Log("\nHERE 2\n"); //Debug
		
		Application.targetFrameRate = ClientSettings.fpsCap;

		QualitySettings.antiAliasing = 4; //2, 4, or 8

		//Debug.Log("\nHERE 3\n"); //Debug

		Screen.fullScreen = ClientSettings.fullScreen; //Set the game to run in a window, not full screen.
		//PlayerSettings.resizableWindow = true; //Turn on resizable windows.

		//Debug.Log("\nHERE 3.1\n"); //Debug

		#if !UNITY_ANDROID && !UNITY_IOS //Note: "Screen.resolutions" is always empty on android.
		if(ClientSettings.fullScreen){ //If the player wants the window in fullscreen.
			Resolution[] resolutions = Screen.resolutions; //First max out the window's resolution.
			//Debug.Log("\nHERE 3.2\n"); //Debug
			Resolution maxRes = resolutions[resolutions.Length-1]; //The last option is the biggest
			//Debug.Log("\nHERE 3.3\n"); //Debug
			Screen.SetResolution(maxRes.width, maxRes.height, ClientSettings.fullScreen);
			
			//Debug.Log("\nHERE 3.4\n"); //Debug
			//Debug.Log(Screen.fullScreen);
		}
		#endif

		//Debug.Log("\nHERE 3.5\n"); //Debug


		QualitySettings.vSyncCount = (ClientSettings.vSync) ? 1 : 0; //Be sure to set the vSync after "Screen.SetResolution" because that may turn the vsync on by default...
		
		//Debug.Log("\nHERE 3.6\n"); //Debug

		/*for(int i =0; i<resolutions.Length; i++){
			Debug.Log(resolutions[i].ToString());
		}
		*/
	}

	public static void AppQuitToDo(){
		//Called before the application quits!
		serverCtrl.CloseUDP();
	}

	public void goToPage(int pageInd){
		if(pageInd < 0 || pageInd >= pages.Length){ return; } //Don't move to an index that is out of range!

		curPageInd = pageInd; //Keep track of the current page!

		//Set all page to inactive.
		for(int i=0; i<pages.Length; i++){
			pages[i].gameObject.SetActive(false);
		}

		//Set the correct page to active.
		pages[pageInd].gameObject.SetActive(true);
	}



}
