using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Added
using UnityEngine.UI;


/*
 * Help:
 * 		1. https://forum.unity.com/threads/projecting-low-resolution-onto-a-full-screen-high-resolution.227823/
 * 		2. https://docs.unity3d.com/ScriptReference/RenderTexture.html
 * 		3. https://docs.unity3d.com/ScriptReference/Screen.SetResolution.html
 * 		4. https://docs.unity3d.com/ScriptReference/Screen.html
 * 		5. https://docs.unity3d.com/ScriptReference/Screen-fullScreenMode.html
 * 		6. https://docs.unity3d.com/ScriptReference/FullScreenMode.html
 * 
 * 		7. Maybe use "RenderTexture.useDynamicScale" Doc: https://docs.unity3d.com/ScriptReference/RenderTexture-useDynamicScale.html
 *		8. Low Res World UI Culling help: https://answers.unity.com/questions/735151/multiple-camera-depth-rendering.html
 *			(Be sure:
 *				1. The Game camera's culling is set to exclude the UI layer.
 *				2. The World UI camera's culling is set to only include/render the UI layer.
 *				-3. The World UI camera's Clear flag is set to "Depth Only"
 *				3. Instead of the last step, make the Clear Flags = SolidColor, where SolidColor's alpha is zero.
 *				4. The World UI camera's depth is set to 1.
 *				5. The Game camera's depth is set to -1.
 *				6. The World UI camera's RawImage UI object is lower in the hierarchy than the Game camera's UI object. This will allow it to render on top of the game camera's UI.)
 */

public class lowResCam : MonoBehaviour {

	
	public int width = 100;
	public int height = 100;
	public int HDScale = 10; //This is used to up the quality of the base RenderTexture so the pixels won't appear blurry.
	public int antiAliasing = 4; // Can be: 2, 4, or 8.
	// Be sure to set cam the UI camera's Culling Mask = "Nothing". This will only render the UI elements. https://docs.unity3d.com/Manual/Layers.html
	// Also set the camera's Clear Flags = solidColor, then the camera's Background color can be changed to whatever you want.
	// Optional: Might want to set the RenderCamera (not the above UICamera) as orthographic for 2D games! (Since the UICamera is only rendering UI, setting it orthographic would do nothing!)
	// Optional: Can use a UI Image behind the RenderTexture to make a frame around the gameplay window.

	
	public Camera RenderCamera;
	private RenderTexture renText; // Note: You can give the "RenderTexture" a color filter by changing its color!
	// Investigate RawImage material more later. Maybe able to use shaders!
	public RawImage renUI;


	public Camera WorldUICamera;
	private RenderTexture WorldUIRenText;
	public RawImage WorldUIRenUI;
	public float orthographicSize;

	public Camera sceneViewCamera; //This is the camera rendering to the screen that is pointed at the UI canvas.


	void Start () {
		sceneViewCamera = (sceneViewCamera == null) ? Camera.main : sceneViewCamera;

		//Generate RenderTexture object instead of pre-making them in the project!
		renText = MakeRendText();
		WorldUIRenText = MakeRendText();

		//Link cameras to the  RenderTextures
		RenderCamera.targetTexture = renText;
		WorldUICamera.targetTexture = WorldUIRenText;
		
		//Link the UI canvas objects to the RenderTextures.
		renUI.texture = renText; // This breaks anti-aliasing. Trilinear is better for smooth anti-aliasing. //renText.filterMode = FilterMode.Point;
		WorldUIRenUI.texture = WorldUIRenText;

		//Static camera setup...
		sceneViewCamera.orthographicSize = orthographicSize;
		RenderCamera.orthographicSize = orthographicSize;
		WorldUICamera.orthographicSize = orthographicSize;

		// Set the camera settings for perspective if that is the mode the cameras are in.
		if(!WorldUICamera.orthographic) WorldUICamera.transform.localPosition = new Vector3(WorldUICamera.transform.localPosition.x, WorldUICamera.transform.localPosition.y, orthographicSize*-2f);
		if(!RenderCamera.orthographic) RenderCamera.transform.localPosition = new Vector3(RenderCamera.transform.localPosition.x, RenderCamera.transform.localPosition.y, orthographicSize*-2f);
		

		pixelPerfectmaxHeightScreen(250, 250); //WAS: pixelPerfectmaxHeightScreen(250, 250); //Set a default view!
	}
	

	void Update () {
	}

	public void resizeScreen(int newWidth, int newHeight) { //This won't work unless the screen is in "Regular" mode...
		width = newWidth;
		height = newHeight;

		regularScreen();
	}

