using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
public class LeaderBoard : MonoBehaviour {
	public Text[] highScores; 
	public int [] highScoreValues; 
	string [] highScoreNames; 
	// public static bool newRecord = true; 

	// Use this for initialization 
	void Start () {  // Array highScoreValues sized as neeeded 

	//	Debug.Log("newRecord " + newRecord);

		highScoreValues = new int [highScores.Length]; 
		highScoreNames = new string [highScores.Length]; 

		LoadScores (); 
	/*	for (int i = 0; i < highScores.Length; i++) {
			highScoreValues [i] = PlayerPrefs.GetInt ("highScoreValues" + i); 
			highScoreNames [i] = PlayerPrefs.GetString ("highScoreNames" + i); 
		}
*/
}

	public void LoadScores() {
 	 	for (int i = 0; i < highScores.Length; i++) {
			highScoreValues [i] = PlayerPrefs.GetInt ("highScoreValues" + i); 
			highScoreNames [i] = PlayerPrefs.GetString ("highScoreNames" + i); 
		}
	}



	public void SaveScores() {
		for (int i = 0; i < highScores.Length; i++) {
			PlayerPrefs.SetInt ("highScoreValues" + i, highScoreValues [i]);  
			PlayerPrefs.SetString ("highScoreNames" + i, highScoreNames [i]); 
		}
	}

	public void CheckForHighScores(int _value, string _userName) {
	
		for (int i = 0; i < highScores.Length; i++) {	
			if (_value > highScoreValues [i]) {

				for (int a = highScores.Length - 1; a > i; a--) {
					highScoreValues [a] = highScoreValues [a - 1];
					highScoreNames [a] = highScoreNames [a - 1]; 
				}
				highScoreValues [i] = _value; 
				highScoreNames [i] = _userName; 
		
				SaveScores ();
				 
				break;
			}
		}

	}


	public void ResetScores() {

		for (int i = 0; i < highScores.Length; i++) {	


				for (int a = highScores.Length - 1; a > i; a--) {

				highScoreValues [i] = 0; 
				highScoreNames [i] = ""; 

				SaveScores ();

				break;
			}
		}

	}



	public bool CheckRecord (int _value) {
		bool record = false; 	

		for (int i = 0; i < highScores.Length; i++) {	
			if (_value > highScoreValues [i]) {
				record = true; 
			}

		
	}
		return record;
	}



	public void DrawScores () {
		for (int i = 0; i < highScores.Length; i++) {

			highScores [i]. text = highScoreValues [i].ToString () + " : " + highScoreNames [i]; 
		}
	}


	public void HideScores(){
		for (int i = 0; i < highScores.Length; i++) {

			highScores [i].text = " "; 
		}
	}



	// Update is called once per frame
	void Update () {
		
	}
}
