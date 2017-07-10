using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeDestroyer : MonoBehaviour {

	public LevelManager levelManager;


	void Start () {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		StartCoroutine (AutoDestroy ());
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.gameObject.tag == "Tree") {
			levelManager.treesToRemoveList.Add (other.gameObject);
		}
	}

	public IEnumerator AutoDestroy()
	{
		yield return new WaitForSeconds (0.5f);
		DestroyObject (gameObject);
	}
}
