using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftLinePosMarker : MonoBehaviour {

	public float wantedY;
	RaycastHit hit;
	public float rayTime;
	public float rayRate;
	public float minRayRate = 0.08f;
	public float maxRayRate = 0.12f;

	void Start () {
		rayRate = Random.Range (minRayRate, maxRayRate);

	}
	
	void Update () {
		if (Time.time >= rayTime) {
			rayTime = Time.time + rayRate;

			if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
				Debug.DrawLine (transform.position, hit.point, Color.blue);
				transform.position = new Vector3 (transform.position.x, hit.point.y + wantedY, transform.position.y);
			}
		}
	}
}
