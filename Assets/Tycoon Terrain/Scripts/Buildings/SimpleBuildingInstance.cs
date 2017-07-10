using UnityEngine;
using System.Collections.Generic;

namespace TycoonTerrain{
	/// <summary>
	/// Building instance data that is saved between sessions
	/// </summary>
	[System.Serializable]
	public class SimpleBuildingInstance {
		public enum BuildingConstructionState{
			PLANNED,
			BUILDING,
			FINISHED,
			RAZED,
		}

		/// <summary>
		/// The key for this building base on its position. For this to work no two buildings should have the exakt same x and y coords.
		/// If you really need several buildings with the same coords (really really..??) then you need to calculate a unique key some other way...
		/// Maybe the level of stacking or whatever. These changes need to be reflected in the BuildingManager as well
		/// </summary>
		public int key;
		public int x;
		public int y;
		public int direction = 0;
		public int buildingTypeIndex;
		public float amountLeftOnConstruction;
		public BuildingConstructionState buildingConstructionState = BuildingConstructionState.PLANNED;



		/* Add any more data you'd like to store here. It could be hitpoints, or current number of employees or whatever
		 * is specific for one individual building
		 */


		public SimpleBuildingInstance(){}
		
		public SimpleBuildingInstance(int direction, int buildingIndex){
			this.direction = direction;
			this.buildingTypeIndex = buildingIndex;
		}



	}
}