using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionMarker : MonoBehaviour {

	public LevelManager levelManager;
	public float checkTime = 0.25f;
	public List<GameObject> collidingObjects;

	public Material colMat;
	public Material noColMat;


	void Awake() {
		collidingObjects = new List<GameObject> ();
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
	}
	
	void Update () {
		if (Time.time >= checkTime) {
			checkTime = Time.time + 0.25f;
			CheckCollisions ();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (!collidingObjects.Contains (other.transform.gameObject)) {
			if (other.transform.gameObject.tag == "Building") {
				collidingObjects.Add (other.transform.gameObject);
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (collidingObjects.Contains (other.transform.gameObject)) {
			collidingObjects.Remove (other.transform.gameObject);
		}
	}

	public void CheckCollisions()
	{
		if (collidingObjects.Count > 0) {
			GetComponent<MeshRenderer> ().material = colMat;
			levelManager.canBuild = false;
		}
		else if (collidingObjects.Count <= 0) {
			GetComponent<MeshRenderer> ().material = noColMat;
			levelManager.canBuild = true;
		}
	}
}
