using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TycoonTerrain{
	public class UIBuildButtonBehaviour : MonoBehaviour {
		private SimpleBuildingType buildingType;
		private IPlayerController player;
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void Setup(SimpleBuildingType buildingType, IPlayerController player, Text txtTooltip){
			this.buildingType = buildingType;
			this.player = player;

			gameObject.name = buildingType.name;
			GetComponentInChildren<Text>().text = buildingType.name;

			UIButtonTooltip tooltip = GetComponent<UIButtonTooltip>();
			tooltip.tooltipObject = buildingType;
			tooltip.txtTooltip = txtTooltip;

			GetComponent<Button>().onClick.AddListener(HandleClick);
		}

		public void HandleClick(){
			player.SetBuildingType(buildingType);
		}


	}
}