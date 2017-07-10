using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
	[RequireComponent(typeof(UIMeterBehaviour))]
	public class UIConstructionMeterBehaviour : MonoBehaviour {

		private SimpleBuildingBehaviour building;
		
		private UIMeterBehaviour meter;
		// Use this for initialization
		void Start () {
			building = GetComponentInParent<SimpleBuildingBehaviour>();
			meter = GetComponent<UIMeterBehaviour>();
		}
		
		// Update is called once per frame
		void Update () {
			meter.SetFillRatio(1f - building.buildingInstanceData.amountLeftOnConstruction / building.buildingType.timeToBuild);
		}
	}
}