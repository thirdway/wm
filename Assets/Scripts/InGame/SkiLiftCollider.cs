using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftCollider : MonoBehaviour {

	public LevelManager levelManager;
	public float checkTime = 0.25f;
	public List<GameObject> collidingObjects;

	public Material colMat;
	public Material noColMat;

	public GameObject[] skiLiftPylonSpawners;
	public GameObject SkiLiftPylonPrefab;

	public GameObject leftAnchor;
	public GameObject rightAnchor;


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

	public IEnumerator SpawnSkiLiftPylons() {

		leftAnchor = levelManager.currentSkiLift.GetComponent<SkiLiftComp> ().skiLiftStart.GetComponent<SkiLiftStartComp> ().leftAnchor;
		rightAnchor = levelManager.currentSkiLift.GetComponent<SkiLiftComp> ().skiLiftStart.GetComponent<SkiLiftStartComp> ().rightAnchor;


		yield return new WaitForSeconds (0.01f);
		List<GameObject> curSpawners = new List<GameObject>();

		float dist = Vector3.Distance(levelManager.currentSkiLift.GetComponent<SkiLiftComp>().skiLiftStart.transform.position,levelManager.currentSkiLift.GetComponent<SkiLiftComp>().skiLiftEnd.transform.position );
//		Debug.Log ("" + dist);

		if (dist < 100) {
			curSpawners.Add (skiLiftPylonSpawners [4]);
		} else if (dist > 100 && dist < 200) {
			curSpawners.Add (skiLiftPylonSpawners [2]);
			curSpawners.Add (skiLiftPylonSpawners [5]);
			curSpawners.Add (skiLiftPylonSpawners [7]);

		} else if (dist > 200 && dist < 300) {
			curSpawners.Add (skiLiftPylonSpawners [1]);
			curSpawners.Add (skiLiftPylonSpawners [3]);
			curSpawners.Add (skiLiftPylonSpawners [5]);
			curSpawners.Add (skiLiftPylonSpawners [7]);

		} else if (dist > 300) {
			foreach (GameObject go in skiLiftPylonSpawners) {
				curSpawners.Add (go);
			}
		}
		yield return new WaitForSeconds (0.1f);

		GetComponent<BoxCollider> ().enabled = false;
		int i = 0;
	//	foreach (GameObject go in skiLiftPylonSpawners) {
		foreach(GameObject go in curSpawners){
			
			RaycastHit hit;

			if(Physics.Raycast(go.transform.position, -Vector3.up, out hit))
			{
				Debug.DrawLine (transform.position, hit.point, Color.red);
				Vector3 updatedPos = new Vector3 (hit.point.x, hit.point.y , hit.point.z);

				GameObject newPylon = Instantiate (SkiLiftPylonPrefab, updatedPos, transform.rotation);
				newPylon.transform.parent = levelManager.currentSkiLift.transform;
				float yRot = transform.parent.transform.eulerAngles.y;
				newPylon.transform.eulerAngles = new Vector3 (0, yRot, 0);
				newPylon.GetComponent<SkiLiftPylonComp> ().cableTargetLeft = leftAnchor;
				newPylon.GetComponent<SkiLiftPylonComp> ().cableTargetRight = rightAnchor;


				newPylon.GetComponent<SkiLiftPylonComp> ().DrawCables ();

				leftAnchor = newPylon.GetComponent<SkiLiftPylonComp> ().cableAnchorLeft;
				rightAnchor = newPylon.GetComponent<SkiLiftPylonComp> ().cableAnchorRight;
				levelManager.currentSkiLift.GetComponent<SkiLiftComp>().pylonsList.Add (newPylon);

				if (i >= curSpawners.Count - 1) {
					levelManager.currentSkiLift.GetComponent<SkiLiftComp> ().skiLiftEnd.GetComponent<SkiLiftEndComp> ().leftTarget = rightAnchor;
					levelManager.currentSkiLift.GetComponent<SkiLiftComp> ().skiLiftEnd.GetComponent<SkiLiftEndComp> ().rightTarget = leftAnchor;
					levelManager.currentSkiLift.GetComponent<SkiLiftComp> ().skiLiftEnd.GetComponent<SkiLiftEndComp> ().DrawCables ();
				}
			}

			i += 1;
		}
		yield return new WaitForSeconds (0.1f);

		GetComponent<BoxCollider> ().enabled = true;

		//levelManager.StartCoroutine(levelManager.SortCurrentPylonsList ());
		levelManager.currentSkiLift.GetComponent<SkiLiftComp>().StartToSortWayPoints();
	}


}
