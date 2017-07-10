using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace TycoonTerrain {
    public class UIBuildingInfoWindowBehaviour : MonoBehaviour {
        public Text txtName;
        public Text txtConsumtion;
        public Text txtProduction;
        public Toggle rdoProductionActive;

        SimpleBuildingBehaviour currentBuilding = null;
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        public void ShowBuildingInfo(SimpleBuildingBehaviour building) {
            currentBuilding = null;
            txtName.text = building.buildingType.name;
            var productionScript = building.GetComponent<ResourceGeneratorBehaviour>();
            if (productionScript) {
                txtProduction.text = productionScript.resourceGenerated.ToString();
                txtConsumtion.text = productionScript.resourceConsumed.ToString();
                rdoProductionActive.isOn = productionScript.isActive;
                rdoProductionActive.gameObject.SetActive( true);
            } else {
                txtProduction.text = "";
                txtConsumtion.text = "";
                rdoProductionActive.gameObject.SetActive(false);
            }
            gameObject.SetActive(true);
            currentBuilding = building;
        }

        public void OnProductionChange(bool enabled) {
            if (currentBuilding != null)
                currentBuilding.GetComponent<ResourceGeneratorBehaviour>().isActive = enabled;
        }
    }
}