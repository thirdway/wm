using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameHudButtons : MonoBehaviour {

	public int buttonType;
	public LevelManager levelManager;

	void Awake()
	{
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();

	}

	void Start () {
		
	}

	public void OnButtonDown()
	{
		if (buttonType == 0) {
			if (levelManager.isCreatingTrail == false) {
				levelManager.StartCoroutine (levelManager.SetTrailCreationModeOn ());
			} else if (levelManager.isCreatingTrail == true) {
				levelManager.SetTrailCreationModeOff ();
			}
		}
		else if (buttonType == 1) {
			levelManager.StartCoroutine (levelManager.CreateTrail ());
		}
		else if (buttonType == 2) {
			levelManager.CancelThisTrail ();
		}
		else if (buttonType == 3) {
			levelManager.StartCoroutine (levelManager.SetSkiLiftCreationModeOn ());
		}
		else if (buttonType == 4) {
			levelManager.StartCoroutine (levelManager.CreateSkiLift ());
		}
		else if (buttonType == 5) {
			levelManager.CancelThisSkiLift ();
		}
		else if (buttonType == 6) {
			levelManager.RestartLevel ();
		}


	}
}
