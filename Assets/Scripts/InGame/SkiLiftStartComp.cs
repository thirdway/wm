using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftStartComp : MonoBehaviour {

	public GameObject leftAnchor;
	public GameObject rightAnchor;
	public GameObject skiLiftPapa;

	public LevelManager levelManager;
	public float overlapSphereRadius = 10.0f;

	void Start () {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		skiLiftPapa = transform.parent.transform.gameObject;	
		SetOverLapSphere ();
	}

	public void SetOverLapSphere()
	{
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, overlapSphereRadius);
		int i = 0;
		foreach (Collider col in hitColliders) {
			if (col.gameObject.tag == "Tree") {
				levelManager.treesToRemoveList.Add(col.gameObject);
			}
		}
	}

}
