using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TycoonTerrain {
    [RequireComponent(typeof( SimpleBuildingBehaviour))]
	public class ResourceGeneratorBehaviour : MonoBehaviour {
        public bool isActive = true;
        public ResourceInstance resourceGenerated;
        public ResourceInstance resourceConsumed;
        public float interval = 5;
        private SimpleBuildingBehaviour building;
		// Use this for initialization
		void Start () {
            building = GetComponent<SimpleBuildingBehaviour>();
            if(building.enabled)
                StartCoroutine(GenerateResources());
		}
		
		// Update is called once per frame
		void Update () {
		
		}

        IEnumerator GenerateResources() {
            yield return new WaitForSeconds(Random.Range(0, interval));//Just to make sure not everyone cashes in at once
            while (true) {
                if (isActive && building.buildingInstanceData.buildingConstructionState == SimpleBuildingInstance.BuildingConstructionState.FINISHED) {
                    if (resourceConsumed.amount == 0 || ResourceManager.Instance.CanAfford(resourceConsumed)) {
                        if (resourceConsumed.amount != 0) {
                            ResourceManager.Instance.Pay(resourceConsumed);
                            FloatingTextManager.Instance.AddText(transform.position + Vector3.up, "-" + resourceConsumed.ToString(), 2 * Vector3.up, 3f, Color.red);
                        }
                        ResourceManager.Instance.Add(resourceGenerated);
                        FloatingTextManager.Instance.AddText(transform.position, resourceGenerated.ToString(), 2 * Vector3.up, 3f, Color.green);
                    }
                    
                }
                yield return new WaitForSeconds(interval);
            }
        }
	}
}