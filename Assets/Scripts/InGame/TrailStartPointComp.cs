using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailStartPointComp : MonoBehaviour {

	public GameObject[] panelPieces;
	public LevelManager levelManager;
	public GameObject trailLineMarker;

	void Start () {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		levelManager.AddPointToList (gameObject);
		levelManager.trailLineRendPosList.Add (trailLineMarker);
	}

	public void OnMouseDown()
	{
		transform.parent.gameObject.GetComponent<TrailComp> ().ShowMenu ();
	}

	public void OnMouseUp()
	{
		transform.parent.gameObject.GetComponent<TrailComp> ().HideMenu ();
	}
}
