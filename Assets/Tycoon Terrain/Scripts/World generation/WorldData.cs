using UnityEngine;
using System.Collections.Generic;

namespace TycoonTerrain{

	[System.Serializable]
	public class TreeInstanceData{
		public TreeInstance tree;
		public int key;
	}
	/// <summary>
	/// World data that is saved between sessions
	/// </summary>
	[System.Serializable]
	public class WorldData{

        //TODO: This is untested! Don't play around with this just yet!
        [Tooltip(@"WARNING: This is failry untested at this point! Change this to change the height change of a tile, i.e. the angle of slopes")]
        public float heightPerTile = 1f;
		/// <summary>
		/// The width of the tile. Note that if you change this and you are using map tile textures with squares on them then you need to also change
		/// the tile texture settings in the unity terrain object to correspond with the new values
		/// </summary>
		[Tooltip(@"The width of the tile. Note that if you change this and you are using map tile textures with squares on them then you need to also change the tile texture settings in the unity terrain object to correspond with the new values")]
		public int tileWidth = 2;
		[Tooltip(@"The number of tiles to each side on the map +1. I.e. if you want a map with 32x32 tiles then set this value to 33. Only powers of 2 + 1 will be accepted as that is needed by the diamond square algorithm that generates the heightmap.")]
		public int heightMapSize = 33;
		[Tooltip(@"How height values are generated inside the diamond-square algorithm. The produce different types of hills and also different heights of them so experiment with this and the hillyness value before deciding exactly what is the best setup for you.")]
		public MapGenerator.RandomDistributionType heightRandomType = MapGenerator.RandomDistributionType.LINEAR;
		[Tooltip("Higher values gives more hills. Note that also the height random type affects how hilly it gets with linear random type giving the most hills. You may need to experiment with both these a bit to get the types of hills you want.")]
		public float hillyness = 1f;
		[Range(0.0f, 1.0f), Tooltip("0 gives no trees. 1 gives lots of trees")]
		public float treeyness = 0.5f;
		[Range(0.0f, 1.0f), Tooltip("0 gives no water. 1 gives only water. Did someone say... Waterworld??!1!")]
		public float wateryness = 0.5f;

		[Range(0.0f, 1.0f), Tooltip("Affects both trees and grass. 0 for a barren wasteland with neither, 1 for all green")]
		public float soilFertility = 0.5f;
		[Tooltip("The time in seconds for a tile to regrow its grass after being terraaformed")]
		public float timeToRegrowGrass = 20f;

		[Range(0.0f, 1.0f), Tooltip("The chance every second that a tree will start to regrow given that the circumstances allow it.")]
		public float treeRegrowChance = 0.1f;
		[Tooltip("The speed at which a tree regrows after it has started regrowing.")]
		public float treeRegrowthRate = 0.1f;
		[Range(0.0f, 1.0f), Tooltip("Set this to a value between 0 and 1. If set at 0 then trees will regrow even if there are no trees adjacent. If set to 1 the they will only regrow if every adjacent tile has a fully grown tree.")]
		public float minimumAdjacentTreeSizeForRegrowth = 0.1f;
		[Tooltip("The maximum height at which soil will be considered fertile for grass to grow.")]
		public float maxHeightForSoilFertility = 5f;

		[System.Xml.Serialization.XmlIgnore]
		public float[,] heightMapFloats;
		[System.Xml.Serialization.XmlIgnore]
		public int[,] heightMap;
		
		/// <summary>
		/// A workaround to xml serialization not supporting multidimensional arrays (unless we use jagged arrays which we don't want)
		/// This will cause some garbage though but unless you don't use it too often it shouldn't be a problem. 
		/// </summary>
		/// <value>The height map serialization.</value>
		public int[] heightMapSerialization{
			get{
				int[] serializedHeightMap = new int[heightMapSize * heightMapSize];
				for (int i = 0; i < heightMapSize * heightMapSize; i++) {
					serializedHeightMap[i] = heightMap[i%heightMapSize, i/heightMapSize];
				}
				
				return serializedHeightMap;
			}
			
			set{
				heightMap = new int[heightMapSize, heightMapSize];
				for (int i = 0; i < heightMapSize * heightMapSize; i++) {
					heightMap[i%heightMapSize, i/heightMapSize] = value[i];
				}
			}
		}

		public int soilFertilityRandomSeed;
        public int treeMapsRandomSeed;

        [System.Xml.Serialization.XmlIgnore]
		public float[,] soilFertilityMap;

		[System.Xml.Serialization.XmlIgnore]
		public List<float[,]> treeMaps = new List<float[,]>();
		
		/// <summary>
		/// We keep a hashset of the coordinate keys of trees that have been removed
		/// This way we kan make them start growing again and we also know where not to place treesw when reloading map
		/// </summary>
		public HashSet<int> removedTrees = new HashSet<int>();
		
		public float[] treeMapSerialization{
			get{
				float[] serializedTreeMaps = new float[(heightMapSize-1) * (heightMapSize-1) * treeMaps.Count];
				
				for (int j = 0; j < treeMaps.Count; j++) {
					float[,] map = treeMaps[j];
					
					for (int i = 0; i < (heightMapSize-1) * (heightMapSize-1); i++) {
						serializedTreeMaps[i + (heightMapSize-1) * (heightMapSize-1) * j] = map[i%(heightMapSize-1), i/(heightMapSize-1)];
					}
					
				}
				return serializedTreeMaps;
			}
			
			set{
				treeMaps = new List<float[,]>();
				int noOfMaps = value.Length / ((heightMapSize-1) * (heightMapSize-1));
				
				for (int j = 0; j < noOfMaps; j++) {
					float[,] deSerializedTreeMap = new float[(heightMapSize-1), (heightMapSize-1)];
					
					for (int i = 0; i < value.Length / noOfMaps; i++) {
						deSerializedTreeMap[i%(heightMapSize-1), i/(heightMapSize-1)] = value[i + j * (heightMapSize-1) * (heightMapSize-1)];							
					}	
					
					treeMaps.Add(deSerializedTreeMap);
				}
			}
		}

		[HideInInspector]
		public int waterLevel = 0;
	}
}