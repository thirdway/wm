using UnityEngine;
using System.Collections;
using System;

namespace TycoonTerrain{
	public class SimpleBuildingBehaviour : MonoBehaviour {

		//Set these in case you want the building to change its looks in different states. If you don't want it to, then
		//just leave them as null
        [Tooltip("A reference to the child game object that you want to represent the building when it is planned")]
		public GameObject plannedBuilding = null;
        [Tooltip("A reference to the child game object that you want to represent the building when it is under construction")]
        public GameObject underConstructionBuilding= null;
        [Tooltip("A reference to the child game object that you want to represent the building when it is finished")]
        public GameObject finishedBuilding= null;
        [Tooltip("A reference to the child game object that you want to represent the building when it is razed")]
        public GameObject razedBuilding= null;

		[HideInInspector]
		public int width;
		[HideInInspector]
		public int length;
		[HideInInspector]
		public int positionKey;
		[HideInInspector]
		public SimpleBuildingType buildingType;
		[System.NonSerialized]
		public SimpleBuildingInstance buildingInstanceData;
		/// <summary>
		/// Set to true if this is a building placed in a scene and not constructed by player
		/// </summary>
		[Tooltip("Set to true if this is a building placed in a scene and not constructed by player")]
		public bool isSceneBuilding = false;

        [HideInInspector]
		public WorldBehaviour world;
		protected bool hasBeenSetup = false;

		#region Action delegates
		public Action<GameObject> OnBuildingFinishedConstruction;
		public Action<GameObject, SimpleBuildingInstance.BuildingConstructionState> OnBuildingChangedState;
		#endregion Action delegates
		// Use this for initialization
		protected virtual void Start () {
		
		}
		
		public virtual void Setup(){
			hasBeenSetup = true;
			//Note that this will not work if you have several players!


			SimpleBuildingManager.Instance.RegisterBuilding(this);
			world = FindObjectOfType<WorldBehaviour>();

			switch (buildingInstanceData.buildingConstructionState) {
			case SimpleBuildingInstance.BuildingConstructionState.PLANNED:
				buildingInstanceData.amountLeftOnConstruction = buildingType.timeToBuild;
				StartCoroutine(ConstructBuilding());
				break;
			case SimpleBuildingInstance.BuildingConstructionState.BUILDING:
				StartCoroutine(ConstructBuilding());
				break;
			case SimpleBuildingInstance.BuildingConstructionState.FINISHED:
				if(OnBuildingFinishedConstruction != null)
					OnBuildingFinishedConstruction(gameObject);
				break;
			case SimpleBuildingInstance.BuildingConstructionState.RAZED:
				//TODO: Add whatever should happen
				break;
			default:
				break;
			}
			SetNewConstructionState(buildingInstanceData.buildingConstructionState);
		}

        public void HandleBuildingClicked() {
            Debug.Log(name + " was clicked");
        }

		#region Construction of building

		/// <summary>
		/// Construct the specified amount on the building.
		/// If the value for building construction left reaches zero then the building enters finished state
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void Construct(float amount){
			buildingInstanceData.amountLeftOnConstruction -= amount;
			if(buildingInstanceData.amountLeftOnConstruction <= 0){
				SetNewConstructionState(SimpleBuildingInstance.BuildingConstructionState.FINISHED);
			}
		}


		/// <summary>
		/// Constructs the building over time. 
		/// 
		/// </summary>
		/// <returns>The building.</returns>
		protected IEnumerator ConstructBuilding(){
			float waitAmount = 0.1f;
			WaitForSeconds wait = new WaitForSeconds( waitAmount);
			
			SetNewConstructionState(SimpleBuildingInstance.BuildingConstructionState.BUILDING);
			while(this != null && buildingInstanceData.amountLeftOnConstruction > 0){
				Construct(waitAmount);
				yield return wait;
			}
		}
		
		protected void SetNewConstructionState(SimpleBuildingInstance.BuildingConstructionState newState){
			//In order to render the building
			if(plannedBuilding != null)
				plannedBuilding.SetActive(false);
			if(underConstructionBuilding != null)
				underConstructionBuilding.SetActive(false);
			if(finishedBuilding != null)
				finishedBuilding.SetActive(false);
			if(razedBuilding != null)
				razedBuilding.SetActive(false);

			//First handle leaving current state
			switch (buildingInstanceData.buildingConstructionState) {
			case SimpleBuildingInstance.BuildingConstructionState.PLANNED:

				break;
			case SimpleBuildingInstance.BuildingConstructionState.BUILDING:

				break;
			case SimpleBuildingInstance.BuildingConstructionState.FINISHED:

				break;
			case SimpleBuildingInstance.BuildingConstructionState.RAZED:

				break;
			default:
				break;
			}
			
			//Handle entering new state
			switch (newState) {
			case SimpleBuildingInstance.BuildingConstructionState.PLANNED:
				if(plannedBuilding != null)
					plannedBuilding.SetActive(true);
				break;
			case SimpleBuildingInstance.BuildingConstructionState.BUILDING:
				if(underConstructionBuilding != null)
					underConstructionBuilding.SetActive(true);
				break;
			case SimpleBuildingInstance.BuildingConstructionState.FINISHED:
				if(finishedBuilding != null)
					finishedBuilding.SetActive(true);
				if(buildingInstanceData.buildingConstructionState == SimpleBuildingInstance.BuildingConstructionState.BUILDING){
					if(OnBuildingFinishedConstruction != null)
						OnBuildingFinishedConstruction(gameObject);
				}
				
				break;
			case SimpleBuildingInstance.BuildingConstructionState.RAZED:
				if(razedBuilding != null)
					razedBuilding.SetActive(true);
				break;
			default:
				break;
			}
			buildingInstanceData.buildingConstructionState = newState;

			//Fire event if there are subscribers
			if(OnBuildingChangedState != null)
				OnBuildingChangedState(gameObject, newState);
		}
		#endregion Construction of building
	}
}