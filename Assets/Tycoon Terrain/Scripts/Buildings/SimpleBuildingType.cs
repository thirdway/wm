using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TycoonTerrain {
    [CreateAssetMenu(fileName = "New building type.asset", menuName = "Tycoon Terrain/Create new building type")]
	public class SimpleBuildingType : ScriptableObject {
		
		[System.Serializable]
		public class TileTypeArea{
			public int width = 1;
			public int length = 1;
			public MapTile.MapTileType[] tiles = new MapTile.MapTileType[1];
			
			public  MapTile.MapTileType GetTileType(int x, int y){
				return tiles[x + y * width];
			}
		}

		[Tooltip("In case you want to use one prefab several times but at different sizes then you can set their individual scales with this variable. Otherwise you are better off setting the scale of the prefab directly.")]
		public Vector3 scale = Vector3.one;
		[Tooltip("The time in seconds to finish construction of building.")]
		public float timeToBuild = 10;
		[Tooltip("Set the prefab you want to represent this building in the world. Note that the prefab should have the SimpleBuildingBehaviour script attached.")]
		public GameObject prefab;
		[Tooltip("Use this to add a description that will be shown when hovering the build button in the UI for this building.")]
		public string description = "";
		[Tooltip("How much it costs to construct this building.")]
		public List<ResourceInstance> constructionCost;
		/// <summary>
		/// The build area that is accepted by the building.
		/// Most of the time you probably want it to be flat but in case you need leaning terrain or oceans
		/// for some or all tiles you can set that here.
		/// </summary>
		[Tooltip(@"The build area that is accepted by the building.
	 	Most of the time you probably want it to be flat but in case you need leaning terrain or oceans
		for some or all tiles you can set that here.")]
		public TileTypeArea buildArea;

		/// <summary>
		/// You only need to set this in case you allow tiles to be leaning for this building.
		/// 
		/// A case where this would be set to true is in case you want to build roads up a slope. Then those roads should be leaning 
		/// with the terrain and the variable should be set to true
		/// 
		/// If you have a house standing on a slope you probably don't want it to be leaning with the terrain so set to false.
		/// </summary>
		[Tooltip(@"You only need to set this in case you allow tiles to be leaning for this building. 
		 
		 A case where this would be set to true is in case you want to build roads up a slope. Then those roads should be leaning 
		 with the terrain and the variable should be set to true
		 
		 If you have a house standing on a slope you probably don't want it to be leaning with the terrain so set to false.")]
		public bool doLeanWithTerrain = false;

		public override string ToString (){
			return name +
                "\nSize: " + buildArea.width.ToString() + "x" + buildArea.length.ToString() + 
                GetConstructionCostString() +
                "\n" + description;
		}

        private string GetConstructionCostString() {
            if (constructionCost.Count == 0)
                return "";
            string costStr = "\n";
            foreach (var item in constructionCost) {
                costStr += item.ToString() + "\n";
            }
            return costStr;
        }
	}
}