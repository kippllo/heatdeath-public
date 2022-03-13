using UnityEngine;
using TMPro;


//Note: The below script copies the old material settings into a new material object.
//		It does this to avoid a visual error caused by the static properties of unity material prefabs.

public class FontAni : MonoBehaviour {

	public float underlayOffsetX;
	public float underlayOffsetY;
	public float underlayDilate;

	private bool matInit = false;
	private Material aniMat;

	//void Start () {
	//}
	

	void Update () {
		Material currMat = gameObject.GetComponent<CanvasRenderer>().GetMaterial() ?? new Material(new Material(Shader.Find("TextMeshPro/Distance Field"))); //Use null coalescing. Grab the current render material.
		if(currMat == gameObject.GetComponent<CanvasRenderer>().GetMaterial() && !matInit) { //If the gameobject's material is null it will have been coalesced to a new object. So, if they are both the same object that means TMP has been initialized. "&& !matInit" is a flag to make sure this code only runs once!

			Material newMat = new Material(Shader.Find("TextMeshPro/Distance Field")); //Create a copy of the current material, this is so we can modify the copy while leaving the original unchanged.
			newMat.CopyPropertiesFromMaterial( gameObject.GetComponent<CanvasRenderer>().GetMaterial() );

			gameObject.GetComponent<TextMeshProUGUI>().fontMaterial = newMat; //The the gameObject's TMP material to the new copy.
			
			aniMat = gameObject.GetComponent<TextMeshProUGUI>().fontMaterial; //Cache a reference to the copy.
			matInit = true; //Toggle the "init" flag.
		}
		
		if(matInit){
			aniMat.SetFloat("_UnderlayOffsetX", underlayOffsetX); //These values are changed by an Animator!
			aniMat.SetFloat("_UnderlayOffsetY", underlayOffsetY);
			aniMat.SetFloat("_UnderlayDilate", underlayDilate);
		}
	}
}