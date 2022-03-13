using UnityEngine;

public class netBulletCtrl : MonoBehaviour {

	public Vector3 netPos;
	public Quaternion netRot;
	public float lerpSpeed = 17.0f;
	//public AudioClip soundShoot;

	void Start(){
		// Makeing the network bullets not have an flag for playing audio, instead make them play a sound in their "Start()" function so that they will make a sound whenever they come into view of any player!
		// The bullets will only be playing one clip at the start of their lifetime, so it is okay to the "AudioSource.PlayClipAtPoint()"
		AudioSource.PlayClipAtPoint(AudioLib.soundShoot, transform.position, AudioLib.effectVolume);
	}

	void Update () {
		float lerp = lerpSpeed; //float lerp = lerpSpeed * Time.deltaTime;
		//float lerp = lerpSpeed * Time.deltaTime * SmartLerp.networkLerp;
		transform.position = Vector3.Lerp(transform.position, netPos, lerp);
		transform.rotation = Quaternion.Lerp(transform.rotation, netRot, lerp);
	}
}
