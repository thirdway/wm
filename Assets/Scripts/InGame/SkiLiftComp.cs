using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftComp : MonoBehaviour {

	public LevelManager levelManager;
	public bool isFinished = false;
	public GameObject skiLiftStart;
	public GameObject skiLiftEnd;
	public List<GameObject> wps;
	public List<GameObject> pylonsList;
	public bool canSpawnCabins = false;
	public GameObject cabinPrefab;
	public float cabinSpawningTime;
	public float cabinSpawningRate = 10.0f;
	public int maxCabinNumber = 4;
	public int wantedCabinNumber = 4;
	public int currentCabinNumber = 0;

	public List<GameObject> currentSkiersWaiting;
	public List<GameObject> skiersToUnload;
	public float unloadSkiersTime;
	public float unloadSkiersRate = 1.5f;
	public GameObject treeDestroyerPrefab;

	void Awake()
	{
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		cabinPrefab = levelManager.skiLiftCabinPrefab;
		treeDestroyerPrefab = levelManager.treeDestroyer;
		currentSkiersWaiting = new List<GameObject> ();
		skiersToUnload = new List<GameObject>();
	}

	void Start () {
		wps = new List<GameObject> ();
		pylonsList = new List<GameObject> ();

	}
	
	void Update () {
		if (canSpawnCabins == true) {
			if (Time.time >= cabinSpawningTime) {
				if (currentCabinNumber < wantedCabinNumber) {
					cabinSpawningTime = Time.time + cabinSpawningRate;
					SpawnNewCabin ();
				}
			}
		}

		if (skiersToUnload.Count >= 1)
		{
			if (Time.time >= unloadSkiersTime) {
				unloadSkiersTime = Time.time + unloadSkiersRate;
				UnloadSkier ();
			}
		}
	}

	public void StartToSortWayPoints()
	{
		StartCoroutine (SortWayPoints ());
	}

	public IEnumerator SortWayPoints()
	{
		Debug.Log ("Sort waypoints");

		foreach (GameObject rightGo in pylonsList) {
			wps.Add(rightGo.GetComponent<SkiLiftPylonComp> ().cableAnchorRight);
		}

		yield return new WaitForSeconds (0.05f);

		wps.Add (skiLiftEnd.GetComponent<SkiLiftEndComp>().leftAnchor);
		wps.Add (skiLiftEnd.GetComponent<SkiLiftEndComp>().rightAnchor);

		yield return new WaitForSeconds (0.05f);

		pylonsList.Reverse ();

		foreach (GameObject leftGo in pylonsList) {
			wps.Add (leftGo.GetComponent<SkiLiftPylonComp> ().cableAnchorLeft);
		}

		yield return new WaitForSeconds (0.05f);

		wps.Add (skiLiftStart.GetComponent<SkiLiftStartComp> ().leftAnchor);
		wps.Add (skiLiftStart.GetComponent<SkiLiftStartComp> ().rightAnchor);

		canSpawnCabins = true;

		StartCoroutine (ScanArea ());
	}

	public void SpawnNewCabin ()
	{
		GameObject newCabin = Instantiate (cabinPrefab, skiLiftStart.GetComponent<SkiLiftStartComp> ().rightAnchor.transform.position, skiLiftStart.GetComponent<SkiLiftStartComp> ().rightAnchor.transform.rotation);
		SkiLiftCabin cabinComp = newCabin.GetComponent<SkiLiftCabin> ();
		cabinComp.wps = wps;
		cabinComp.StartCoroutine(cabinComp.SetCabinOn ());
		currentCabinNumber += 1;
		if (currentSkiersWaiting.Count >= 1) {
			LoadSkiers (newCabin);
		}
	}
		
	public void UnloadSkier()
	{
		GameObject skier = skiersToUnload[0];
		skiersToUnload.Remove (skier);
		SkierTryPrefab skierComp = skier.GetComponent<SkierTryPrefab> ();
		skierComp.StartCoroutine(skierComp.FindNewDestination());

	}
	public void AddSkierToQueue(GameObject s)
	{
		currentSkiersWaiting.Add (s);
	}

	public void LoadSkiers(GameObject cabin)
	{
		SkiLiftCabin cabinComp = cabin.GetComponent<SkiLiftCabin> ();
		int i = cabinComp.maxSkierCapacity;;
		foreach (GameObject go in currentSkiersWaiting) {
			if (i > 0) {
				if (i < cabinComp.maxSkierCapacity) {
					go.transform.parent = cabin.transform;
					go.transform.position = cabinComp.cabinModel.transform.position;
					cabinComp.currentLoadedSkiers.Add (go);
					StartCoroutine(RemoveSkierFromQueue(go));
					cabinComp.hasSkiers = true;
				}
			}
			i -= 1;
		}
	}

	public IEnumerator RemoveSkierFromQueue(GameObject s)
	{
		yield return new WaitForSeconds (0.1f);
		currentSkiersWaiting.Remove (s);
	}

	public IEnumerator ScanArea()
	{
		List <GameObject> potDest = new List<GameObject> ();

		GameObject[] potTrails = GameObject.FindGameObjectsWithTag ("TrailStartPoint");
		if (potTrails.Length > 0) {
			foreach (GameObject go in potTrails) {
				if (go.transform.parent.transform.gameObject.GetComponent<TrailComp> ().isFinished == true) {
					potDest.Add (go);
				}
			}
		}
		GameObject[] potSkiLift = GameObject.FindGameObjectsWithTag ("SkiLiftStartPoint");
		if (potSkiLift.Length > 0) {
			foreach (GameObject go in potSkiLift) {
				if (go.transform.parent.transform.gameObject != gameObject) {
					if (go.transform.parent.transform.gameObject.GetComponent<SkiLiftComp> ().isFinished == true) {
						potDest.Add (go);
					}
				}
			}
		}

		yield return new WaitForSeconds (0.2f);

		potDest.Sort (ByDistanceFromSkiLiftEnd);

		levelManager.treesToRemoveList.Clear ();

		if (potDest.Count >= 1) {
			foreach (GameObject go in potDest) {
				float dist = Vector3.Distance (skiLiftEnd.transform.position, go.transform.position);
				Debug.Log ("" + dist);
				if (dist <= 250) {
					GameObject newTreeDestroyer = Instantiate (treeDestroyerPrefab, skiLiftEnd.transform.position, skiLiftEnd.transform.rotation);
					Vector3 newScale = new Vector3 (newTreeDestroyer.transform.localScale.x, newTreeDestroyer.transform.localScale.z, dist);
					newTreeDestroyer.transform.LookAt (go.transform.position);
					newTreeDestroyer.transform.localScale = newScale;
				}
			}

			yield return new WaitForSeconds (0.1f);

			levelManager.DeleteTrees ();

			StartCoroutine(ScanAreaForStarts ());

		}
	}

	public IEnumerator ScanAreaForStarts()
	{
		List <GameObject> potDest = new List<GameObject> ();

		GameObject[] potTrails = GameObject.FindGameObjectsWithTag ("TrailStartPoint");
		if (potTrails.Length > 0) {
			foreach (GameObject go in potTrails) {
				if (go.transform.parent.transform.gameObject.GetComponent<TrailComp> ().isFinished == true) {
					potDest.Add (go.transform.parent.transform.gameObject.GetComponent<TrailComp> ().trailEnd);
				}
			}
		}

		yield return new WaitForSeconds(0.1f);

		if (potDest.Count >= 1) {
			foreach (GameObject go in potDest) {
				float dist = Vector3.Distance (skiLiftStart.transform.position, go.transform.position);
				Debug.Log ("" + dist);
				if (dist <= 250) {
					GameObject newTreeDestroyer = Instantiate (treeDestroyerPrefab, skiLiftStart.transform.position, skiLiftStart.transform.rotation);
					Vector3 newScale = new Vector3 (newTreeDestroyer.transform.localScale.x, newTreeDestroyer.transform.localScale.z, dist);
					newTreeDestroyer.transform.LookAt (go.transform.position);
					newTreeDestroyer.transform.localScale = newScale;
				}
			}

			yield return new WaitForSeconds (0.1f);

			levelManager.DeleteTrees ();
		}
	}


	public int ByDistance(GameObject a, GameObject b)
	{
		var dstToA = Vector3.Distance(transform.position, a.transform.position);
		var dstToB = Vector2.Distance(transform.position, b.transform.position);
		return dstToA.CompareTo(dstToB);
	}

	public int ByDistanceFromSkiLiftEnd(GameObject a, GameObject b)
	{
		var dstToA = Vector3.Distance(skiLiftEnd.transform.position, a.transform.position);
		var dstToB = Vector2.Distance(skiLiftEnd.transform.position, b.transform.position);
		return dstToA.CompareTo(dstToB);
	}
}