	public void regularScreen() {
		setRenUISize(width, height);
		renderHD();
	}

	public void stretchedScreen() {
		setRenUISize(Screen.width, Screen.height);

		renderHD();
	}

	public void maxHeightScreen() { //Keeps the same aspect, but with the max window/screen height...

		int maxHeight = Screen.height;
		int aspectMaxWidth = (maxHeight * width) / height;

		setRenUISize(aspectMaxWidth, maxHeight);
		renderHD();
	}

	public void pixelPerfectScreen(int resWidth, int resHeight, int displayWidth, int displayHeight) {
		width = resWidth;
		height = resHeight;
		HDScale = 1;

		setRenUISize(displayWidth, displayHeight);
		renderHD();
	}


	public void pixelPerfectmaxHeightScreen(int resWidth, int resHeight){
		width = resWidth;
		height = resHeight;
		HDScale = 1;

		int maxHeight = Screen.height;
		int aspectMaxWidth = (maxHeight * width) / height;

		setRenUISize(aspectMaxWidth, maxHeight);
		renderHD();
	}



	public void moveScreen(float x, float y) { //Move the virual screen inside of the window.
		//Note: using this function on the main lowResCam that is being used for mouse aiming will break the mouse aim!
		// you'd also have to move the sceneViewCam like in "LowResSquare".
		// But for a mini map it will be fine!
		renUI.rectTransform.Translate(x, y, 0);
		WorldUIRenUI.rectTransform.Translate(x, y, 0);
	}

	public void centerScreen() { //Move the virual screen inside of the window.
		renUI.rectTransform.localPosition = new Vector3 (0, 0, 0);
		WorldUIRenUI.rectTransform.localPosition = new Vector3 (0, 0, 0);
	}


	public void setRenUISize(int x, int y) {
		renText.Release(); //RenderTexture's dimensions can't be changed if they are already created, so Release them before the dimensions are changed and then Create them again.
		renText.width = x;
		renText.height = y;
		renText.Create();
		renUI.SetNativeSize(); //The easiest way to change the dimensions of the RawImage is to change it via NativeSize. So temporarily change the size of the RenderTexture and then revert it...

		//Set the orthographicSize for both cameras.
		RenderCamera.orthographicSize = orthographicSize;
		WorldUICamera.orthographicSize = orthographicSize;
		sceneViewCamera.orthographicSize = orthographicSize;

		if(!WorldUICamera.orthographic) WorldUICamera.transform.localPosition = new Vector3(WorldUICamera.transform.localPosition.x, WorldUICamera.transform.localPosition.y, orthographicSize*-2f);
		if(!RenderCamera.orthographic) RenderCamera.transform.localPosition = new Vector3(RenderCamera.transform.localPosition.x, RenderCamera.transform.localPosition.y, orthographicSize*-2f);
		//WorldUICamera.transform.localPosition = new Vector3(WorldUICamera.transform.localPosition.x, WorldUICamera.transform.localPosition.y, orthographicSize*-2f);
		//RenderCamera.transform.localPosition = new Vector3(RenderCamera.transform.localPosition.x, RenderCamera.transform.localPosition.y, orthographicSize*-2f);

		QualitySettings.antiAliasing = antiAliasing; //Set the "antiAliasing" in the QualitySettings.

		//Change the uicamera to match...
		WorldUIRenText.Release();
		WorldUIRenText.width = x;
		WorldUIRenText.height = y;
		WorldUIRenText.Create();
		WorldUIRenUI.SetNativeSize();
		WorldUICamera.ResetAspect();
	}

	public void renderHD() {
		//Apply the HDScale to the RenderTexture to avoid blurry pixels.
		renText.Release(); //RenderTexture's dimensions can't be changed if they are already created, so Release them before the dimensions are changed and then Create them again.
		renText.width = width*HDScale;
		renText.height = height*HDScale;
		renText.Create();

		RenderCamera.ResetAspect(); //The RenderTexture will be skewed/blurry if the camera rendering to it is not reset after that texture's dimensions are changed. This can also be done by setting the camera inactive and reactive again...
	}

	private RenderTexture MakeRendText(){
		RenderTexture rendText = new RenderTexture(100, 100, 24, RenderTextureFormat.ARGB32);
		rendText.filterMode = FilterMode.Point;
		rendText.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
		rendText.antiAliasing = 8;
		rendText.useDynamicScale = true;
		rendText.useMipMap = false;
		rendText.wrapMode = TextureWrapMode.Clamp;
		return rendText;
	}
}
