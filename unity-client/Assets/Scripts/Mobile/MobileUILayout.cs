using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Keybored.Mobile {
    public class MobileUILayout : MonoBehaviour {

        public GameObject[] JoystickUIElements; //Change this name to "UIJoysticks"...
        public Vector2[] JoystickUIPercentPos; //Each value should be between 0-1, representing a percent of the screen width or height.

        //This script will take care of moving and scaling the lobby & player count text.
        public TMP_Text LobbyTimer;
        public TMP_Text PlayerCount;
        public Text FPSText;
        public Button spectateBackToMenu;
        
        #if UNITY_IOS || UNITY_ANDROID //This is to avoid a unity warning for unused var on PC build...
            float baseTextYOffset = -225f;
            float baseTextYStep = -50f; //Space between text objects...
            float baseTextXOffset = -10f;
            int baseFontSize = 24;
            Vector2 baseRectSize = new Vector2(200, 50); //Width and height of rect...
        #endif

        void Start() {
            #if UNITY_EDITOR //This is for using scenes out of order during debugging...
                ClientSettings.ReadSettings();
            #endif

            #if UNITY_IOS || UNITY_ANDROID
                Vector2 fixedPos;
                for(int i=0; i<JoystickUIElements.Length; i++){
                    float xOffset = Screen.width * JoystickUIPercentPos[i].x;
                    float yOffset = Screen.height * JoystickUIPercentPos[i].y;
                    fixedPos = new Vector2(xOffset, yOffset);
                    JoystickUIElements[i].GetComponent<RectTransform>().position = fixedPos;

                    TouchJoystick joystick = JoystickUIElements[i].GetComponent<TouchJoystick>();
                    joystick.UIScale = ClientSettings.uiScale; //Don't read TouchLib scale here, that is done inside of the "TouchJoystick" script.
                }

                //Fix the Lobby & player count text sizes-------------------------------------------------------------
                float scaledXPos = baseTextXOffset*TouchLib.ScreenScale*ClientSettings.uiScale;
                float scaledYPos = baseTextYOffset*TouchLib.ScreenScale*ClientSettings.uiScale;
                float scaledYStep = baseTextYStep*TouchLib.ScreenScale*ClientSettings.uiScale;
                float scaledFontSize = baseFontSize*TouchLib.ScreenScale*ClientSettings.uiScale;
                Vector2 scaledRectSize = baseRectSize*TouchLib.ScreenScale*ClientSettings.uiScale;
                
                RectTransform rectTrans = LobbyTimer.GetComponent<RectTransform>();
                fixedPos = new Vector2(scaledXPos, scaledYPos);
                Vector2 zeroedTransform = new Vector2(Screen.width, Screen.height);
                rectTrans.position = fixedPos +zeroedTransform; //Note: For some reason setting "RectTransform.position" is always based off of the bottom-left corner no matter what the rect pivot is set to! That is why I need the extra transform ("zeroedTransform") here to make sure it is based on an offset of the top-right corner.

                rectTrans = PlayerCount.GetComponent<RectTransform>();
                fixedPos.y += scaledYStep;
                rectTrans.position = fixedPos +zeroedTransform;
                rectTrans.sizeDelta = scaledRectSize; //This will fix the overflow problem on the player count text!

                LobbyTimer.fontSize = scaledFontSize;
                PlayerCount.fontSize = scaledFontSize;
                
                FPSText.fontSize = Mathf.RoundToInt(scaledFontSize); //Just update the font size of the FPS text, nothing else...
                FPSText.rectTransform.position = new Vector2(Screen.width*0.025f, FPSText.rectTransform.position.y); //Move the FPS Test 2.5% to the right away from the left edge to avoid overlap with rounded corners.

                spectateBackToMenu.transform.localScale = new Vector3(TouchLib.ScreenScale*ClientSettings.uiScale, TouchLib.ScreenScale*ClientSettings.uiScale, TouchLib.ScreenScale*ClientSettings.uiScale);

                //Note: Make sure the TMP "Wrapping" is set to "Disabled", and "Overflow" is set to "Overflow".
                //	Also make sure that the TMP are right-aligned!
                //	For the playerCount text you may want to have "Wrapping" set to "Enabled", text right-aligned, and "Overflow" is set to "Overflow".
                //END text sizes fixes-------------------------------------------------------------
            #endif
        }
    }
}