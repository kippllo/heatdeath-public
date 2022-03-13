using UnityEngine;
using Newtonsoft.Json;

public abstract class NetworkObj {
	public int objID { get; set; }

	protected GameObject _prefab; //Any non-public (so protected or private) properties will not serialize with "JsonConvert.SerializeObject"! This is very useful when using a "UnityEngine" class object that is not present on the server-side.
	public void setPrefab(GameObject prefabIn) {
		_prefab = prefabIn;
	}

	protected GameObject _parent;
	public void setParent(GameObject parentIn) {
		_parent = parentIn;
	}

	public void updateObject(){
		GameObject localObj = GameObject.Find(""+ objID); //Object must be named the objID!

		if (localObj) { //If the local version is not null...
			updateProperties(localObj); //Each class that inherents this will define its own property update method.
			localObj.transform.GetChild(0).tag = "status:Updated"; //Always make the "NetworkTrack" be the first child!
		}


		if (!localObj) { //If the object is not in the scene, but it is not destroyed on the server, spawn the object!
			localObj = instantiateObj();
			updateProperties(localObj);
			localObj.transform.GetChild(0).tag = "status:Updated";
		}
	}

	public abstract GameObject instantiateObj();
	public abstract void updateProperties(GameObject localObj);


	public override string ToString()
	{
		return JsonConvert.SerializeObject(this, new JsonSerializerSettings (){ ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
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
		objID = -1;
		hp = -1;
		pos = new Vector3Thin();
		rot = new Vector3Thin();
		username = "Not_A_Name";
	}

	public playerObj(int objIDIn, float hpIn, Vector3 posIn, Vector3 rotIn, string usernameIn, bool partsysFireEmitIN, bool partsysDamageEmitIN, bool flgSoundDeathIN){
		objID = objIDIn;
		hp = hpIn;
		pos = Vector3Thin.FromVector3(posIn);
		rot = Vector3Thin.FromVector3(rotIn);
		username = usernameIn;
		partsysFireEmit = partsysFireEmitIN;
		partsysDamageEmit = partsysDamageEmitIN;
		flgSoundDeath = flgSoundDeathIN;
	}

	public override GameObject instantiateObj() { //Might could be moved to base class...
		GameObject localObj = UnityEngine.Object.Instantiate(_prefab, pos.ToVector3(), Quaternion.Euler(rot.ToVector3()), _parent.transform ); //Maybe add error handle if _parent is null...
		localObj.name = ""+ objID; //Name the object the tag...
		localObj.GetComponent<netShipCtrl>().username = username; //Set the username.
		return localObj;
	}

	public override void updateProperties(GameObject localObj) {
		netShipCtrl localCtrl = localObj.GetComponent<netShipCtrl>();
		
		/* Debug.LogWarning(""+(pos != null) );
		Debug.LogWarning(""+(localCtrl != null) ); //this is where the error happens... I think one player is getting assigned multiple ID's and that is causing this error...
		Debug.LogWarning(""+(localCtrl.netPos != null) );
		Debug.LogWarning("end"); */

		localCtrl.netPos = pos.ToVector3();
		localCtrl.netRot = Quaternion.Euler(rot.ToVector3());
		localCtrl.partsysFireEmit = partsysFireEmit;
		localCtrl.partsysDamageEmit = partsysDamageEmit;
		localCtrl.flgSoundDeath = flgSoundDeath;

		////localCtrl.username = username;
		localCtrl.hp = hp;
	}
}


public class bulletObj: NetworkObj {
	public Vector3Thin pos;
	public Vector3Thin rot;

	public bulletObj() {
		objID = -1;
		pos = new Vector3Thin();
		rot = new Vector3Thin();
	}

	public bulletObj(int objIDIn, Vector3 posIn, Vector3 rotIn){
		objID = objIDIn;
		pos = Vector3Thin.FromVector3(posIn);
		rot = Vector3Thin.FromVector3(rotIn);
	}

	public override GameObject instantiateObj() {
		GameObject localObj = UnityEngine.Object.Instantiate(_prefab, pos.ToVector3(), Quaternion.Euler(rot.ToVector3()), _parent.transform );
		localObj.name = ""+ objID;
		return localObj;
	}

	public override void updateProperties (GameObject localObj) {
		netBulletCtrl localCtrl = localObj.GetComponent<netBulletCtrl>();
		localCtrl.netPos = pos.ToVector3();
		localCtrl.netRot = Quaternion.Euler(rot.ToVector3());
	}
}
