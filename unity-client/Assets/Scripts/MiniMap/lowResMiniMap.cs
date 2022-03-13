using UnityEngine;
using UnityEngine.UI;

public class lowResMiniMap : MonoBehaviour {

	public int width = 100;
	public int height = 100;
	public int HDScale = 10; //This is used to up the quality of the base RenderTexture so the pixels won't appear blurry.
	public int antiAliasing = 4; // Can be: 2, 4, or 8.

	public Camera RenderCamera;
	public RawImage renUI;
    private RenderTexture renText;
	public float orthographicSize;


	void Start () {
		//Generate RenderTexture object instead of pre-making them in the project!
		renText = MakeRendText();

		//Link cameras to the RenderTextures
		RenderCamera.targetTexture = renText;
		
		//Link the UI canvas objects to the RenderTextures.
		renUI.texture = renText; // This breaks anti-aliasing. Trilinear is better for smooth anti-aliasing. //renText.filterMode = FilterMode.Point;

		//Static camera setup...
		RenderCamera.orthographicSize = orthographicSize;

		pixelPerfectmaxHeightScreen(250, 250); //Set a default view!
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
	}

	public void centerScreen() { //Move the virual screen inside of the window.
		renUI.rectTransform.localPosition = new Vector3 (0, 0, 0);
	}


	public void setRenUISize(int x, int y) {
		renText.Release(); //RenderTexture's dimensions can't be changed if they are already created, so Release them before the dimensions are changed and then Create them again.
		renText.width = x;
		renText.height = y;
		renText.Create();
		renUI.SetNativeSize(); //The easiest way to change the dimensions of the RawImage is to change it via NativeSize. So temporarily change the size of the RenderTexture and then revert it...

		//Set the orthographicSize for both cameras.
		RenderCamera.orthographicSize = orthographicSize;
		QualitySettings.antiAliasing = antiAliasing; //Set the "antiAliasing" in the QualitySettings.
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
