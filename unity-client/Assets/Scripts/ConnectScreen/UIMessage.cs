using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMessage : MonoBehaviour {

	public TMP_Text txtMessage;

	void Start () {
		txtMessage.text = serverCtrl.serverMessage;
		

		serverCtrl.deleteReconnectFile(); //If the player made it this far, there is no game to reconnect to.
	}

	public void backToMenu(){
		SceneManager.LoadScene(0);
	}
}
