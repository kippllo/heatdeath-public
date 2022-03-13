using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using static gameBackendTest.GameObj.StringFormat;
using Keybored.BackendServer.GameEngine;


namespace Keybored.BackendServer.Network {

    public abstract class NetworkObj {
		public int objID { get; set; }

		[JsonIgnore]
		public static readonly int ObjIDLimit = 4;

		protected static T deserialize<T>(string serializedObj) {
			T deserObj = JsonConvert.DeserializeObject<T>(serializedObj, new JsonSerializerSettings{
				TypeNameHandling = TypeNameHandling.None
			});
			return deserObj;
		}
	}


	public class playerObj: NetworkObj {
		public float hp;
		public string username;
		public Vector3Thin pos;
		public Vector3Thin rot;
		public bool partsysFireEmit;
		public bool partsysDamageEmit;
		public bool flgSoundDeath;

		public playerObj(){
			hp = 0;
			pos = new Vector3Thin();
			rot = new Vector3Thin();
			username = "";
		}

		public static playerObj FromString(string serializedObj){ //This is a work around because C# can't have abstract-static methods...
			playerObj deserObj = NetworkObj.deserialize<playerObj>(serializedObj); //I could just use: "return JsonConvert.DeserializeObject<playerObj>(serializedObj);", but I wanted to use a generic method.
			deserObj = (deserObj == null) ? new playerObj() : deserObj;
			return deserObj;
		}
	}


	public class bulletObj: NetworkObj {
		public Vector3Thin pos;
		public Vector3Thin rot;

		[JsonIgnore]
		private float bulletSpeed = 25.0f;
		////[JsonIgnore]
		////public Vector3Thin origin;
		
		private string _source = "";
		public string source() { //This is a function and not a "getter" so that it will not show up in "JsonConvert.SerializeObject" output.
			//If "_source" has never been set, cache its value once here.
			_source = (_source == "") ? JsonConvert.SerializeObject(this, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) : _source;
			return _source;
		}

		public void resetSource(){
			// This will reset the source cache string. This is needed for updating bot's bullets.
			_source = JsonConvert.SerializeObject(this, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
		}

		public bulletObj(){
			pos = new Vector3Thin();
			rot = new Vector3Thin();
		}

		public void stepBullet(float dt){ //This is not on the client-side / frontend.
            // Move bullet in a straight line...
            pos.x += bulletSpeed* (float)Math.Cos(GameMath.degreesToRadians(rot.z)) *dt;
            pos.y += bulletSpeed* (float)Math.Sin(GameMath.degreesToRadians(rot.z)) *dt;
            resetSource(); //Reset the cached string that is sent to players...
        }

		public bool isOwner(int syncID){
			string strObjID = ""+objID;
			int idLength = strObjID.Length - ObjIDLimit;
			int bulletOwnerSyncID = int.Parse(strObjID.Substring(0, idLength)); // The syncID is always the first part of the bullet's 'objID'. I might need to cache this later for speed...
			return syncID == bulletOwnerSyncID;
		}
	}
}