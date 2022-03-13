using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;


public class ClientSettings {

	[JsonProperty]
	public static string defaultIP;
	[JsonProperty]
	public static int defaultPort;
	[JsonProperty]
	public static int fpsCap;
	[JsonProperty]
	public static bool vSync;
	[JsonProperty]
	public static bool fullScreen;
	[JsonProperty]
	public static float uiScale;
	
	// Note: All volume setting vars should be read from "AudioLib" instead of "ClientSettings" because AudioLib
	// applies extra settings to the base values here.
	// For example, to apply the masterVolume to the other volumes, just multiply them inside of the AudioLib.
	[JsonProperty]
	public static float masterVolume; // All volumes should be a range of 0.0-1.0 on a UI slider.
	[JsonProperty]
	public static float effectVolume; //Call this "Game Sounds" in the UI that the users sees...
	[JsonProperty]
	public static float musicVolume;
	

	public static void SetDefault(){
		defaultIP = "heatdeathserver001.keybored.net";
		defaultPort = 54040;
		fpsCap = 60;
		vSync = false;
		fullScreen = false;
		uiScale = 1f;
		masterVolume = 1f;
		effectVolume = 1f;
		musicVolume = 1f;

		#if UNITY_ANDROID || UNITY_IOS
		fullScreen = true; //Set mobile to fullscreen by default!
		#endif
	}


	///<summary>
	///  Writes this settings class as a JSON file.
	///  Note: I'm not using async read/write here because it will only run once at the start of the program.
	///  And I don't want to have to await this function!
	///</summary>
	public static void SaveSettings(){
		string settingsPath = Application.persistentDataPath + "/settings";
		try{
			if(!Directory.Exists(settingsPath)){
				Directory.CreateDirectory(settingsPath);
			}

			//JsonSerializerSettings jsonSettings = new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
			string json = JsonConvert.SerializeObject(new ClientSettings(), new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
			byte[] strBytes = Encoding.ASCII.GetBytes(json);

			File.WriteAllBytes(settingsPath + "/setting.json", strBytes);
		} catch(Exception err){
			Debug.Log(err.ToString());
		}
	}


	public static void ReadSettings(){
		string settingsFullFilePath = Application.persistentDataPath + "/settings/setting.json";
		try{
			if( File.Exists(settingsFullFilePath) ){
				byte[] readData = File.ReadAllBytes(settingsFullFilePath);
				string json = Encoding.ASCII.GetString(readData);

				//This will set static properties to what they are in the saved file.
				JsonConvert.DeserializeObject<ClientSettings>(json, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.None}); //I got rid of the "ClientSettings objData = ..." part so Unity won't complain.
			} else {
				// If the file cannot be found
				// Save the default settings to a JSON file.
				SetDefault();
				SaveSettings();
			}
		} catch(Exception err){
			Debug.Log(err.ToString());
		}
	}

}



/* NOTE:
	You can add and take away properties freely. The JSON file will save the updates and keep the old user settings!

	To take away old properties still left in the JSON use:
		ClientSettings.ReadSettings();
		ClientSettings.SaveSettings();

	To add new properties not present in the JSON use:
		ClientSettings.ReadSettings(); //Reading the file first keep all the old properties set!
		ClientSettings.newProp = "123WORKS"; //Set a default value or user input here.
		ClientSettings.SaveSettings();
*/