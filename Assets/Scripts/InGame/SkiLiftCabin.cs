using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLiftCabin : MonoBehaviour {

	public List<GameObject> wps;
	public GameObject currentTarget;
	public GameObject StartBase;
	public GameObject endBase;
	public LevelManager levelManager;
	public bool canMove;
	public float cabinSpeed = 12.0f;
	public int currentWpNumber = 1;
	public bool hasSkiers;
	public List<GameObject> currentLoadedSkiers;
	public int maxSkierCapacity = 10;
	public GameObject cabinModel;

	void Awake() {
		wps = new List<GameObject> ();
		canMove = false;
		levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager> ();
	}

	/*void Start()
	{
		StartCoroutine (GetWayPoints ());
	}

	public IEnumerator GetWayPoints()
	{
		wps = transform.parent.transform.gameObject.GetComponent<SkiLiftComp> ().wps;
		yield return new WaitForSeconds(0.05f);
//		protectedWps = wps;
	}*/

	void Update()
	{
		if(canMove == true)
		{
			transform.position = Vector3.MoveTowards (transform.position, currentTarget.transform.position, cabinSpeed * Time.deltaTime);
		}
	}

	public IEnumerator SetCabinOn()
	{
		yield return new WaitForSeconds (0.1f);
		currentTarget = wps [0];
		canMove = true;
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.transform.gameObject.tag == "SkiLiftWp")
		{
			if (other.transform.gameObject == currentTarget) {

				if (currentWpNumber < wps.Count - 1) {
					currentWpNumber += 1;
					currentTarget = wps [currentWpNumber];
				} else if (currentWpNumber >= wps.Count - 1) {
					currentWpNumber = 1;
					currentTarget = wps [currentWpNumber];
				}
				if(other.transform.gameObject.name == "SkiLiftStartRightAnchor" || other.transform.gameObject.name == "SkiLiftStartLeftAnchor")
				{
					if (hasSkiers == false) {
					//	if (other.transform.gameObject.GetComponent<EndStartType> ().endStart == 0) {
							other.transform.parent.transform.gameObject.GetComponent<SkiLiftStartComp> ().skiLiftPapa.GetComponent<SkiLiftComp> ().LoadSkiers (gameObject);
					//	}
						/*else if (other.transform.gameObject.GetComponent<EndStartType> ().endStart == 1) {
							other.transform.parent.transform.gameObject.GetComponent<SkiLiftEndComp> ().skiLiftPapa.GetComponent<SkiLiftComp> ().LoadSkiers (gameObject);
						}*/
					}
				}
				if(other.transform.gameObject.name == "SkiLiftEndRightAnchor" || other.transform.gameObject.name == "SkiLiftEndLeftAnchor")
				{
				//	if (other.transform.gameObject.GetComponent<EndStartType> ().endStart == 1) {
						if (hasSkiers == true) {
							UnloadSkiers (other.transform.gameObject);
						}
				//	}
				}
			}
		}
	}

	public void UnloadSkiers(GameObject col)
	{
		foreach (GameObject go in currentLoadedSkiers) {
			go.transform.parent = null;
			go.transform.position = col.transform.parent.transform.gameObject.GetComponent<SkiLiftEndComp> ().unloadPos.transform.position;
			col.transform.parent.transform.gameObject.GetComponent<SkiLiftEndComp> ().skiLiftPapa.GetComponent<SkiLiftComp> ().skiersToUnload.Add (go);
			go.GetComponent<SkierTryPrefab> ().IsOffOfSkiLift ();
		}
		hasSkiers = false;
	}
}
