using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextoComp : MonoBehaviour {

	public int textType;
	public TextsManager textsManager;
	public Text textComp;

	void Start () {
		textsManager = GameObject.Find ("TextsManager").GetComponent<TextsManager> ();
		textComp = GetComponent<Text> ();
		SetText ();
	}
	
	public void SetText() {
		if (textType == 0) {textComp.text = textsManager.trailConfirmationTitleText;}
		else if (textType == 1) {textComp.text = textsManager.yesText;}
		else if (textType == 2) {textComp.text = textsManager.noText;}
		else if (textType == 3) {textComp.text = textsManager.trailNameText;}
		else if (textType == 4) {textComp.text = textsManager.trailDifficultyText;}

	}
}
