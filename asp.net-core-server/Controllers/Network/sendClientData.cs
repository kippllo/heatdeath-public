/*
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static gameBackendTest.GameObj.StringFormat;
*/

namespace Keybored.BackendServer.Network {
    

    public class sendClientData {
		public int gameID;
		public int syncID;
		//public int syncKey;
		public string localPlayer;
		public string localBullets;
	}
	
	public class syncPackage {
		public bool uploadSuccess { get; set;}
		public string dataSync { get; set;}
	}

}