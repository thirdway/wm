using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftPylonComp : MonoBehaviour {


	public GameObject cableAnchorLeft;
	public GameObject cableAnchorRight;

	public GameObject cableTargetLeft;
	public GameObject cableTargetRight;

	public GameObject cableLeft;
	public GameObject cableRight;

	void Start () {
		
	}
	
	public void DrawCables() {

		cableLeft.transform.LookAt (cableTargetLeft.transform.position);
		cableRight.transform.LookAt (cableTargetRight.transform.position);

		float leftDist = Vector3.Distance(cableAnchorLeft.transform.position, cableTargetLeft.transform.position);
		float rightDist = Vector3.Distance(cableAnchorRight.transform.position, cableTargetRight.transform.position);

		Vector3 leftScale = new Vector3(cableLeft.transform.localScale.x, cableLeft.transform.localScale.y, leftDist);
		Vector3 rightScale = new Vector3(cableRight.transform.localScale.x, cableRight.transform.localScale.y, rightDist);

		cableLeft.transform.localScale = leftScale;
		cableRight.transform.localScale = rightScale;

	}
}
