//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Be sure to set cam the UI camera's Culling Mask = "Nothing". This will only render the UI elements. https://docs.unity3d.com/Manual/Layers.html
// Also set the camera's Clear Flags = solidColor, then the camera's Background color can be changed to whatever you want.
// Optional: Might want to set the RenderCamera (not the above UICamera) as orthographic for 2D games! (Since the UICamera is only rendering UI, setting it orthographic would do nothing!)
// Optional: Can use a UI Image behind the RenderTexture to make a frame around the gameplay window.


//NOTE TO SELF: Look into "Custom Render Textures" which can add shaders on top of render Textures!		Help: https://docs.unity3d.com/Manual/class-CustomRenderTexture.html


public class LowResSquare : MonoBehaviour {

	
	public int width = 250;
	public int height = 250;
	private int HDScale; //This is used to up the quality of the base RenderTexture so the pixels won't appear blurry.
	public int antiAliasing = 4; // Can be: 2, 4, or 8.


	public Camera RenderCamera;
	// Note: You can give the "RenderTexture" a color filter by changing its color!
	private RenderTexture renText;
	// Investigate RawImage material more later. Maybe able to use shaders!
	public RawImage renUI;

	public Camera WorldUICamera;
	private RenderTexture WorldUIRenText;
	public RawImage WorldUIRenUI;
	private float orthographicSize;

	public Camera sceneViewCamera; //This is the camera rendering to the screen that is pointed at the UI canvas.
	private float camScale = 1.0f;
	private float baseOrthSize;

	private Vector2 screenRes;


	void Start () {
		screenRes =  new Vector2(Screen.width, Screen.height);
		sceneViewCamera = (sceneViewCamera == null) ? Camera.main : sceneViewCamera; //If the scene camera is blank, use the main camera as the default.
		baseOrthSize = sceneViewCamera.orthographicSize;
		
		//Generate RenderTexture object instead of pre-making them in the project!
		renText = MakeRendText();
		WorldUIRenText = MakeRendText();

		//Link cameras to the  RenderTextures
		RenderCamera.targetTexture = renText;
		WorldUICamera.targetTexture = WorldUIRenText;

		//Link the UI canvas objects to the RenderTextures.
		renUI.texture = renText;
		WorldUIRenUI.texture = WorldUIRenText;

		orthographicSize = baseOrthSize;

		// Side Note: This breaks anti-aliasing. Trilinear is better for smooth anti-aliasing. //renText.filterMode = FilterMode.Point;

		////width = 250; //First set the base size like this.
		////height = 250;
		HDScale = 1;
		renText.filterMode = FilterMode.Point;
		resizeScreen(1.0f); //This initialize the LowResSpace to the full scale. You can change the slace later by using it also. //Then this will scale based on the base size. This will be both 10% of the base resolution and 10% of the display size.
		//moveScreen(-55f, -55f); //After that you can move the screen freely.


		//width = 150; //If you do something like this, you can change the shape to not be a square!
		//height = 50;
		//resizeScreen(0.5f); //Testing
		//moveScreen(-55f, -55f); //Testing
	}
	

	void Update () {
		if(screenRes.x != Screen.width || screenRes.y != Screen.height) {
			//The window has changed sizes, reset the LowResSquares to match!
			resizeScreen(camScale);

			Vector3 currPost = renUI.rectTransform.localPosition;
			centerScreen();
			moveScreen(currPost.x, currPost.y);
			
			screenRes =  new Vector2(Screen.width, Screen.height); //Cache the new resolution.
		}
	}

	public void resizeScreen( float sizScale ){
		camScale = sizScale;

		int baseWidth = width; //Save a copy of the regular width and height so it can be put back correctly...
		int baseHeight = height;

		width = Mathf.RoundToInt(baseWidth * camScale);
		height = Mathf.RoundToInt(baseHeight * camScale);

		int maxHeight = Mathf.RoundToInt(Screen.height * camScale); //If you resize the camera based on "Screen.height" it will be in sync with a regular camera!!
		int aspectMaxWidth = (maxHeight * width) / height;
		orthographicSize = baseOrthSize * camScale;

		setRenUISize(aspectMaxWidth, maxHeight);
		renderHD();

		width = baseWidth;
		height = baseHeight;
	}



	public void moveScreen(float x, float y) { //Move the virual screen inside of the window.
		//float screenDiffScale = 1; //Screen.height / Screen.width; //0.0274f;
		
		Vector3 canvasCenter = new Vector3(renUI.canvas.pixelRect.width/2, renUI.canvas.pixelRect.height/2, 0); //First find the center in screen space of the canvas.
		Vector3 fixedScreenPoint = canvasCenter + new Vector3(x, y, 0); //Then add the translate distance to the center point. This is needed because the "RawImage" UI GameObject starts with a position of (0,0) instead of proper screen space center point. In other words, we needed to transform from Canvas space to screen space.
		Vector3 worldSpaceScale = sceneViewCamera.ScreenToWorldPoint(fixedScreenPoint); //Finally, take the true screen point position and convert it to world space.

		renUI.rectTransform.Translate(x, y, 0); //Use the regular canvas space coordinates for the UI Objects.
		RenderCamera.transform.Translate(worldSpaceScale.x, worldSpaceScale.y, 0); //Use the correct world space coordinates to move the cameras in world space!
		WorldUIRenUI.rectTransform.Translate(x, y, 0);
		WorldUICamera.transform.Translate(worldSpaceScale.x, worldSpaceScale.y, 0);
	}

	public void centerScreen() { //Move the virual screen inside of the window.
		Vector3 centerPos = new Vector3 (0, 0, sceneViewCamera.transform.position.z);
		renUI.rectTransform.localPosition = centerPos;
		RenderCamera.transform.localPosition = centerPos;
		WorldUIRenUI.rectTransform.localPosition = centerPos;
		WorldUICamera.transform.localPosition = centerPos;
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
