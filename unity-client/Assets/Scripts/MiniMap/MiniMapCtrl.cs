using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using Keybored.Mobile;

public class MiniMapCtrl : MonoBehaviour {

	public lowResMiniMap lowResCamScrpt;
	public Canvas UICanvas;
	public Image mapOutline;
	public static float orthographicZoom;
	private float zoomMin = 15;
	public float zoomMax = 100;
	public int squareSize = 100; //X and Y map size in pixels!
	private int edgeOffset = 10; //This is used to set both the screen-edge offset of the minimap and the outline thickness.
	
	private Vector2 screenRes;
	public static bool mapFull;

	TouchTap TouchMap;
	PinchZoom TouchZoomMap;
	TouchDrag TouchDrag;
	TouchTap TouchExitMapLeft;
	TouchTap TouchExitMapRight;

	public TouchJoystick TouchMove;
	public TouchJoystick TouchAim;

	//	Note: Don't forget to set the minimap's canvas to render on top of the other canvases!
	//		Do this with: "Canvas.SortOrder"

	async void Start () {
		MobileStart();

		// For whatever reason unity is throwing in error if I wait here for more than "0" or one tick (I think 0 = one tick). So only wait that time!
		await Task.Delay(10); //Wait till the lowResCam start function is complete!
		screenRes =  new Vector2(Screen.width, Screen.height);
		mapFull = false;
		orthographicZoom = 15;

		resetMiniMap();
	}

	void MobileStart() {
		#if UNITY_IOS || UNITY_ANDROID
			//Grab the mobile controls...
			TouchMap = transform.Find("Canvas/TouchCtrls/TouchMap").gameObject.GetComponent<TouchTap>(); //Note: Ypu could just do "transform.Find("...").GetComponent<...>()" but I'm leaving the ".gameObject" as an example for how to access the gameObject by "transform.Find".
			TouchZoomMap = transform.Find("Canvas/TouchCtrls/TouchZoomMap").gameObject.GetComponent<PinchZoom>();
			TouchDrag = transform.Find("Canvas/TouchCtrls/TouchDrag").gameObject.GetComponent<TouchDrag>();
			TouchExitMapLeft = transform.Find("Canvas/TouchCtrls/TouchExitMapLeft").GetComponent<TouchTap>();
			TouchExitMapRight = transform.Find("Canvas/TouchCtrls/TouchExitMapRight").GetComponent<TouchTap>();

			// Setup the map touch exit buttons. These will exit the fullscreen map if the player
			// taps anywhere outside of the map when it is in fullscreen mode.
			Vector2 mapFullscreenSize = new Vector2(Screen.height-100, Screen.height-100); //Is the is size of the map in fullscreen mode.
			Vector2 mapFullscreenStartPos = new Vector2(Screen.width/2, Screen.height/2) - (mapFullscreenSize/2); //The map is centered on the UI Canvas, going half of it width & height in both the positive and negative directions.
			Vector2 mapFullscreenEndPos = new Vector2(Screen.width/2, Screen.height/2) + (mapFullscreenSize/2); //Half the map size to positive direction will be the ending X position.
			TouchExitMapLeft.startPos = new Vector2(0,0); //The first exit touch detection start at the bottom left corner and goes to start of the left edge of the minimap.
			TouchExitMapLeft.width = (int)mapFullscreenStartPos.x;
			TouchExitMapLeft.height = Screen.height; //It should detect touch all the way to the top of the screen.

			TouchExitMapRight.startPos = new Vector2(mapFullscreenEndPos.x, 0); //The right exit touch zone will start at the end of the minimap.
			TouchExitMapRight.width = Screen.width - (int)mapFullscreenEndPos.x; //It will have a width equal to the leftover amount of screen pixels after the minimap is drawn.
			TouchExitMapRight.height = Screen.height; //The right zone will go to the top of the screen as well.
		#endif
	}
	

