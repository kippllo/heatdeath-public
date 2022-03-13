/*
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json; //dedbug remove later!
*/

using UnityEngine;


public class ZoneCtrl : MonoBehaviour {

	public Color32 SafeColor = new Color32(255,255,255,255);
	public Color32 DangerColor = new Color32(255,255,255,255);
	
	DrawMesh canvas;
	GameObject safeZoneGameObject;
	GameObject dangerZoneGameObject;

	
	void Start () {
		canvas = new DrawMesh();
		safeZoneGameObject = new GameObject(); //Set to a default GameObject.
		dangerZoneGameObject = new GameObject();
		canvas.segmentCount = 30;
	}
	

	void Update () {
		if(TempRoundData.serverHasSetSafeZone){ //The memory leak is here.
			Destroy(dangerZoneGameObject);

			//Draw the danger zone every frame, lerping it closer based on the nextZoneTimer.
			canvas.clear();
			float lerpAmount = TempRoundData.zoneTime/TempRoundData.maxZoneTime;
			TempRoundData.dangerRadius = Mathf.Lerp(TempRoundData.NextSafeDist, TempRoundData.safeDist, lerpAmount);
			TempRoundData.dangerCenter = Vector3.Lerp(TempRoundData.NextSafeCenter.ToVector3(), TempRoundData.safeCenter.ToVector3(), lerpAmount);
			canvas.DrawSquareWithCircleHole(Vector3.zero, TempRoundData.map.width, TempRoundData.map.height, TempRoundData.dangerCenter, TempRoundData.dangerRadius, DangerColor);
			dangerZoneGameObject = canvas.RenderToGameObject();
			dangerZoneGameObject.layer = 10; //Set the layer to render on both the mini map and normal player camera.


			//Only redraw the safeZone outline if the zone has changed! Don't draw it every frame!
			if(lerpAmount >= 0.75f){ //Might have to make this slightly lower than 1, maybe 0.9 or 0.75! Note: it was ">= 1", but I did end up having to make it smaller or it wouldn't draw the saftey circle at all!
				Destroy(safeZoneGameObject);
				canvas.clear();
				//Draw the next saftey circle only once per timer reset.
				float outlineThickness = TempRoundData.NextSafeDist * -0.025f; //Outline thickness is 5% of the outline circle radius. Negative os that the outline thickness goes towards the inside of the cirlce instead of expanding the circle outward...
				outlineThickness = Mathf.Clamp(outlineThickness, -8, -4);
				
				canvas.DrawCircleOutlineInBox(Vector3.zero, TempRoundData.map.width, TempRoundData.map.height, TempRoundData.NextSafeCenter.ToVector3(), TempRoundData.NextSafeDist, outlineThickness, SafeColor);
				
				safeZoneGameObject = canvas.RenderToGameObject(); //render the safe zone to a different game Object than the dangerZone so that the danger zone can be shown in the game world.
				safeZoneGameObject.layer = 12; //Set the layer to only render on the mini map.
			}
			
		}
	}
}
