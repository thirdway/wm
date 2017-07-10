using UnityEngine;
using System.Collections.Generic;

namespace TycoonTerrain{
	public interface IPlayerController {
		void SetBuildingType(SimpleBuildingType buildingType);
		/// <summary>
		/// Gets the available building types. These are the one that have been unlocked and can be built so if you have a researchtree 
		/// this is the list that are researched.
		/// </summary>
		/// <value>The available building types.</value>
		List<SimpleBuildingType> AvailableBuildingTypes{get;}
	}
}