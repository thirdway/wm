using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailMarkerColliderPrefab : MonoBehaviour {

	public LevelManager levelManager;


	void Awake () {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		levelManager.currentColliders.Add (gameObject);
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.transform.gameObject.tag == "Tree") {
			levelManager.AddTreeToList (other.transform.gameObject);
		} else if (other.gameObject.tag == "Building") {
			levelManager.trailIsOnObstacle += 1;
		}
	}
}
