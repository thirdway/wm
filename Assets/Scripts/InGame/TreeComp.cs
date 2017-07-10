using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeComp : MonoBehaviour {

	public bool isSelected;
	RaycastHit hit;
	public float treeHeight;
	public LevelManager levelManager;


	void Start()
	{
		
		if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
			if (hit.transform.gameObject.tag != "Tree") {
				float wantedYPos = hit.point.y + (transform.localScale.y / 2);
				Vector3 wantedPos = new Vector3 (transform.position.x, wantedYPos, transform.position.z);
				transform.position = wantedPos;
				gameObject.isStatic = true;
				levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
				levelManager.totalTreesList.Add (gameObject);
			} else if (hit.transform.gameObject.tag == "F") {
				DestroyObject (gameObject);
			}
		} else {
//			Debug.Log ("No floor");
			DestroyObject (gameObject);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.transform.gameObject.tag == "TrailCollider")
		{
			isSelected = true;
			Debug.Log("sel");
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.transform.gameObject.tag == "TrailCollider")
		{
			isSelected = false;
		}
	}
}
