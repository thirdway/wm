using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTreePrefab : MonoBehaviour {

	public List<GameObject> trees;
	RaycastHit hit;
	public float treeHeight;
	public LevelManager levelManager;


	void Start () {
		foreach (GameObject t in trees) {
			if (Physics.Raycast (t.transform.position, -Vector3.up, out hit)) {
				if (hit.transform.gameObject.tag != "Tree") {
						float wantedYPos = hit.point.y + (t.transform.localScale.y / 2);
					Vector3 wantedPos = new Vector3 (t.transform.position.x, wantedYPos, t.transform.position.z);
					transform.position = wantedPos;
					gameObject.isStatic = true;
					levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
					levelManager.totalTreesList.Add (gameObject);
				} else if (hit.transform.gameObject.tag == "Tree") {
					DestroyObject (t);
				}
			} else {
				Debug.Log ("No floor" + t.name);
				DestroyObject (t);
			}
		}
	}

}
