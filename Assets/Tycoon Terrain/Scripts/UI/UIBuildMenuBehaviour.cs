using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TycoonTerrain{
	public class UIBuildMenuBehaviour : MonoBehaviour {
		public WorldBehaviour world;
		public GameObject playerGameobject;
		public GameObject buildingButtonPrefab;
		public Text txtTooltip;

		IPlayerController player;

		// Use this for initialization
		void Start () {
			//Find the player interface behaviour
			foreach (MonoBehaviour mono in playerGameobject.GetComponents(typeof(MonoBehaviour))){
				if (mono is IPlayerController){
					player = (IPlayerController) mono;
				}
			}
			SetupBuildButtons();
		}

		/// <summary>
		/// Creates and setups the build buttons for all buildings. In case you need a tech tree or similar then this 
		/// would be the place where you would now not instantiate buttons for buildings that are not invented yet.
		/// Then whenever a building is unlocked you could do similar to below.
		/// </summary>
		void SetupBuildButtons(){
			foreach (var building in player.AvailableBuildingTypes) {
				GameObject button = GameObject.Instantiate<GameObject>(buildingButtonPrefab);
				button.transform.SetParent(transform);
				button.GetComponent<UIBuildButtonBehaviour>().Setup(building, player, txtTooltip);
			} 
		}

		// Update is called once per frame
		void Update () {
		
		}
	}
}