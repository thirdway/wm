using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkierSpawnerComp : MonoBehaviour {

	public LevelManager levelManager;

	void Start () {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager>();
		SetPos ();
	}
	
	public void SetPos()
	{
		RaycastHit hit;

		if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
			Debug.DrawLine (transform.position, hit.point, Color.green);
			float wantedY = hit.point.y + 3.0f;
			Vector3 wantedPos = new Vector3 (transform.position.x, wantedY, transform.position.z);
			transform.position = wantedPos;
		}

	}
}
