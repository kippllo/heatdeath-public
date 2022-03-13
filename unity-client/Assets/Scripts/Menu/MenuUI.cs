using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
//using UnityEditor; //For PlayerSettings, Cannot be used in runtime code...

public class MenuUI : MonoBehaviour {

	
	//public TMP_InputField ip;
	//public TMP_Text onlineCountUI;
	
	//public TMP_Text serverMessageUI;
	
	public Button reconnUI;
	//public Toggle saveReconnUI;

	public TMP_InputField usernameUI;

	public TitleCtrl titleCtrl;
	//public float onlineCountTimer = 0.0f;
	//public float onlineCountTimerMax = 5.0f;


	void Start () {
		// Hied/Show Reconnect button.
		if(serverCtrl.isReconnectFileValid){
			reconnUI.gameObject.SetActive(true);
		} else {
			reconnUI.gameObject.SetActive(false);
		}
	}
	

	void Update () {
		if(Input.GetKeyDown("return")){ //is this working??
			connectToServerViaIP();
		}

		/*
		if(Input.GetKeyDown("tab")){
			if(usernameUI.isFocused){
				ip.ActivateInputField();
			}
			else if(ip.isFocused){
				usernameUI.ActivateInputField();
			}
		}
		*/

		//Load the map editor if the user holders the right keys...
		if( Input.GetKey("m") && Input.GetKey("a") && Input.GetKey("p") ){
			SceneManager.LoadScene(3);
		}

	}

	public async void connectToServerViaIP(){

		//If the client is outdated, don't let the player join a game!
		if(await titleCtrl.isClientOutdated()){
			titleCtrl.goToPage(0);
			return;
		}

		//Remember "serverCtrl.urlPrefixDefault" is set in "serverCtrl.reset()"!
		serverCtrl.urlPrefix = serverCtrl.urlPrefixDefault; //Work around for now... Remove var "serverCtrl.urlPrefix" later!
		
		//Add banned words client-side check on username.
		// Also, if the name is blank choose a random name from a list I fill later.
		// Also, put a 25 char cap on length.
		//int maxNameLength = 16;
		//serverCtrl.username = (usernameUI.text.Length > maxNameLength) ? usernameUI.text.Substring(0, maxNameLength) : usernameUI.text;
		serverCtrl.username = fixUsername();

		serverCtrl.saveReconn = true;//Just leave this as static for now. remove it later and make serverCtrl always save a reconnection file!
		SceneManager.LoadScene(1);
	}


	public async void connectToServerViaReconnect(){
		//If the client is outdated, don't let the player join a game!
		if(await titleCtrl.isClientOutdated()){
			titleCtrl.goToPage(0);
			return;
		}

		serverCtrl.tryReconnect = true; //This flag will cause a reconnect when the scene is loaded.
		SceneManager.LoadScene(1);
	}

	public void gotoSettingsPage(){
		titleCtrl.goToPage(2);
	}


	string fixUsername() {
		int maxNameLength = 24; //Was: 16
		string[] bannedSubStrings = new string[] { "\\", "'", "\"", //Note: These all need to be lower case.
		"ass", "shit", "piss", "damn", "fuck", "dick", "penis", "vagina", "cunt", "sex",
		"pedo", "nigger", "niger", "nigga", "niga", "faggot", "fag", "bitch", "bastard", "slut", "boob", "boobs" };
		
		string[] defaultNames = new string[] { "I Need A Name", "3.14", "Doge",
		"Bonbon", "I am Hacker", "Hacker", "Triggered", "Baka", "Sub to Pewds",
		"White Boi", "TouchaMySpaghet", "Ok Boomer", "shrek", "batman", "Sanic",
		"Sans", "Do You Know Da Wae", "Hey", "AllYourBaseAreBelongToUs", "Lol Cat",
		"Herp Derp", "Big Chungus", "Waifu", "Its Over 9000", "Leeroy Jenkins",
		"REEEEEEEE", "Desu", "Creeper Aww Man", "Vsauce",
		"Top10 SaddestAnimeDeaths", "Joker", "Fake News", "I Have $3", "Oh hi Mark",
		"Darth Vader", "Half-Life 3", "Que Será, Será", "Knights Who Say Ni!" };
		
		string strReturn = usernameUI.text; //Grab the username from the UI.

		//Replace the banned chars with empty space.
		/*for(int i=0; i<bannedSubStrings.Length; i++){
			strReturn = strReturn.Replace(bannedSubStrings[i], "");
		}*/

		//Replace the banned chars with empty space.
		// To do this first make a copy of the username in all lower case letters.
		// Then find any index of the banned strings.
		// Then remove the banned string from both the lower case copy and the original username.
		string usernameLowerCase = strReturn.ToLower();
		for(int i=0; i<bannedSubStrings.Length; i++){
			while( usernameLowerCase.IndexOf(bannedSubStrings[i]) != -1) {
				int lengthOfBanned = bannedSubStrings[i].Length;
				int indOfBanned = usernameLowerCase.IndexOf(bannedSubStrings[i]);
				usernameLowerCase = usernameLowerCase.Remove(indOfBanned, lengthOfBanned);
				strReturn = strReturn.Remove(indOfBanned, lengthOfBanned);
			}
		}

		//If the user did not pick a name, or put a banned name, assign them a random default name.
		if(strReturn == ""){
			//return defaultNames[Random.Range(0, defaultNames.Length)];
			strReturn = defaultNames[Random.Range(0, defaultNames.Length)];
		}
		
		//Only after any banned chars have been replaced check if the name is still too long.
		strReturn = (strReturn.Length > maxNameLength) ? strReturn.Substring(0, maxNameLength) : strReturn;
		
		return strReturn;
	}



	

}
