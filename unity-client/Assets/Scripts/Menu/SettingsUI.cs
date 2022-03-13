using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class SettingsUI : MonoBehaviour {

	//Page 0
	public TMP_InputField ipUI;
	public TMP_InputField portUI;
	public ButtonSetter fpsUI;
	public ButtonSetter vsyncUI;
	public ButtonSetter fullscreenUI;
	public GameObject MobileSettingsButton; //This is used to hide the mobile settings if you're playing the PC version of the game...
	
	//Page 1
	public Slider uiScaleUI;
	public TextMeshProUGUI uiScaleTextUI;

	//Page 2
	public Slider masterVolumeUI;
	public TextMeshProUGUI masterVolumeTextUI;
	public Slider effectVolumeUI;
	public TextMeshProUGUI effectVolumeTextUI;
	public Slider musicVolumeUI;
	public TextMeshProUGUI musicVolumeTextUI;


	public TitleCtrl titleCtrl;
	public GameObject[] pages; //The parent objects that hold the settings controls for each page...
	private int curPageInd;


	void Start () {
		//Read settings from the settings JSON.
		ClientSettings.ReadSettings(); //Read settings so this whole class is up-to-date.

		//This will add new properties that are not currently save in the old version of the JSON file!
		////ClientSettings.ReadSettings();
		////ClientSettings.test = "123WORKS";
		////ClientSettings.SaveSettings();

		resetUIValues();

		//Go to the first page
		goToPage(0);


		//Hide mobile settings if needed.
		#if !UNITY_ANDROID && !UNITY_IOS
			MobileSettingsButton.SetActive(false);
		#endif
	}
	

	void Update () {
		//Only update text that needs it every frame in this function!

		//Page 0
			//None
		
		//Page 1
		uiScaleTextUI.text = "UI Scale: " + (float)Math.Round(uiScaleUI.value, 2);

		//Page 2
		masterVolumeTextUI.text = "Master Volume: " + (float)Math.Round(masterVolumeUI.value, 2);
		effectVolumeTextUI.text = "SFX Volume:    " + (float)Math.Round(effectVolumeUI.value, 2);
		musicVolumeTextUI.text =  "Music Volume:  " + (float)Math.Round(musicVolumeUI.value, 2);
	}


	public void goToPage(int pageInd){
		if(pageInd < 0 || pageInd >= pages.Length){ return; } //Don't move to an index that is out of range!

		curPageInd = pageInd; //Keep track of the current page!

		//Set all page to inactive.
		for(int i=0; i<pages.Length; i++){
			pages[i].SetActive(false);
		}

		//Set the correct page to active.
		pages[pageInd].SetActive(true);
	}


	public void backButton(){
		//Save setting

		//Page 0
		ClientSettings.defaultIP = ipUI.text;
		ClientSettings.defaultPort = (int.TryParse(portUI.text, out ClientSettings.defaultPort)) ? int.Parse(portUI.text) : 4040; //A little messy, but this will try to parse and catch it with a default value if it fails in one line!
		ClientSettings.fpsCap = ( int.TryParse(fpsUI.value, out ClientSettings.fpsCap) ) ? ClientSettings.fpsCap : 60;
		ClientSettings.vSync = (vsyncUI.value == "On") ? true : false;
		ClientSettings.fullScreen = (fullscreenUI.value == "On") ? true : false;

		//Page 1
		ClientSettings.uiScale = (float)Math.Round(uiScaleUI.value, 2); //Round to 2 decimal places.

		//Page 2
		ClientSettings.masterVolume = (float)Math.Round(masterVolumeUI.value, 2);
		ClientSettings.effectVolume = (float)Math.Round(effectVolumeUI.value, 2);
		ClientSettings.musicVolume = (float)Math.Round(musicVolumeUI.value, 2);


		ClientSettings.SaveSettings();

		//Load the new quality settings! The network settings are applied automatically!
		titleCtrl.fixQualitySettings();

		//Fix IP settings!
		serverCtrl.reset();

		//Fix Sound Levels
		AudioLib.setVolumeLevels();

		//Go back to the ip page...
		titleCtrl.goToPage(1);
	}

	public void restoreDefaults(){
		ClientSettings.SetDefault();
		ClientSettings.SaveSettings();

		resetUIValues();
	}

	public void resetUIValues(){
		//Page 0
		ipUI.text = ClientSettings.defaultIP;
		portUI.text = ""+ClientSettings.defaultPort;
		fpsUI.value = ""+ClientSettings.fpsCap;
		vsyncUI.value = ClientSettings.vSync ? "On" : "Off";
		fullscreenUI.value = ClientSettings.fullScreen ? "On" : "Off";

		//Page 1
		uiScaleUI.value = ClientSettings.uiScale;

		//Page 2
		masterVolumeUI.value = ClientSettings.masterVolume;
		effectVolumeUI.value = ClientSettings.effectVolume;
		musicVolumeUI.value = ClientSettings.musicVolume;
	}

	
	//These are so UI button can easily access the page system!
	public void goToPage0(){
		goToPage(0);
	}

	public void goToPage1(){
		goToPage(1);
	}

	public void goToPage2(){
		goToPage(2);
	}


}
