using UnityEngine;

public class InGameResize : MonoBehaviour {

	
	public lowResCam lowResCamScrpt;
	private Vector2 screenRes;
	
	public int resWidth = 250;
	public int resHeight = 250;


	public int camRes{
		get{
			return resWidth;
		}
		set {
			resWidth = value;
			resHeight = value;
			lowResCamScrpt.pixelPerfectmaxHeightScreen(resWidth, resHeight);
		}
	}


	void Start () {
		screenRes =  new Vector2(Screen.width, Screen.height);
	}
	

	void Update () {
		if(screenRes.x != Screen.width || screenRes.y != Screen.height) {
			//The window has changed sizes, reset the map to match!
			lowResCamScrpt.pixelPerfectmaxHeightScreen(resWidth, resHeight);

			//Save the new resolution.
			screenRes =  new Vector2(Screen.width, Screen.height);
		}
	}
}
