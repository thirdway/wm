using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrailDifficultyDropDownCOmp : MonoBehaviour {

	public Dropdown dropDownComp;
	public LevelManager levelManager;

	void Start () {
		dropDownComp = GetComponent<Dropdown> ();
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
	}
	
	public void OnDropDownChanged()
	{
		levelManager.currentTrailDifficultyLevel = dropDownComp.value;
	}
}
