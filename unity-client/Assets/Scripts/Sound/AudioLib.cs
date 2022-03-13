using UnityEngine;


public class AudioLib : MonoBehaviour {

    // Static properties that other scripts will use.
    public static AudioClip soundShoot;
	public static AudioClip soundDamage;
	public static AudioClip soundDeath;
    public static AudioClip musicBackground; //Maybe have different type of music that play. Like Background & Battle??
    public static AudioClip musicFight;
    
    // Access these to get the correct volume for all sound played!
    public static float effectVolume;
    public static float musicVolume;

    
    // Non-Static properties used to set the static ones. These are set in the Unity inspector.
    public AudioClip default_soundShoot;
	public AudioClip default_soundDamage;
	public AudioClip default_soundDeath;
    public AudioClip default_musicBackground;
    public AudioClip default_musicFight;



    void Start() {
        setVolumeLevels();
        setDefaultSounds();
        // Maybe add //masterVolume = ClientSettings.masterVolume;
    }
    
    public static void setVolumeLevels(){
        ClientSettings.ReadSettings(); //Make sure that "ClientSettings" has already been read from the file!
        
        effectVolume = ClientSettings.effectVolume * ClientSettings.masterVolume;
        musicVolume = ClientSettings.musicVolume * ClientSettings.masterVolume;
    }

    public void setDefaultSounds(){
        AudioLib.soundShoot = default_soundShoot;
        AudioLib.soundDamage = default_soundDamage;
        AudioLib.soundDeath = default_soundDeath;
        AudioLib.musicBackground = default_musicBackground;
        AudioLib.musicFight = default_musicFight;
    }

    
    // Change to the Easter Egg sounds if the user has a certain username or ship skin on.

    
    /*
    public void setRetroSounds(){ // Call when username includes "galaga", "pacman", or other retro game titles.
        AudioLib.soundShoot = default_soundShoot;
        AudioLib.soundDamage = default_soundDamage;
        AudioLib.soundDeath = default_soundDeath;
    }

    public void setGuitarSounds(){ } // If Username includes "Guitar"
    public void setMemeSounds(){ } // If Username includes "meme". (Might have to take "meme" out of the random names? Maybe don't?)
    public void setAllYourBaseSounds(){ } // If Username includes "ZeroWing" or "Zero Wing". Maybe also play if the user has the Zero Wing ship skin on.

    public void setClassicalSounds(){ // If Username includes "Beethoven", "Wolfgang", "Mozart", "Bach", etc.
        AudioLib.musicBackground = symphony_musicBackground;
    }

    */
}