	void Update () {
		//The window has changed sizes, reset the map to match!
		if(screenRes.x != Screen.width || screenRes.y != Screen.height) {
			resetMiniMap();
			screenRes =  new Vector2(Screen.width, Screen.height); //Save the new resolution.
		}

		#if !UNITY_IOS && !UNITY_ANDROID
			//Toggle to full screen...
			if(Input.GetButtonDown("Map")){
				mapFull = !mapFull;

				//Fix the camera vars for non-fullscreen mode.
				lowResCamScrpt.RenderCamera.transform.localPosition = new Vector3(0,0,-10); //Center the rendering camera incase the player moved it.
				orthographicZoom = Mathf.Clamp(orthographicZoom, zoomMin, zoomMax); //Limit the zoom!

				setFullMap(mapFull);
			}


			//If the map is in full screen mode, allow the player to interact with it.
			if(mapFull){
				Camera mapRenderCam = lowResCamScrpt.RenderCamera; //Grab the minimap render camera.
				
				//Grab player input to control the zoom.
				float zoomAdd = (Input.GetAxis("MapZoomPS4") != 0) ? Input.GetAxis("MapZoomPS4") : Input.GetAxis("Mouse ScrollWheel")*50; //Be sure to give the low values of the scroll wheel a boost!
				orthographicZoom += zoomAdd; //Zoom the camera! In a non 2D style game you'll probably want to move the camera's z position here!
				orthographicZoom = Mathf.Clamp(orthographicZoom, zoomMin, zoomMax*5); //Limit the zoom both in and out! Out is a bigger limit here then in non-fullscreen mode.
				
				mapRenderCam.orthographicSize = orthographicZoom; //Update the camera's "orthographicSize" to update the zoom live.

				//Move the camera if the player moves:
				Vector3 MoveCam = new Vector3(0, 0, 0);
				float speed = orthographicZoom/3; //Speed is based on zoom!

				if (Input.GetAxis("move_YAxis") > 0 || Input.GetButton("Up")) {
					MoveCam.y += speed;
				}

				if (Input.GetAxis("move_YAxis") < 0 || Input.GetButton("Down")) {
					MoveCam.y -= speed;
				}

				if (Input.GetAxis("move_XAxis") > 0 || Input.GetButton("Right")) {
					MoveCam.x += speed;
				}

				if (Input.GetAxis("move_XAxis") < 0 || Input.GetButton("Left")) {
					MoveCam.x -= speed;
				}

				mapRenderCam.transform.Translate(MoveCam * Time.deltaTime); //Again adjust the camera-gameObject's properties instead of the "lowResCamScrpt".
			}
		#else
			bool openMapFullscreen = TouchMap.GetTapDown && !mapFull;
			bool exitMapTouched = (TouchExitMapLeft.GetTapDown || TouchExitMapRight.GetTapDown) && mapFull;
			if(openMapFullscreen || exitMapTouched){
				mapFull = !mapFull;
				lowResCamScrpt.RenderCamera.transform.localPosition = new Vector3(0,0,-10);
				orthographicZoom = Mathf.Clamp(orthographicZoom, zoomMin, zoomMax);
				setFullMap(mapFull);
			}

			if(mapFull){
				Camera mapRenderCam = lowResCamScrpt.RenderCamera;

				//Grab player input to control the zoom.
				float zoomAdd = -TouchZoomMap.GetZoom / TouchLib.ScreenScale; // Reduce the zoom by the screen scale because it is fast on phones with large resolutions.
				orthographicZoom += zoomAdd;
				orthographicZoom = Mathf.Clamp(orthographicZoom, zoomMin, zoomMax*5);
				mapRenderCam.orthographicSize = orthographicZoom;

				//Move the camera if the player drags...
				Vector3 MoveCam = new Vector3(0, 0, 0);
				float speed = orthographicZoom/5f; //Speed is based on zoom! Reduced the speed by 5 so it will match where the player's finger is on touch.
				MoveCam = -TouchDrag.GetDrag * speed; //Grab the finger drag amount from the touch control.
				MoveCam /= TouchLib.ScreenScaleVector; //Reduce speed on bigger screens because the finger drag distance will be larger.

				mapRenderCam.transform.Translate(MoveCam * Time.deltaTime);
			}
		#endif
	}

	
	//Call this to show a larger version of the map!
	async void setFullMap(bool fullSize){
		try{
			//Update touch UI controls...
			#if UNITY_IOS || UNITY_ANDROID
				TouchZoomMap.fullscreen = fullSize; //If the map is in full screen, the player can darag anywhere on screen to move it around.

				//Disable the touch joysticks when the map is in fullscreen mode...
				TouchMove.gameObject.SetActive(!fullSize);
				TouchAim.gameObject.SetActive(!fullSize);
			#endif

			int oldSize = lowResCamScrpt.height;
			int newSize = fullSize ? Screen.height-100 : squareSize; //Shrink or grow the map depending. Keep a offset of 100 pixels from the edge of the screen.

			#if UNITY_IOS || UNITY_ANDROID
				int squareSizeScaled = Mathf.RoundToInt(squareSize*TouchLib.ScreenScale*ClientSettings.uiScale);
				newSize = fullSize ? Screen.height-100 : squareSizeScaled;
			#endif

			lowResCamScrpt.orthographicSize = orthographicZoom;

			if(fullSize){
				//First center the map UI on the screen!
				lowResCamScrpt.centerScreen();
				mapOutline.rectTransform.localPosition = new Vector3 (0, 0, 0);
			} else {
				resetMiniMap();
			}

			int lerpmax = 30;
			for(int i=1; i<=lerpmax; i++){
				int sizeLerp = Mathf.RoundToInt( Mathf.Lerp(oldSize, newSize, (float)i/(float)lerpmax) );
				lowResCamScrpt.pixelPerfectScreen(sizeLerp, sizeLerp, sizeLerp, sizeLerp);
				
				mapOutline.rectTransform.sizeDelta = new Vector2(sizeLerp+edgeOffset, sizeLerp+edgeOffset);
				await Task.Delay(1); //Wait a millisecond to make this a smooth animation.
			}
		} catch(MissingReferenceException UIDeletedErr){
			//This is because the scene changed before the async method completed.
			//For now don't worry about it, but later make a cancellation token!
			Debug.Log(UIDeletedErr.ToString());
		}
	}


