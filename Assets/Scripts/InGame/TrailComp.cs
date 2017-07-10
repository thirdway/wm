using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrailComp : MonoBehaviour {

	public int trailNumber;
	public GameObject trailStart;
	public GameObject trailEnd;
	public List<GameObject> wps;
	public LevelManager levelManager;
	public string trailName;
	public int trailDifficulty;
	public Color[] trailColors;
	public GameObject imageCanvas;
	public Image trailMenu;
	public Text trailNameText;
	public bool canShowTrailMenu;
	public GameObject cam;
	public bool isFinished;
	public GameObject treeDestroyerPrefab;



	void Awake() {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		treeDestroyerPrefab = levelManager.treeDestroyer;
		cam = GameObject.Find ("Main Camera");
		trailColors = new Color[4];
		trailColors [0] = Color.green;
		trailColors [1] = Color.blue;
		trailColors [2] = Color.red;
		trailColors [3] = Color.black;
		imageCanvas.SetActive (false);

	}

	void Update()
	{
		if (canShowTrailMenu == true) {
			imageCanvas.transform.LookAt (cam.transform.position);
		}
	}

	void Start()
	{
		Invoke ("SetTrailInfos", 0.25f);
	}

	public void SetTrailInfos()
	{
		foreach (GameObject go in trailStart.GetComponent<TrailStartPointComp>().panelPieces) {
			go.GetComponent<MeshRenderer> ().material.color = trailColors [trailDifficulty];
		}
		trailNameText.color = trailColors [trailDifficulty];
		trailNameText.text = "" + trailName;
	}

	public void ShowMenu()
	{
		canShowTrailMenu = true;
		imageCanvas.SetActive (true);
	}

	public void HideMenu()
	{
		canShowTrailMenu = false;
		imageCanvas.SetActive (false);
	}

	public IEnumerator ScanArea()
	{
		List <GameObject> potDest = new List<GameObject> ();

		GameObject[] potTrails = GameObject.FindGameObjectsWithTag ("TrailStartPoint");
		if (potTrails.Length > 0) {
			foreach (GameObject go in potTrails) {
				if (go.transform.parent.transform.gameObject != gameObject) {
					if (go.transform.parent.transform.gameObject.GetComponent<TrailComp> ().isFinished == true) {
						potDest.Add (go);
					}
				}
			}
		}
		GameObject[] potSkiLift = GameObject.FindGameObjectsWithTag ("SkiLiftStartPoint");
		if (potSkiLift.Length > 0) {
			foreach (GameObject go in potSkiLift) {
				if (go.transform.parent.transform.gameObject.GetComponent<SkiLiftComp> ().isFinished == true) {
					potDest.Add (go);
				}
			}
		}

		yield return new WaitForSeconds (0.2f);

		potDest.Sort (ByDistanceFromTrailEnd);

		levelManager.treesToRemoveList.Clear ();

		if (potDest.Count >= 1) {
			foreach (GameObject go in potDest) {
				float dist = Vector3.Distance (trailEnd.transform.position, go.transform.position);
				Debug.Log ("" + dist);
				if (dist <= 250) {
					GameObject newTreeDestroyer = Instantiate (treeDestroyerPrefab, trailEnd.transform.position, trailEnd.transform.rotation);
					Vector3 newScale = new Vector3 (newTreeDestroyer.transform.localScale.x, newTreeDestroyer.transform.localScale.z, dist);
					newTreeDestroyer.transform.LookAt (go.transform.position);
					newTreeDestroyer.transform.localScale = newScale;
				}
			}

			yield return new WaitForSeconds (0.1f);

			levelManager.DeleteTrees ();
		}

		StartCoroutine(ScanForStartWays ());
	}

	public IEnumerator ScanForStartWays()
	{
		List<GameObject> potDest = new List<GameObject>();

		if (levelManager.skierSpawners.Length >= 1) {
			foreach (GameObject go in levelManager.skierSpawners) {
				potDest.Add (go);
			}
		}

		yield return new WaitForSeconds (0.1f);

		foreach (GameObject go in potDest) {
			float dist = Vector3.Distance (go.transform.position, trailStart.transform.position);
			if (dist <= 250.0f) {
				GameObject newTreeDestroyer = Instantiate (treeDestroyerPrefab, trailStart.transform.position, trailStart.transform.rotation);
				Vector3 newScale = new Vector3 (newTreeDestroyer.transform.localScale.x, newTreeDestroyer.transform.localScale.z, dist);
				newTreeDestroyer.transform.LookAt (go.transform.position);
				newTreeDestroyer.transform.localScale = newScale;
			}
		}

		yield return new WaitForSeconds (0.1f);

		levelManager.DeleteTrees ();
	}

	public int ByDistanceFromTrailEnd(GameObject a, GameObject b)
	{
		var dstToA = Vector3.Distance(trailEnd.transform.position, a.transform.position);
		var dstToB = Vector2.Distance(trailEnd.transform.position, b.transform.position);
		return dstToA.CompareTo(dstToB);
	}

	public int ByDistanceFromTrailStart(GameObject a, GameObject b)
	{
		var dstToA = Vector3.Distance(trailStart.transform.position, a.transform.position);
		var dstToB = Vector2.Distance(trailStart.transform.position, b.transform.position);
		return dstToA.CompareTo(dstToB);
	}

}
