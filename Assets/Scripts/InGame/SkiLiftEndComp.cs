using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftEndComp : MonoBehaviour {


	public GameObject leftAnchor;
	public GameObject rightAnchor;
	public GameObject leftTarget;
	public GameObject rightTarget;
	public GameObject leftCable;
	public GameObject rightCable;
	public GameObject skiLiftPapa;
	public GameObject unloadPos;
	public LevelManager levelManager;
	public float overlapSphereRadius = 100.0f;


	void Start () {
		skiLiftPapa = transform.parent.transform.gameObject;
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
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

	public void DrawCables()
	{
		leftCable.transform.LookAt (leftTarget.transform.position);
		rightCable.transform.LookAt (rightTarget.transform.position);

		float leftDist = Vector3.Distance (leftCable.transform.position, leftTarget.transform.position);
		float rightDist = Vector3.Distance (rightCable.transform.position, rightTarget.transform.position);

		Vector3 updatedLeftScale = new Vector3 (leftCable.transform.localScale.x, leftCable.transform.localScale.y, (leftDist * 0.2f));
		Vector3 updatedRightScale = new Vector3 (rightCable.transform.localScale.x, rightCable.transform.localScale.y, (rightDist * 0.2f));

		leftCable.transform.localScale = updatedLeftScale;
		rightCable.transform.localScale = updatedRightScale;

	}
}
