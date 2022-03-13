using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class lowResSquareCtrlTest : MonoBehaviour {

	public GameObject LowResSquarePrefab;

	private float timer = 4;
	private float timerMax = 5.0f;

	//GameObject square;
	//bool flg = false;

	// Use this for initialization
	void Start () {
		//square = Instantiate(LowResSquarePrefab);
		//square.GetComponent<LowResSquare>().resizeScreen( 1.0f );
		//square.GetComponent<LowResSquare>().moveScreen(500,  500);
		//square.GetComponent<LowResSquare>().centerScreen();
	}
	
	// Update is called once per frame
	void Update () {

		
		

		
		timer += Time.deltaTime;
		if(timer >= timerMax){


			/* if(!flg){
				flg = true;
				square.GetComponent<LowResSquare>().resizeScreen( Random.Range(0.1f, 1.0f) );
				square.GetComponent<LowResSquare>().moveScreen(Random.Range(0, 1001),  Random.Range(0, 1001));
				//square.GetComponent<LowResSquare>().centerScreen();
			} */

			timer -= timerMax;
			spawnLowResSquare();
		}
	}


	async void spawnLowResSquare(){ //This is better than a Coroutine/IEnumerator for waiting! Remember "async void" won't cause a caution during build if you don't "await" it.
		GameObject square = Instantiate(LowResSquarePrefab);
		
		await Task.Delay(10); //Wait till the start function is complete!

		LowResSquare squareScrpt = square.GetComponent<LowResSquare>();
		
		squareScrpt.width = Mathf.RoundToInt( 250 *Random.Range(0.1f, 1.0f) );
		squareScrpt.height = Mathf.RoundToInt( 250 *Random.Range(0.1f, 1.0f) );

		squareScrpt.resizeScreen( Random.Range(0.1f, 0.5f) );
		squareScrpt.moveScreen(Random.Range(-750, 750),  Random.Range(-750, 750));

		Destroy(square, 30); //If much more than 8 "LowResSquares" are spawn at the same time, the game will start lagging!
	}
}
