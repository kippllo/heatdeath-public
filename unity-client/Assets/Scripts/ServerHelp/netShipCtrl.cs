using UnityEngine;
using UnityEngine.UI;

public class netShipCtrl : MonoBehaviour {

	public Vector3 netPos;
	public Quaternion netRot;
	
	public Text usernameUI;
	public string username;

	public float hp = 100;
	private float hpMax = 100; //Used only to set the enemy hp ui.
	public Slider hpUI;

	public float lerpSpeed = 30.0f;

	public ParticleSystem partsysFire;
	public ParticleSystem partsysDamage;
	public bool partsysFireEmit;
	public bool partsysDamageEmit;
	public bool flgSoundDeath;

	
	AudioSource aud;
	private float audTimerDmg = 0f; //You cannot rely on the server sending only one audio/particle flag. So, only play on a timer, not every package.
	private float audTimerDmgMax = 0.075f; //Pretty good: 0.05f; //0.025f;
	private bool audFlgDeath = false;
	//private float audTimerDeathMax = 0.01f;

	void Start() {
		//Set the username UI text to the network username.
		usernameUI.text = username;
		
		//Setup the HP slider based on vars...
		hpUI.maxValue = hpMax;

		// Audio Source Setup
		aud = GetComponent<AudioSource>();
		aud.loop = false;
		aud.playOnAwake = false;
	}

	void Update () {
		
		float lerp = lerpSpeed; //WAS: float lerp = lerpSpeed * Time.deltaTime;

		//float lerp = lerpSpeed * Time.deltaTime * SmartLerp.networkLerp; //Base is about 0.33 //Lower is smoother!
		transform.position = Vector3.Lerp(transform.position, netPos, lerp);
		transform.rotation = Quaternion.Lerp(transform.rotation, netRot, lerp);

		hpUI.value = hp;

		//Network Particle controls.
		if(partsysFireEmit){
			partsysFire.Emit(5);
			partsysFireEmit = false;
		}
		if(partsysDamageEmit){
			partsysDamage.Emit(5);
			partsysDamageEmit = false;

			audTimerDmg += Time.deltaTime;
			if(audTimerDmg >= audTimerDmgMax){
				audTimerDmg -= audTimerDmgMax;
				aud.PlayOneShot(AudioLib.soundDamage, AudioLib.effectVolume);
			}
		} else { audTimerDmg = audTimerDmgMax; } // Reset the audio timer if the network flag is false. Set it to max so once this enemy is hit again, it will immediately play the audio.

		if(flgSoundDeath && !audFlgDeath){
			audFlgDeath = true;
			AudioSource.PlayClipAtPoint(AudioLib.soundDeath, gameObject.transform.position, AudioLib.effectVolume);
			//aud.PlayOneShot(AudioLib.soundDeath, AudioLib.effectVolume);
		}

		//if(!partsysDamageEmit) audTimerDmg = 0; // Reset the audio timer if the network flag is false.
	}
}
