using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextsManager : MonoBehaviour {

	public string trailConfirmationTitleText;
	public string yesText;
	public string noText;
	public string youCantBuildText;
	public string hereText;

	public string trailText;
	public string skiLiftText;

	public string trailNameText;
	public string trailDifficultyText;

	public string[] messageMenuTexts;

	void Start () {
		SetLanguage (DataSaver.selectedLanguage);
	}
	
	public void SetLanguage (int l) {
		if (l == 0) {
			SetLanguageEnglish ();
		}
	}

	public void SetLanguageEnglish()
	{
		trailConfirmationTitleText = "Do you want to create this trail ?";
		yesText = "YES";
		noText = "NO";

		youCantBuildText = "You cant build ";
		hereText = " here.";

		trailText = "a trail";
		skiLiftText = "a Ski Lift";

		trailNameText = "Trail name:";
		trailDifficultyText = "Trail difficulty:";

		messageMenuTexts = new string[10];
		messageMenuTexts[0] = "Draw a trail with left mouse button, and release to finish";
		messageMenuTexts[1] = "Place the ski lift start point with left mouse button";
		messageMenuTexts[2] = "Place the ski lift finish point with left mouse button";


	}
}
