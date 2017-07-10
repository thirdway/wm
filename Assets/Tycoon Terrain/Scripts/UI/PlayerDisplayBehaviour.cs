using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
    public class PlayerDisplayBehaviour : MonoBehaviour{

        public GameObject arrowUp;
        public GameObject arrowDown;
        public GameObject imgBomb;
        public GameObject imgLevel;
        public GameObject imgTurnArrows;
        [HideInInspector]
        public GameObject tmpBuildObject;
        BasicPlayerController.TerraFormType currentTerraform;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        public void SetTerraformType(BasicPlayerController.TerraFormType newType) {
            arrowUp.SetActive(false);
            arrowDown.SetActive(false);
            imgBomb.SetActive(false);
            imgLevel.SetActive(false);
            imgTurnArrows.SetActive(false);

            if (tmpBuildObject != null)
                Destroy(tmpBuildObject);

            currentTerraform = newType;

            switch (currentTerraform) {
                case BasicPlayerController.TerraFormType.BULLDOZE:
                    arrowDown.SetActive(true);
                    imgBomb.SetActive(true);
                    break;
                case BasicPlayerController.TerraFormType.BULLDOZE_AREA:
                    arrowDown.SetActive(true);
                    imgBomb.SetActive(true);
                    break;
                case BasicPlayerController.TerraFormType.RAISE_TERRAIN:
                    arrowUp.SetActive(true);
                    break;
                case BasicPlayerController.TerraFormType.LOWER_TERRAIN:
                    arrowDown.SetActive(true);
                    break;
                case BasicPlayerController.TerraFormType.LEVEL_TERRAIN:
                    arrowDown.SetActive(true);
                    imgLevel.SetActive(true);
                    break;
                case BasicPlayerController.TerraFormType.BUILD:
                    arrowDown.SetActive(true);
                    imgTurnArrows.SetActive(true);

                    break;
                default:
                    break;
            }
        }
    }
}