using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailPointMarker : MonoBehaviour {

	public int markerNumber;
	public LevelManager levelManager;
	public GameObject target;
	public GameObject spawner;
	public GameObject[] borderSpawners;
	public GameObject borderPrefab;
	public float wantedZ;
	public GameObject trailWayPointSpawner;
	public GameObject trailWayPointPrefab;
	public GameObject trailLineMarker;
	public GameObject wayMarker;

	void Start() {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
	//	levelManager.AddPointToList (gameObject);
		gameObject.name = "TrailPointMarker" + markerNumber.ToString ();
		levelManager.StartCoroutine (levelManager.SetTrailLine ());

		if (target != null) {
			spawner.transform.LookAt (target.transform.position);
			wantedZ = (transform.position - target.transform.position).magnitude;
			Vector3 spawnerScale = new Vector3 (spawner.transform.localScale.x, spawner.transform.localScale.y, wantedZ);
			spawner.transform.localScale = spawnerScale;
			spawner.transform.position = transform.position + (target.transform.position - transform.position) / 2;
		}
		else if (target == null) {
			spawner.SetActive (false);
		}

		StartCoroutine (SpawnSafetyBorders ());
		StartCoroutine (SpawnTrailWayPoint ());
		SpawnWayMarker ();
	}

	public void SpawnWayMarker ()
	{
		RaycastHit hit;

		if (Physics.Raycast (wayMarker.transform.position, -Vector3.up, out hit)) {
			Debug.DrawLine (transform.position, hit.point, Color.red);
			Vector3 updatedPos = new Vector3 (hit.point.x, hit.point.y + (levelManager.skierHeight / 2), hit.point.z);
			levelManager.currentWayUpPosList.Add (updatedPos);
		}
	}

	public IEnumerator SpawnSafetyBorders()
	{
		yield return new WaitForSeconds (0.1f);

		foreach (GameObject go in borderSpawners) {
			RaycastHit hit;

			if (Physics.Raycast(go.transform.position, -Vector3.up, out hit)) {
				Debug.DrawLine (transform.position, hit.point, Color.red);
//				Debug.Log ("" + hit.normal);
				Vector3 updatedPos = new Vector3 (hit.point.x, hit.point.y, hit.point.z);

				GameObject newBorder = Instantiate (borderPrefab, updatedPos, go.transform.rotation);

				newBorder.transform.eulerAngles = spawner.transform.eulerAngles;
				levelManager.currentBorders.Add(newBorder);
			}
		}
	}

	public IEnumerator SpawnTrailWayPoint()
	{
		yield return new WaitForSeconds (0.1f);

		RaycastHit hit;

		if(Physics.Raycast(trailWayPointSpawner.transform.position, -Vector3.up, out hit))
		{
			Debug.DrawLine (transform.position, hit.point, Color.red);
			Vector3 updatedPos = new Vector3 (hit.point.x, hit.point.y + 3.0f, hit.point.z);

			GameObject newWayPoint = Instantiate (trailWayPointPrefab, updatedPos, trailWayPointSpawner.transform.rotation);
			levelManager.currentTrail.GetComponent<TrailComp> ().wps.Add (newWayPoint);
			newWayPoint.transform.parent = levelManager.currentTrail.transform;
		}
	}
}
