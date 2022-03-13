using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followPlayer : MonoBehaviour {


	public float lerpSpeed = 0.05f;
	public bool instantFollow = false;
	private GameObject player;

	
	void Start () {
	}
	

	void Update () {
		if(!player){
			player = GameObject.FindGameObjectWithTag("localPlayer");
		} else {
			Vector3 MoveVec = new Vector3(0, 0, 0);
			MoveVec.x = player.transform.position.x;
			MoveVec.y = player.transform.position.y;
			
			transform.position = (instantFollow) ? MoveVec :  Vector3.Lerp(transform.position, MoveVec, lerpSpeed*Time.deltaTime);
			//transform.position = (instantFollow) ? Vector3.Lerp(transform.position, MoveVec, 1) :  Vector3.Lerp(transform.position, MoveVec, lerpSpeed*Time.deltaTime);
		}
	}
}
