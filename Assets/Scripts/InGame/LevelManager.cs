using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class LevelManager : MonoBehaviour {

	public TextsManager textsManager;

	public Terrain levelTerrain;

	public bool isCreatingSkiLift = false;
	public int skiLiftCreationPhase = 0;
	public GameObject skiLiftCreationCollider;
	public GameObject skiLiftStartPrefab;
	public GameObject skiLiftEndPrefab;
	public Vector3 skiLiftStartPos;
	public GameObject currentSkiLift;
	public SkiLiftCollider skiLiftCreationColliderComp;

	public bool isCreatingTrail;
	public int trailCreationPhase;
	public int trailIsOnObstacle = 0;

	public GameObject currentTrail;

	public bool mouseIsDown;

	public Vector3 cursorPos;

	Ray ray;
	RaycastHit hit;

	public bool canBuild;

	public Image trailConfirmationMenu;
	public Image skiLiftConfirmationMenu;
	public Image cantBuildMenu;

	public Text messageTextComp;

	public GameObject constructionMarker;

	public List<GameObject> pointsList;
	public GameObject pointMarkerPrefab;
	public float canSpawnPointPrefabTime;
	public GameObject lastPointMarker;
	float pointsDist;
	public float maxPointsDist;
	public int currentPointNumber;
//	public GameObject currentTrailStartPoint;
	public int currentTrailDifficultyLevel;
	public string currentTrailName = "";

//	public List<Vector3> pointsPosList;

	public GameObject trailStartPointPrefab;
	public GameObject trailMarkerColliderPrefab;
	public GameObject trailLineRend;
	public GameObject potTrailLineRend;

	public List<GameObject> treesToRemoveList;
	public List<GameObject> currentBorders;
	public List<GameObject> currentColliders;
	public List<GameObject> skiersList;
	public List<GameObject> trailLineRendPosList;
	public List<Vector3> currentWayUpPosList;


//	public List<GameObject[,,,,,,,,,,,,,,,,,,,,]> trails;
//	public List<List<GameObject>> trailList;
	public int currentTrailWp = 0;
	public int currentTrailBeingCreated = 0;

	/*public List<GameObject> currentTrail;
	public List<GameObject> trail0Wps;
	public List<GameObject> trail1Wps;
	public List<GameObject> trail2Wps;
	public List<GameObject> trail3Wps;
	public List<GameObject> trail4Wps;
	public List<GameObject> trail5Wps;
	public List<GameObject> trail6Wps;
	public List<GameObject> trail7Wps;
	public List<GameObject> trail8Wps;
	public List<GameObject> trail9Wps;
	public List<GameObject> trail10Wps;
	public List<GameObject> trail11Wps;
	public List<GameObject> trail12Wps;
	public List<GameObject> trail13Wps;
	public List<GameObject> trail14Wps;
	public List<GameObject> trail15Wps;
	public List<GameObject> trail16Wps;
	public List<GameObject> trail17Wps;
	public List<GameObject> trail18Wps;
	public List<GameObject> trail19Wps;
*/

	public GameObject emptyPrefab;
	public GameObject trailPrefab;
	public GameObject treeDestroyer;
	public GameObject skiLiftCabinPrefab;

	public float skierHeight = 6.0f;

	public GameObject[] skierSpawners;
	public float skierSpawningTime;
	public float skierSpawningRate = 0.12f;
	public GameObject skierPrefab;
	public int skiersToSpawn;
	public bool canSpawnSkiers;

	public List<GameObject> totalTreesList;

	public Image messageMenu;
	public Text messageMenuText;


	void Awake()
	{
		textsManager = GameObject.Find ("TextsManager").GetComponent<TextsManager> ();
		levelTerrain = GameObject.Find ("Terrain").GetComponent<Terrain>();
		mouseIsDown = false;
		isCreatingTrail = false;
		trailCreationPhase = 0;
		canBuild = false;

		trailConfirmationMenu.gameObject.SetActive (false);
		skiLiftConfirmationMenu.gameObject.SetActive (false);
		cantBuildMenu.gameObject.SetActive (false);

		pointsList = new List<GameObject> ();
		treesToRemoveList = new List<GameObject> ();
//		trails = new List<GameObject[,,,,,,,,,,,,,,,,,,,,]>();
//		currentTrail = new List<GameObject> ();
		currentBorders = new List<GameObject> ();
		currentColliders = new List<GameObject> ();
		skiersList = new List<GameObject> ();
		trailLineRendPosList = new List<GameObject> ();
		totalTreesList = new List<GameObject> ();
		currentWayUpPosList = new List<Vector3> ();
		skiersToSpawn = 0;
		canSpawnSkiers = false;
		messageMenu.gameObject.SetActive (false);
		SetTrailLists ();

	//	StartCoroutine (BatchTrees ());
	}

	void Start () {
		skierSpawners = GameObject.FindGameObjectsWithTag ("SkierSpawner");
	}

	/*public IEnumerator BatchTrees()
	{
		yield return new WaitForSeconds (1.2f);
		StaticBatchingUtility.Combine(
	}*/

	void Update()
	{
		ray = Camera.main.ScreenPointToRay (Input.mousePosition);

		if (Input.GetMouseButtonDown (0)) {
			mouseIsDown = true;
		} else if (Input.GetMouseButtonUp (0)) {
			mouseIsDown = false;
			if (isCreatingTrail == true) {
				SetTreesColliderOnOff (0);
				if (trailCreationPhase == 0 || trailCreationPhase == 1) {
					if (pointsList.Count > 1) {
						StartCoroutine (CheckTrailCreation ());
						trailCreationPhase = 2;
					} else if (pointsList.Count <= 1) {
						//SetTrailCreationModeOff ();
						CancelThisTrail();
					}
				}
			}
		}

		if (Physics.Raycast (ray, out hit)) {
			if (hit.transform.gameObject.tag == "Floor") {
				cursorPos = hit.point;
				Debug.DrawLine (Camera.main.transform.position, hit.point, Color.red);
			}
		}

		if (isCreatingTrail == true) {
			if (mouseIsDown == true) {
				if (trailCreationPhase == 0) {
					if (pointsList.Count <= 0) {
						CreateTrailStart ();
					}
				} else if (trailCreationPhase == 1) {
					if (lastPointMarker != null) {
						if (Physics.Raycast (ray, out hit)) {
							if (hit.transform.gameObject.tag == "Floor") {
								pointsDist = Vector3.Distance (lastPointMarker.transform.position, cursorPos);
								if (pointsDist >= maxPointsDist && pointsDist <= (maxPointsDist + 10.0f)) {
									if (cursorPos.y <= (lastPointMarker.transform.position.y + 5.0f)) {
										SpawnNewTrailPoint ();
									}
								}
							}
						}

						SetPotTrailLineRend ();
					}
				} else if (trailCreationPhase == 2) {
						
				}
			}
		}

		if (isCreatingSkiLift == true) {
			constructionMarker.transform.position = cursorPos;

			if (skiLiftCreationPhase == 0) {
				if (Input.GetMouseButtonDown (0)) {
					if (canBuild == true) {
						StartCoroutine (CreateSkiLiftStart ());
					} else if (canBuild == false) {
						StartCoroutine (ShowCantBuildMessage (1));
					}
				}
			} else if (skiLiftCreationPhase == 1) {

				skiLiftCreationCollider.transform.LookAt (cursorPos);
				float cursorDist = Vector3.Distance (skiLiftStartPos, cursorPos);
				Vector3 updatedScale = new Vector3(skiLiftCreationCollider.transform.localScale.x,skiLiftCreationCollider.transform.localScale.y, cursorDist);
				skiLiftCreationCollider.transform.localScale = updatedScale;
				if (Input.GetMouseButtonDown (0)) {
					if (canBuild == true) {
						StartCoroutine (CreateSkiLiftEnd ());
					} else if (canBuild == false) {
						StartCoroutine (ShowCantBuildMessage (1));
					}
				}
			}
		}

		if (canSpawnSkiers == true) {
			if (Time.time >= skierSpawningTime) {
				skierSpawningTime = Time.time + skierSpawningRate;
				CreateNewSkier ();
			}
		}
	}

	public IEnumerator SetTrailCreationModeOn()
	{
		if (isCreatingSkiLift == true) {SetSkiLiftCreationModeOff ();}

		treesToRemoveList.Clear ();
		currentBorders.Clear ();
		currentColliders.Clear ();
		currentWayUpPosList.Clear ();
		trailIsOnObstacle = 0;
		yield return new WaitForSeconds (0.25f);
//		currentTrail.Clear();
		trailLineRend.SetActive (true);
		trailLineRend.GetComponent<LineRenderer> ().SetVertexCount (0);
		currentPointNumber = 0;
		isCreatingTrail = true;
		trailCreationPhase = 0;

		SetTreesColliderOnOff (1);
		ShowMessageMenu (0);
	}

	public void SetTrailCreationModeOff()
	{
		SetTreesColliderOnOff (0);
		isCreatingTrail = false;
		trailLineRend.SetActive (false);
		potTrailLineRend.SetActive (false);
		pointsList.Clear ();
		trailLineRendPosList.Clear ();
		trailConfirmationMenu.gameObject.SetActive (false);
		trailLineRend.GetComponent<LineRenderer> ().SetVertexCount (0);
	//	trailLineRend.GetComponent<LineRenderer> ().SetPosition (0, Vector3.zero);
//		Debug.Log("1");


		foreach (GameObject go in currentColliders) {
			DestroyObject (go);
		}
//		Debug.Log("2");
	/*	foreach (GameObject go in currentBorders) {
			DestroyObject (go);
		}*/
//		Debug.Log("3");

	}

	public void CancelThisTrail()
	{
		trailConfirmationMenu.gameObject.SetActive (false);
		foreach (GameObject go in currentBorders) {
			DestroyObject (go);
		}

		if (currentTrail != null) {
			DestroyObject (currentTrail);
			currentTrail = null;
		}

		StartCoroutine(UnHideTrees ());
		SetTrailCreationModeOff ();
		HideMessageMenu ();
		//SetTrailCreationModeOff ();
	}

	public void ShowTrailConfirmationMenu ()
	{
		trailConfirmationMenu.gameObject.SetActive (true);
	}

	public void ShowSkiLiftConfirmationMenu()
	{
		skiLiftConfirmationMenu.gameObject.SetActive (true);
	}

	public IEnumerator CreateTrail()
	{
		messageMenu.gameObject.SetActive (false);
		trailConfirmationMenu.gameObject.SetActive (false);
		StartCoroutine(RemoveTrees ());
		yield return new WaitForSeconds (0.25f);

		StartCoroutine(FinishNewTrail ());

	}

	public void AddPointToList(GameObject pMarker)
	{
		pointsList.Add (pMarker);
		lastPointMarker = pMarker;

//		trailLineRendPosList.Add (pMarker.GetComponent<TrailPointMarker>().trailLineMarker);
	}

	public IEnumerator SetTrailLine()
	{
		/*Vector3[] pointsPos = new Vector3[trailLineRendPosList.Count];
		int i = 0;
		foreach (GameObject go in trailLineRendPosList) {
			pointsPos [i] = go.transform.position;
			i += 1;
		}*/

		trailLineRend.GetComponent<LineRenderer>().SetVertexCount(trailLineRendPosList.Count);
		for (int j = 0; j < trailLineRendPosList.Count; j++){
			trailLineRend.GetComponent<LineRenderer>().SetPosition(j, trailLineRendPosList[j].transform.position);
		}
		yield return new WaitForSeconds (0.05f);

	}

	public void SetPotTrailLineRend()
	{
		Vector3 updatedCursorPos = new Vector3 (cursorPos.x, cursorPos.y + 5.0f, cursorPos.z);
		potTrailLineRend.GetComponent<LineRenderer> ().SetPosition (0, lastPointMarker.transform.position);
		potTrailLineRend.GetComponent<LineRenderer> ().SetPosition (1, updatedCursorPos);

	}

	public IEnumerator CheckTrailCreation()
	{
		int i = 0;
		foreach (GameObject go in pointsList) {

			if (i < pointsList.Count - 1) {
				GameObject newCollider = Instantiate (trailMarkerColliderPrefab, go.transform.position, go.transform.rotation);
				newCollider.GetComponent<CapsuleCollider> ().radius = trailLineRend.GetComponent<LineRenderer> ().startWidth * 1.35f;
				newCollider.GetComponent<CapsuleCollider> ().direction = 2;
				newCollider.transform.position = go.transform.position + (pointsList [i + 1].transform.position - go.transform.position) / 2;
	
				//			capsule.transform.position = start.position + (target.position - start.position) / 2;
					
				newCollider.transform.LookAt (pointsList [i + 1].transform.position);
				newCollider.GetComponent<CapsuleCollider> ().height = (go.transform.position - pointsList [i + 1].transform.position).magnitude;
					
				i += 1;
			}
		}

		yield return new WaitForSeconds (0.15f);

		if (trailIsOnObstacle <= 0) {
			if (treesToRemoveList.Count >= 1) {
				HideTrees ();
			}

			ShowTrailConfirmationMenu ();
		} 
		else if (trailIsOnObstacle > 0) {
			foreach (GameObject go in pointsList) {
				DestroyObject (go);
			}
			yield return new WaitForSeconds (0.1f);
			SetTrailCreationModeOff ();
		}
	}

	public void AddTreeToList(GameObject t)
	{
		treesToRemoveList.Add (t);
	}

	public IEnumerator UnHideTrees()
	{
		foreach (GameObject go in treesToRemoveList) {
			go.GetComponent<MeshRenderer> ().enabled = true;
		}

		yield return new WaitForSeconds (0.2f);

		treesToRemoveList.Clear ();
	}

	public void HideTrees()
	{
		foreach (GameObject go in treesToRemoveList) {
			if (go != null) {
				go.GetComponent<MeshRenderer> ().enabled = false;
			}
		}
	}

	public IEnumerator RemoveTrees()
	{
		if (treesToRemoveList.Count > 0) {
			foreach (GameObject go in treesToRemoveList) {
				DestroyObject (go);
			}
		}

		yield return new WaitForSeconds (0.18f);

		treesToRemoveList.Clear ();
	}

	public void SetTrailLists()
	{
	/*	trail0Wps = new List<GameObject> ();
		trail1Wps = new List<GameObject> ();
		trail2Wps = new List<GameObject> ();
		trail3Wps = new List<GameObject> ();
		trail4Wps = new List<GameObject> ();
		trail5Wps = new List<GameObject> ();
		trail6Wps = new List<GameObject> ();
		trail7Wps = new List<GameObject> ();
		trail8Wps = new List<GameObject> ();
		trail9Wps = new List<GameObject> ();
		trail10Wps = new List<GameObject> ();
		trail11Wps = new List<GameObject> ();
		trail12Wps = new List<GameObject> ();
		trail13Wps = new List<GameObject> ();
		trail14Wps = new List<GameObject> ();
		trail15Wps = new List<GameObject> ();
		trail16Wps = new List<GameObject> ();
		trail17Wps = new List<GameObject> ();
		trail18Wps = new List<GameObject> ();
		trail19Wps = new List<GameObject> ();
*/
	}

	public IEnumerator FinishNewTrail()
	{
		TrailComp trailComp = currentTrail.GetComponent<TrailComp> ();

		yield return new WaitForSeconds (0.01f);

		trailComp.name = "Trail" + currentTrailBeingCreated.ToString ();
		trailComp.trailEnd = trailComp.wps [trailComp.wps.Count - 1];
		trailComp.trailNumber = currentTrailBeingCreated;
//		trailComp.wps = currentTrail;
		trailComp.trailDifficulty = currentTrailDifficultyLevel;
		trailComp.StartCoroutine(trailComp.ScanArea ());
/*		foreach (GameObject go in currentTrail) {
			go.transform.parent = newTrail.transform;
		}*/
		foreach (GameObject go in currentBorders) {
			go.transform.parent = currentTrail.transform;
		}

		foreach (GameObject go in currentColliders) {
			DestroyObject (go);
			//go.transform.parent = newTrail.transform;
		}

		yield return new WaitForSeconds (0.1f);

		currentWayUpPosList.Reverse ();
		GameObject newWayUp = Instantiate (emptyPrefab, currentWayUpPosList [0], transform.rotation);
		newWayUp.name = "WayUp" + currentTrailBeingCreated.ToString ();
		newWayUp.tag = "WayUpStart";
		newWayUp.AddComponent<WayUpComp> ();
		BoxCollider bComp = newWayUp.AddComponent<BoxCollider> ();
		bComp.isTrigger = true;
		Rigidbody rigidC= newWayUp.AddComponent<Rigidbody> ();
		rigidC.useGravity = false;
		int w = 0;
		foreach (Vector3 v in currentWayUpPosList) {
			if (w >= 1) {
				GameObject newWp = Instantiate (emptyPrefab, v, transform.rotation);
				newWp.transform.parent = newWayUp.transform;
				newWp.tag = "wayUpWP";
				newWayUp.GetComponent<WayUpComp> ().wps.Add (newWp);
				BoxCollider boxComp = newWp.AddComponent<BoxCollider> ();
				boxComp.isTrigger = true;
				Rigidbody rigidComp = newWp.AddComponent<Rigidbody> ();
				rigidComp.useGravity = false;
				if (w == currentWayUpPosList.Count - 1) {
					newWp.name = "WayUpFinish";
				}
			}
			w += 1;
		}

		yield return new WaitForSeconds (0.25f);

		SetTrailCreationModeOff ();
		currentTrailBeingCreated += 1;

		trailComp.isFinished = true;
		trailComp.StartCoroutine(trailComp.ScanArea ());
		StartToSpawnSkiers (12);

	}

	public IEnumerator ShowCantBuildMessage(int i)
	{
		string messageText = "";

		if (i == 0) {
			messageText = textsManager.youCantBuildText + textsManager.trailText + textsManager.hereText;
		}
		else if (i == 1) {
			messageText = textsManager.youCantBuildText + textsManager.skiLiftText + textsManager.hereText;
		}

		yield return new WaitForSeconds (0.15f);

		messageTextComp.text = messageText;
		cantBuildMenu.gameObject.SetActive (true);

		yield return new WaitForSeconds (2.0f);

		cantBuildMenu.gameObject.SetActive (false);
	}


	public void StartToSpawnSkiers(int s)
	{
		skiersToSpawn += s;
		canSpawnSkiers = true;
	}

	public void CreateNewSkier()
	{
		int randSpawner = Random.Range (0, skierSpawners.Length);
		Vector3 spawnerPos = skierSpawners [randSpawner].transform.position;
		Quaternion spawnerRot = skierSpawners [randSpawner].transform.rotation;
		GameObject newSkier = Instantiate (skierPrefab, spawnerPos, spawnerRot);
		skiersToSpawn -= 1;
		if(skiersToSpawn <= 0)
		{
			canSpawnSkiers = false;
		}
	}

	public void SetTreesColliderOnOff(int onOff)
	{
		if (onOff == 0) {
			foreach (GameObject go in totalTreesList) {
				if (go != null) {
					go.GetComponent<BoxCollider> ().enabled = true;
				}
			}
		}
		else if (onOff == 1) {
			foreach (GameObject go in totalTreesList) {
				if (go != null) {
					go.GetComponent<BoxCollider> ().enabled = false;
				}
			}
		}
	}

	public IEnumerator ChangeTerrainTexture(int t)
	{
		yield return new WaitForSeconds (0.1f);
		/*
		if (t == 0) {
			foreach (GameObject go in trail0Wps) {
				Debug.Log ("a finir de changer la texture de la neige");
			}
		}

		Terrain tComp = GameObject.Find("Terrain").GetComponent<Terrain>();
//		TerrainData terrainData = tComp.terrainData;
		float[,,] newMap = new float[tComp.terrainData.alphamapWidth, tComp.terrainData.alphamapHeight, 2];
		tComp.terrainData.SetAlphamaps(0, 0, newMap);

		Debug.Log ("a finir de changer la texture de la neige");*/
	}

	public IEnumerator SetSkiLiftCreationModeOn()
	{
		if (isCreatingTrail == true) {SetTrailCreationModeOff();}
		isCreatingSkiLift = true;
		skiLiftCreationPhase = 0;
		constructionMarker.SetActive (true);
		treesToRemoveList.Clear ();
		yield return new WaitForSeconds (0.01f);
		ShowMessageMenu (1);
	}

	public IEnumerator SetSkiLiftCreationModeOff()
	{
		isCreatingSkiLift = false;
		constructionMarker.SetActive (false);
		skiLiftCreationCollider.SetActive (false);
		treesToRemoveList.Clear ();
		skiLiftCreationCollider.SetActive (false);
		yield return new WaitForSeconds (0.01f);

	}

	public IEnumerator CreateSkiLift()
	{
//		Debug.Log ("Create Ski Lift");
		skiLiftConfirmationMenu.gameObject.SetActive(false);
		yield return new WaitForSeconds (0.01f);
		skiLiftCreationColliderComp.StartCoroutine (skiLiftCreationColliderComp.SpawnSkiLiftPylons ());
		currentSkiLift.GetComponent<SkiLiftComp> ().isFinished = true;

		if (treesToRemoveList.Count >= 1) {
			foreach (GameObject t in treesToRemoveList) {
				DestroyObject (t);
			}
		}
	}

	public void CancelThisSkiLift()
	{
		if (currentSkiLift != null) {
			DestroyObject(currentSkiLift);
			currentSkiLift = null;
		}
		SetSkiLiftCreationModeOff ();
		skiLiftConfirmationMenu.gameObject.SetActive (false);
		HideMessageMenu ();
	}

	public IEnumerator CreateSkiLiftStart()
	{
//		Debug.Log ("skiLiftStart");
		Vector3 updatedPos = new Vector3 (cursorPos.x, cursorPos.y + 5, cursorPos.z);
		GameObject newSkiLift = Instantiate (emptyPrefab, updatedPos, transform.rotation);
		SkiLiftComp newSkiLiftComp = newSkiLift.AddComponent<SkiLiftComp> ();
		GameObject newSkiLiftStart = Instantiate (skiLiftStartPrefab, newSkiLift.transform.position, transform.rotation); 
		skiLiftStartPos = newSkiLiftStart.transform.position;
		newSkiLiftStart.transform.parent = newSkiLift.transform;
		newSkiLiftComp.skiLiftStart = newSkiLiftStart;
		currentSkiLift = newSkiLift;
		yield return new WaitForSeconds (0.01f);

		skiLiftCreationCollider.SetActive (true);
		skiLiftCreationCollider.transform.position = updatedPos;
		skiLiftCreationPhase = 1;
		ShowMessageMenu (2);
	}

	public IEnumerator CreateSkiLiftEnd()
	{
//		Debug.Log ("skiLiftEnd");
		SkiLiftComp newSkiLiftComp = currentSkiLift.GetComponent<SkiLiftComp> ();
		Vector3 updatedPos = new Vector3 (cursorPos.x, cursorPos.y + 5, cursorPos.z);
		GameObject newSkiLiftEnd = Instantiate (skiLiftEndPrefab, updatedPos, transform.rotation);
		newSkiLiftEnd.transform.parent = newSkiLiftComp.gameObject.transform;
		newSkiLiftComp.skiLiftEnd = newSkiLiftEnd;
		Vector3 startPos = new Vector3(newSkiLiftComp.skiLiftStart.transform.position.x,newSkiLiftEnd.transform.position.y , newSkiLiftComp.skiLiftStart.transform.position.z);
		Vector3 endPos = new Vector3(newSkiLiftEnd.transform.position.x, newSkiLiftComp.skiLiftStart.transform.position.y, newSkiLiftEnd.transform.position.z);
		newSkiLiftComp.skiLiftStart.transform.LookAt(endPos);
		newSkiLiftEnd.transform.LookAt(startPos);
			
		yield return new WaitForSeconds (0.01f);

		skiLiftCreationPhase = 2;

		yield return new WaitForSeconds (0.5f);

		ShowSkiLiftConfirmationMenu ();

		HideMessageMenu ();
	}

	public void DeleteTrees()
	{
		foreach (GameObject t in treesToRemoveList) {
	//		treesToRemoveList.Remove (t);
			DestroyObject (t);

		}
	}

	public void CreateTrailStart()
	{
		Vector3 pointPos = new Vector3 (cursorPos.x, cursorPos.y + 0.5f, cursorPos.z);
		GameObject newTrail = Instantiate (trailPrefab, pointPos, transform.rotation);
		currentTrail = newTrail;
		TrailComp trailComp = newTrail.GetComponent<TrailComp> ();
		GameObject trailStartPoint = Instantiate (trailStartPointPrefab, pointPos, transform.rotation);
		trailStartPoint.transform.parent = newTrail.transform;
		trailComp.trailStart = trailStartPoint;
		lastPointMarker = trailStartPoint;
		potTrailLineRend.SetActive (true);
		trailCreationPhase = 1;
	}

	public void SpawnNewTrailPoint()
	{
		TrailComp trailComp = currentTrail.GetComponent<TrailComp> ();
		Vector3 pointPos = new Vector3 (cursorPos.x, cursorPos.y + (skierHeight / 2), cursorPos.z);

		GameObject newPoint = Instantiate (pointMarkerPrefab, pointPos, transform.rotation);
		TrailPointMarker markerComp = newPoint.GetComponent<TrailPointMarker> ();

		markerComp.target = lastPointMarker;
		AddPointToList (newPoint);
		trailLineRendPosList.Add (markerComp.trailLineMarker);
		markerComp.markerNumber = currentPointNumber;
		currentColliders.Add (newPoint);
		currentPointNumber += 1;
	}

	public void ShowMessageMenu(int i)
	{
		messageMenu.gameObject.SetActive (true);
		messageMenuText.text = textsManager.messageMenuTexts [i];
	}

	public void HideMessageMenu()
	{
		messageMenu.gameObject.SetActive (false);
	}

	public void RestartLevel()
	{
		string levelName = SceneManager.GetActiveScene ().name;
		SceneManager.LoadScene (levelName);
	}
}