	//This sets the mini map based on the current window size!
	void resetMiniMap(){
		//First make sure the map is starting in the center position. That is the center of the canvas, or (0,0) in canvas space.
		lowResCamScrpt.centerScreen();
		lowResCamScrpt.orthographicSize = orthographicZoom;
		
		#if !UNITY_IOS && !UNITY_ANDROID
			lowResCamScrpt.pixelPerfectScreen(squareSize, squareSize, squareSize, squareSize);
			
			Vector3 canvasCenter = new Vector3(UICanvas.pixelRect.width/2, UICanvas.pixelRect.height/2, 0);
			Vector3 UIImgzeroedPos = new Vector3(squareSize/2, -squareSize/2, 0); //The UI Image's position starts in its center, but I want it to start at the button left-hand corner.
			Vector3 zeroedPos = Vector3.zero - canvasCenter + UIImgzeroedPos;
			Vector3 translateAmount = new Vector3(Screen.width-edgeOffset -squareSize, Screen.height-edgeOffset, 0); //I want the mini map to be 10 pixels (or edgeOffset) from the top right hand corner. NOTE: "-squareSize" in x because the origin of the UI Image has be transformed to be the bottom left-hand corner, so have to acount for part of the image being off screen.
			Vector3 moveDist = zeroedPos + translateAmount;
			lowResCamScrpt.moveScreen(moveDist.x, moveDist.y);

			//Move the map outline the same distance! They must be different gameObject, not parent/child, because of UI render order...
			mapOutline.rectTransform.localPosition = new Vector3 (0, 0, 0); //The map outline must also be centered first!
			mapOutline.rectTransform.sizeDelta = new Vector2(squareSize+edgeOffset, squareSize+edgeOffset); //Resize the map outline to be 10 pixels (or edgeOffset) more than the minimap itself.
			mapOutline.rectTransform.Translate(moveDist.x, moveDist.y, 0);
		
		#else //If Android or iOS
			int squareSizeScaled = Mathf.RoundToInt(squareSize*TouchLib.ScreenScale*ClientSettings.uiScale);
			int edgeOffsetScaled = Mathf.RoundToInt(edgeOffset*TouchLib.ScreenScale*ClientSettings.uiScale);
			lowResCamScrpt.pixelPerfectScreen(squareSizeScaled, squareSizeScaled, squareSizeScaled, squareSizeScaled);
			
			Vector3 canvasCenter = new Vector3(UICanvas.pixelRect.width/2, UICanvas.pixelRect.height/2, 0);
			Vector3 UIImgzeroedPos = new Vector3(squareSizeScaled/2, -squareSizeScaled/2, 0);
			Vector3 zeroedPos = Vector3.zero - canvasCenter + UIImgzeroedPos;
			Vector3 translateAmount = new Vector3(Screen.width-edgeOffsetScaled -squareSizeScaled, Screen.height-edgeOffsetScaled, 0);
			Vector3 moveDist = zeroedPos + translateAmount;
			lowResCamScrpt.moveScreen(moveDist.x, moveDist.y);

			mapOutline.rectTransform.localPosition = new Vector3 (0, 0, 0);
			mapOutline.rectTransform.sizeDelta = new Vector2(squareSizeScaled+edgeOffsetScaled, squareSizeScaled+edgeOffsetScaled);
			mapOutline.rectTransform.Translate(moveDist.x, moveDist.y, 0);

			//Reset touch controls
			Vector2 touchZone = new Vector3(Screen.width-edgeOffsetScaled -squareSizeScaled, Screen.height-edgeOffsetScaled -squareSizeScaled, 0);
			TouchMap.startPos = touchZone;
			TouchMap.width = squareSizeScaled;
			TouchMap.height = squareSizeScaled;
		#endif
	}

}