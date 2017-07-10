using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkierTryPrefab : MonoBehaviour {

	public bool isTestSkier;
	public LevelManager levelManager;

	public float turnSpeed = 5.0f;

	public Material[] skierMats;

	public bool canCheckFloor;
	public float checkFloorTime;
	public float checkFloorRate;
	public float minCheckRate = 0.1f;
	public float maxCheckRate = 0.2f;
	public Transform floorChecker;
	public float skierHeight = 6.0f;
	RaycastHit hit;
	public float wantedY;

	public GameObject target;
	public Vector3 targetPos;
	public List<GameObject> wps;
	public float skierSpeed;
	public float skierMinSpeed;
	public float skierMaxSpeed;
	public float speedMultiplier = 1.0f;

	public int currentWpNumber = 1;
	public int currentWayUpNumber = 0;
	public bool isOnTrail;

	public List<GameObject> potDest;
	public bool isWaitingForSkiLift;

	public float areaDist = 50f;

	void Awake () {
		canCheckFloor = true;
		isWaitingForSkiLift = false;
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		checkFloorRate = Random.Range (minCheckRate, maxCheckRate);
		skierSpeed = Random.Range (skierMinSpeed, skierMaxSpeed);
		SetSkierAppearence ();

		potDest = new List<GameObject>();

		StartCoroutine(FindNewDestination ());
	}
	
	void Update () {

		if (isOnTrail == false) {
			speedMultiplier = 1.0f;
		} else if (isOnTrail == true) {
			speedMultiplier = 2.0f;
		}

		if(canCheckFloor == true)
		{
			if (Time.time >= checkFloorTime) {
				checkFloorTime = Time.time + checkFloorRate;
				if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
					if (isTestSkier == true) {
						Debug.Log ("" + hit.distance);
					}
					Debug.DrawLine (transform.position, hit.point, Color.red);
					if (hit.distance < 3.0f) {
						wantedY = hit.point.y + (skierHeight / 2);
						transform.position = new Vector3 (transform.position.x, wantedY, transform.position.z);
					}
				}
			}
		}
		if (target != null) {

			float dist = Vector3.Distance (transform.position, target.transform.position);
			if (isTestSkier == true) {
				Debug.Log ("" + dist);
			}
			//	transform.position = Vector3.MoveTowards (transform.position, target.transform.position, (skierSpeed * speedMultiplier) * Time.deltaTime);
			transform.position = Vector3.MoveTowards (transform.position, targetPos, (skierSpeed * speedMultiplier) * Time.deltaTime);
		//	var rotation = Quaternion.LookRotation(target.transform.position - transform.position);
			var rotation = Quaternion.LookRotation(targetPos - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * turnSpeed);

		}
	}

	public void SetSkierAppearence()
	{
		int randMat = Random.Range (0, skierMats.Length);
		GetComponent<MeshRenderer> ().material = skierMats [randMat];
	}

	void OnTriggerEnter(Collider other)
	{
		if (isOnTrail == true) {
			if (other.transform.gameObject.tag == "TrailWp") {
				if (other.transform.gameObject == target) {
					if (currentWpNumber < wps.Count - 1) {
						currentWpNumber += 1;
						target = wps [currentWpNumber];
						float r = Random.Range (-2.5f, 2.5f);
						Vector3 randPos = new Vector3(target.transform.position.x + r,target.transform.position.y,target.transform.position.z + r);
						targetPos = randPos;
					} else if (currentWpNumber >= wps.Count - 1) {
						isOnTrail = false;
						//wps.Clear ();
						currentWpNumber = 0;
				//		target = null;
						StartCoroutine(FindNewDestination ());
					}
				}
			}
		} else if (isOnTrail == false) {
			if (other.transform.gameObject.tag == "TrailStartPoint") {
				SetPath (other.transform.parent.transform.gameObject.GetComponent<TrailComp> ().wps);
			} else if (other.transform.gameObject.tag == "SkiLiftStartPoint") {
				isWaitingForSkiLift = true;
				canCheckFloor = false;
				target = null;
				other.transform.parent.transform.gameObject.GetComponent<SkiLiftComp> ().AddSkierToQueue (gameObject);
			} else if (other.transform.gameObject.tag == "WayUpStart") {
				SetWay (other.transform.gameObject.GetComponent<WayUpComp> ().wps);
			} else if (other.transform.gameObject.tag == "wayUpWP") {
				if (other.transform.gameObject == target) {
					if (currentWayUpNumber < wps.Count - 1) {
						currentWayUpNumber += 1;
						target = wps [currentWayUpNumber];
						targetPos = target.transform.position;
					} else if (currentWayUpNumber >= wps.Count - 1) {
						currentWayUpNumber = 0;
						StartCoroutine(FindNewDestination());
					}
				}
			}
		}
	}

	public void SetWay(List<GameObject> wpList)
	{
		wps = wpList;
		isOnTrail = false;
		target = wps[currentWayUpNumber];
		targetPos = target.transform.position;

	}

	public void SetPath(List<GameObject> wpList)
	{
		wps = wpList;
		isOnTrail = true;
		target = wps[currentWpNumber];
		float r = Random.Range (-2.5f, 2.5f);
		Vector3 randPos = new Vector3(target.transform.position.x + r,target.transform.position.y,target.transform.position.z + r);
		targetPos = randPos;

	}

	public IEnumerator FindNewDestination()
	{
		potDest.Clear ();
		List<GameObject> finalPotDest = new List<GameObject>();
		GameObject[] potTrails = GameObject.FindGameObjectsWithTag ("TrailStartPoint");
		if (potTrails.Length > 0) 
		{
			foreach (GameObject go in potTrails) {
				if (go.transform.parent.transform.gameObject.GetComponent<TrailComp> ().isFinished == true) {
					potDest.Add (go);
				}
			}
		}


		GameObject[] potSkiLift = GameObject.FindGameObjectsWithTag ("SkiLiftStartPoint");
		if (potSkiLift.Length > 0) 
		{
			foreach (GameObject go in potSkiLift) {
				if (go.transform.parent.transform.gameObject.GetComponent<SkiLiftComp> ().isFinished == true) {
					potDest.Add (go);
				}
			}
		}
		else if(potSkiLift.Length <= 0)
		{
			GameObject[] potWayUp = GameObject.FindGameObjectsWithTag ("WayUpStart");
			if (potWayUp.Length > 0) {foreach(GameObject go in potWayUp){potDest.Add (go);}}
			isWaitingForSkiLift = false;
		}

		yield return new WaitForSeconds (0.15f);

			potDest.Sort (ByDistance);

		foreach(GameObject go in potDest)
		{
			float dist = Vector3.Distance (transform.position, go.transform.position);
//			Debug.Log ("" + dist);
			if (dist <= areaDist) {
				finalPotDest.Add (go);
			}
		}

		yield return new WaitForSeconds (0.1f);

		if (finalPotDest.Count >= 1) {
			int randDest = Random.Range (0, finalPotDest.Count);
			target = finalPotDest [randDest];
			targetPos = finalPotDest[randDest].transform.position;
		}
		else if (finalPotDest.Count < 1) {
			target = potDest [0];
			targetPos = target.transform.position;
		}

		/*
		target = potDest [0];
		targetPos = target.transform.position;*/
	}


	public int ByDistance(GameObject a, GameObject b)
	{
		var dstToA = Vector3.Distance(transform.position, a.transform.position);
		var dstToB = Vector2.Distance(transform.position, b.transform.position);
		return dstToA.CompareTo(dstToB);
	}

	public void IsOffOfSkiLift()
	{
		Debug.Log ("a finir");
		canCheckFloor = true;
	}
}

