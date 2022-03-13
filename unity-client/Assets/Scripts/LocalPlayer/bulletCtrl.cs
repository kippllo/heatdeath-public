using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletCtrl : MonoBehaviour {

	public float lifetime = 1.0f;
	public float speed = 1.0f;
	public int objID;

	void Start () {
		//Get this object's Server ID.
		objID = serverCtrl.getNextObjID();
		gameObject.name = ""+ objID;

		Destroy(gameObject, lifetime);
	}
	
	void Update () {
		Vector3 move = new Vector3(speed, 0, 0);
		transform.Translate(move * Time.deltaTime);
	}

	public bulletObj toSyncData(){
		bulletObj bulletData = new bulletObj (objID, transform.position, transform.rotation.eulerAngles);
		return bulletData;
	}
}
