using UnityEngine;

public class mapCtrl : MonoBehaviour {

	void Start () {
	}
	
	void Update () {
	}


	void OnTriggerEnter(Collider hit){ //void OnCollisionEnter(Collision hit){
		if (hit.gameObject.tag == "bullet") {
			Destroy (hit.gameObject, 0.05f); //Wait a bit before dedstroying the bullet so it looks better over the network.
		}
	}

}
