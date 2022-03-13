using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonSetter : MonoBehaviour {

	public string prompt;
	public string[] settingsOptions;
	public TMP_Text buttonTextUI;
	private int currInd;


	public string value {
		get{
			return settingsOptions[currInd];
		}
		set {
			List<string> L = new List<string>(settingsOptions); //Array to list help: https://stackoverflow.com/questions/1603170/conversion-of-system-array-to-list
			setOption( L.IndexOf(value) );
		}
	}



	//Don't set a default value here! Set this in settings control script to the correct saved value!

	public void setOption(int ind){
		if(ind < 0 || ind >= settingsOptions.Length){ return; } //Don't go to an index that is out of range!

		currInd = ind;
		buttonTextUI.text = prompt + settingsOptions[ind];
	}

	public void toggleValue(){
		currInd++;
		currInd = (currInd >= settingsOptions.Length || currInd < 0) ? 0 : currInd;
		setOption(currInd);
	}

	

}
