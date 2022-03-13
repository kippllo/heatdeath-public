using UnityEngine;

public class WorldUIRot : MonoBehaviour {
	Quaternion startRot;
	
	void Start () {
		//startRot = transform.rotation;
		startRot = Quaternion.identity; //Reconnection UI rot fix...
	}
	
	void Update () {
		transform.rotation = startRot;
	}
}
