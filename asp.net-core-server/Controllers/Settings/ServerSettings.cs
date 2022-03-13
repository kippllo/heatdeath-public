using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Keybored.BackendServer.Logging;


namespace Keybored.BackendServer.Settings {

    // This class will be saved as a plain text JSON.
    // The file will be read each time the server start
    // and the settings in it will be used for the server.
    // Other classes should reference this class for setting data vars!
    public class ServerSettings {
        
        [JsonProperty] //This will allow static properties to be serialized.
		public static int port;
        [JsonProperty]
        public static int simultaneousRounds; //The number of simultaneous rounds the master class will allow.
        [JsonProperty]
        public static int connectionsLimit; //The max number of player connections a single game round can have.
        [JsonProperty]
        public static float lobbyWaitTime;
        [JsonProperty]
        public static float connectionDropTime; //The time a connect can not send a packet and still be marked as "connected"
        [JsonProperty]
        public static float initialConnectionGracePeriod;
        [JsonProperty]
        public static float objRenderDist; //The max distance a network object can be from a player and still be sent to that player to be rendered. This distance is measured by a 3D line's length.

        [JsonProperty]
        public static float zoneTime; //The starting time for a zone before being scaled.
        [JsonProperty]
        public static float zoneTimeScale; //The percent that each zone time will shrink by.

        [JsonProperty]
        public static bool FillWithAI;
        [JsonProperty]
        public static bool LANProfile;

        [JsonProperty]
        public static string clientVer;
        [JsonProperty]
        public static string maintenanceMessage;
        


        private static void SetDefault(){
            //Set all vars back to defaults!
            port = 54040;
            simultaneousRounds = 10;
            connectionsLimit = 50;
            lobbyWaitTime = 60.0f;
            connectionDropTime = 1.0f;
            initialConnectionGracePeriod = 10.0f;
            objRenderDist = 40.0f;

            zoneTime = 100.0f;
            zoneTimeScale = 0.75f;

            FillWithAI = true;
            LANProfile = false;

            clientVer = "0.1.0";
            maintenanceMessage = "";
        }

        //Writes this settings class as a JSON file.
        //  Note: I'm not using async read/write here because it will only run once at the start of the program.
        //  And I don't want to have to await this function!
        public static void SaveSettings(){
            try{
				if(!Directory.Exists("./settings")){
					Directory.CreateDirectory("./settings");
				}

				string json = JsonConvert.SerializeObject(new ServerSettings(), new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
				byte[] strBytes = Encoding.ASCII.GetBytes(json);

                File.WriteAllBytes("./settings/setting.json", strBytes);
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
			}
        }


        public static void ReadSettings(){
			try{
				if( File.Exists("./settings/setting.json") ){
                    byte[] readData = File.ReadAllBytes("./settings/setting.json");

					string json = Encoding.ASCII.GetString(readData);
					
                    //This will set static properties to what they are in the saved file.
                    ServerSettings objData = JsonConvert.DeserializeObject<ServerSettings>(json, new JsonSerializerSettings{
						TypeNameHandling = TypeNameHandling.None
					});
				} else {
                    // If the file cannot be found
                    // Save the default settings to a JSON file.
                    SetDefault();
                    SaveSettings();
				}
			} catch(Exception err){
				FileLogger.logErrorToFile(err.ToString());
			}
		}

    }

}