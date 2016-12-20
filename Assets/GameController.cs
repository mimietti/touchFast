using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Audio;
public class GameController : MonoBehaviour {

	public Text ClickCounterText; 
	public Text timeRemainingText; 
	public InputField playerName; 
	public Button submit; 
	public Button newG; 
	public Button reset; 
	public AudioClip beeb; 


	int totalClicks = 0; 
	int timeRemaining = 10; 
	bool gameOn = true; 
	bool IsNewRecord = false; 

	// Use this for initialization
	void Start () {
				 
		FirstGame(); 

	}
	IEnumerator OneSecond(){
		while (1 == 1) {
			yield return new WaitForSeconds (1.0f); 
			if (gameOn) {
				timeRemaining--; 
	//		beeb.Play(); 
			}
			timeRemainingText.text = "" + timeRemaining; 
			if (timeRemaining <= 0) {
				timeRemainingText.text = ""; 
				EndGame ();  
				break; 
			}
		}
	}
		
	public void FirstGame() {
	

		gameOn = false; 
		newG.gameObject.SetActive (true);
		submit.gameObject.SetActive (false);
		playerName.gameObject.SetActive (false); 
		ClickCounterText.gameObject.SetActive(false);
		timeRemainingText.gameObject.SetActive (false); 


	//	GetComponent<LeaderBoard> ().DrawScores (); 
	}

	void EndGame() {
		gameOn = false; 
		IsNewRecord = GetComponent<LeaderBoard> ().CheckRecord (totalClicks); 
		Debug.Log("Is new Record " + IsNewRecord);
		if (IsNewRecord) {
			submit.gameObject.SetActive (true);
			playerName.gameObject.SetActive (true); 
		}
		if (!IsNewRecord) {
			newG.gameObject.SetActive (true);
		}

		GetComponent<LeaderBoard> ().DrawScores (); 
	}



	public void InitialsEntered() {
		
		GetComponent<LeaderBoard> ().CheckForHighScores (totalClicks, playerName.text);
		newG.gameObject.SetActive (true);
		submit.gameObject.SetActive (false);
		playerName.gameObject.SetActive (false); 
		GetComponent<LeaderBoard> ().DrawScores (); 
	}

	public void resetScores() {
		GetComponent<LeaderBoard> ().ResetScores (); 
	}


	public void newGame() {
		 
		gameOn = true; 
		timeRemaining = 10;
		totalClicks = 0; 
		StartCoroutine (OneSecond());
		newG.gameObject.SetActive (false);
		submit.gameObject.SetActive (false); 
		playerName.gameObject.SetActive (false); 

		ClickCounterText.gameObject.SetActive(true);
		timeRemainingText.text = "10"; 
		timeRemainingText.gameObject.SetActive (true); 


		GetComponent<LeaderBoard> ().HideScores (); 
	}



	// Update is called once per frame
	void Update () {
		
	//	IsNewRecord = LeaderBoard.newRecord; 


		// no playing before start button
		if (!gameOn) {
			return; 
		}

		ClickCounterText.text = "Score: " + totalClicks; 

		// PC
		if (Input.GetMouseButtonDown (0)) {
			totalClicks++; 

		}

 

		//  This should work with touch devices:

	/*
		RaycastHit hit = new RaycastHit();
		for (int i = 0; i < Input.touchCount; ++i)
			if (Input.GetTouch(i).phase.Equals(TouchPhase.Began)) {
				// Construct a ray from the current touch coordinates
				totalClicks++; 
				Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
				if (Physics.Raycast(ray, out hit))
					hit.transform.gameObject.SendMessage("OnMouseDown");
			}
*/		


	}



}
