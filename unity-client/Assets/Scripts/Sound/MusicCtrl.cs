#pragma warning disable CS0649 // Turns off the warning when using "[SerializeField]": "warning CS0649: Field 'charCtrl.soundShoot' is never assigned to, and will always have its default value null".
using UnityEngine;
using System.Threading.Tasks;

public class MusicCtrl : MonoBehaviour {
    
    // "[SerializeField]" makes Unity show a private variable in the inspector. Also, You can hind a public var from the inspector using "[HideInInspector]".
    [SerializeField]
    AudioSource audBG;
    [SerializeField]
    AudioSource audFight;

    private bool flgIsFading;


    void Start() {
        // audBG Source Setup
		audBG.playOnAwake = false;
        audBG.spatialBlend = 0f; // Make the music be 2D audio.

        // Play the set BG music on loop:
        playBGMusic(AudioLib.musicBackground);

        // audFight Source Setup
        audFight.playOnAwake = false;
        audFight.spatialBlend = 0f;
    }


    private void playBGMusic(AudioClip bgClip) {
        audBG.loop = true; //This line could be moved to the "Start()" audio setup...
        audBG.volume = AudioLib.musicVolume;
        audBG.clip = bgClip;
        audBG.Play();
    }


    private async void playFightMusic(AudioClip fightClip) {
        if(!audFight.isPlaying && !flgIsFading){
            try{
                await fadeAudio(audBG);
            } catch (MissingReferenceException err){
                Debug.Log("Music fade audio source destroyed because game scene changed. \n\n" + err);
                return;
            }
            
            // Play the fight music.
            audFight.loop = true;
            audFight.volume = AudioLib.musicVolume;
            audFight.clip = fightClip;
            audFight.Play(); //Note: I don't use "UnPause()" here because the fight audio might not be playing yet.
        }
    }


    private async void pauseFightMusic() {
        if(audFight.isPlaying && !flgIsFading){
            // Pause the fight music.
            try{
                await fadeAudio(audFight);
            } catch (MissingReferenceException err){
                Debug.Log("Music fade audio source destroyed because game scene changed. \nIgnore the following error: \n\n" + err); // There is nothing to worry about! This error just happens because the game scene has change, but this thread was still running.
                return; // It might be a good idea to reset the "flgIsFading" to "false" here...
            }
            
            // Restore the BG music to regular volume.
            audBG.volume = AudioLib.musicVolume;
            audBG.UnPause();
        }
    }


    public void playOneShot(AudioClip oneShotClip){
        audBG.PlayOneShot(oneShotClip, AudioLib.musicVolume); // Note: This is played using the "musicVolume".
    }


    private async Task fadeAudio(AudioSource audSrc){
        flgIsFading = true;
        
        int maxSteps = 16; // The total number of steps it will take to fade the audio source to zero volume.
        float totalFadeTime = 1.5f; // The total amount of time (in seconds) the fade will take to complete.

        int fadeSpeed = ((int)totalFadeTime*1000) / maxSteps; // Calc the number of milliseconds to wait between music volume decreases. "fadeSpeed * maxSteps" will give the total fade time to zero.    !DON'T SET THIS, LET IT BE CALCULATED!
        float fadeAmount = audSrc.volume / maxSteps; //Calc the amount to fade for the fade steps specified.    !DON'T SET THIS, LET IT BE CALCULATED!
        

        for(int s=0; s<maxSteps; s++){
            await Task.Delay(fadeSpeed);
            audSrc.volume -= fadeAmount;
        }
        audSrc.volume = 0f; //Just to be sure the volume is totally zero.
        audSrc.Pause(); // Pause the audio so it can be played again from the same spot it stopped at.

        flgIsFading = false;
    }


    void Update() {
        if(GameSync.curRenderFrame != null && GameSync.curRenderFrame.getPlayers().Length > 0){
            // Play fight music.
            playFightMusic(AudioLib.musicFight);

        } else {
            //Crossfade back to regular music
            pauseFightMusic();
        }
    }
}