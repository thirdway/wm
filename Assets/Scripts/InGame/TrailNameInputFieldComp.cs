using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrailNameInputFieldComp : MonoBehaviour {

	public InputField fieldComp;
	public LevelManager levelManager;

	void Awake() {
		fieldComp = GetComponent<InputField> ();
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();

		fieldComp.text = levelManager.currentTrailName;
	}

	public void OnFinishEditingName()
	{
		levelManager.currentTrailName = fieldComp.text;
	}

	void OnEnable()
	{
		fieldComp.text = levelManager.currentTrailName;
	}
}
