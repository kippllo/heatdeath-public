using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class newsTicker : MonoBehaviour {

	// 1. Make sure the UI text is text-align left!
	// 2. Make sure the UI text has "Auto Size" turned off.
	// 3. Make sure the UI text anchors are Center & middle! I.e. {min: (0.5, 0.5), max: (0.5, 0.5), Pivot: (0, 0)}
	// Note: This script should work even if the canvas is not in screen space. For example it should work on an in-game TV!

	public TMP_Text textUI;
	public float scrollSpeed = 0.5f;
	
	[Tooltip("The amount the ticker will be offset from the left and right side of the screen. (Hint: this can be used to pause in between message loops.)")]
	public float edgeOffset = 10;
	
	[Tooltip("The amount the ticker will be down from the top of the screen! Should be a percent between 0-1.")]
	public float yOffset = 0.1f;
	
	private Vector3 startPos;
	private Vector3 endPos;
	private float startX;
	private float endX;

	private float scrollCounter = 0;
	private float percentScale; //This will allow all string lengths to move at the same speed!
	private bool initDone = false;


	void Start () {
		reset();
	}
	

	void Update () {
		if(initDone) { //Only run the update code if the async start has finished!
			scrollCounter += scrollSpeed * Time.deltaTime * percentScale; //The Lerp based on a counter so the movement speed will be constant through the whole animation.

			Vector3 movePos = Vector3.Lerp(startPos, endPos, scrollCounter); //Lerp based on the counter.
			textUI.rectTransform.localPosition = movePos;

			if(scrollCounter >= 1){
				scrollCounter = 0; //Reset the ticker to player again on a loop!
			}
		}

		//Note: Screen resolutions changes are handled in the "TitleCtrl.cs" script. They are not handled in this script because you might want this ticker on an in-game TV screen. That screen will not need to update when the game's window is resized.
	}


	//Be sure to call this after changing the string text to make sure all the dimensions are correct!
	public async void reset() {
		try{
			//Reset vars the default.
			initDone = false;
			scrollCounter = 0;
			
			Rect canvasRect = textUI.canvas.pixelRect; //Cache a reference to the canvas' rect.
			textUI.rectTransform.localPosition = new Vector3(canvasRect.width*2, canvasRect.height*2, 100); //Just move the text WAY off screen to start! This is just so the users does not see it in the first frame!
			
			//To reset the TMP Text Container to the new size, turn off "autoSizeTextContainer" and then turn it back on.
			textUI.autoSizeTextContainer = false;
			await Task.Delay(100);
			textUI.autoSizeTextContainer = true; //This will resize the TMP container to perfectly match the text!
			await Task.Delay(100); //Wait for the "autoSizeTextContainer" to change the correct properties to match the text!
			//Then, set the text height to zero so it won't mess with the vertical placement!
			textUI.rectTransform.sizeDelta = new Vector2(textUI.rectTransform.rect.width, 0);

			//Grab the scaled dimensions of the canvas (the canvas is scaled by unity).
			float scaledCanvasHeight = canvasRect.height/textUI.canvas.scaleFactor;
			float scaledCanvasWidth = canvasRect.width/textUI.canvas.scaleFactor;

			//Find the start and end X positions.
			startX = scaledCanvasWidth + edgeOffset;
			endX = -textUI.rectTransform.rect.width -edgeOffset;

			Vector3 screenOffset = new Vector3(0, scaledCanvasHeight *yOffset, 0); //Calculate the Y-offset. By converting the y-offset into a screen space vector.
			Vector3 canvasCenter = new Vector3(scaledCanvasWidth/2, scaledCanvasHeight/2, 0); //Calculate the offset the needed because this element is centered on the canvas instead of being on the bottom-left corner of the screen.
			Vector3 zeroedPos = Vector3.zero - canvasCenter + screenOffset; //This is the correct starting Y-position of ticker text.

			startPos = new Vector3(startX, 0, 0) +zeroedPos; //Add in the X-offset to finally find the UI start and end position.
			endPos = new Vector3(endX, 0, 0) +zeroedPos;

			textUI.rectTransform.localPosition = startPos; //Set the UI position to the start vector.

			percentScale = 1/(startX - endX); //This scale is based on the distance from the start point to the end point. It will make sure the lerp is the same for really long text and super short text! Although this method makes the lerp speed vary slightly with different window sizes!
			initDone = true; //Toggle the async done flag.
		} catch(MissingReferenceException UIDeletedErr){
			//This is because the scene changed before the async method completed.
			//For now don't worry about it, but later make a cancellation token!
			Debug.Log(UIDeletedErr.ToString());
		}
	}
}


//Tip: On unity canvas UI the UIElement.rectTransform.localPosition will only be accurate if the UIElement is centered on the canvas!