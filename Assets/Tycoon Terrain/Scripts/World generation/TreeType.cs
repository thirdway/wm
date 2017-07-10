using UnityEngine;
using System.Collections;

namespace TycoonTerrain{

    /// <summary>
    /// This class defines the data of a certain tree type. Note that the tree prefab needs to be added to the terrain object and this tree type must be added to 
    /// </summary>
    [CreateAssetMenu(fileName = "New tree type.asset", menuName = "Tycoon Terrain/Create new tree type")]
    public class TreeType : ScriptableObject {
        [Tooltip("The index of this tree in the Unity Terrain object. The Unity Terrain needs to set up each tree so it can represent it graphically in the terrain. The World Behaviour will when generating the world use each of its Tree Types and tell the terrain to show them.")]
        public int treePrototypeIndex = 0;
        [Tooltip("The base scale for this tree. This can be left as 1 unless you want for example a giant version of a tree to exist as its own type or something similar.")]
        public float scale = 1f;
        [Tooltip("This is used in the tree generation process of the world generation to set the base light of the tree.")]
        public float baseColorIntensity = 0.8f;
        [Tooltip("This is used in the tree generation process of the world generation to set how much individual color of trees should vary.")]
        public float colorVariation = 0.2f;
        [Tooltip("The lowest height above the sea conted in step that this tree can grow.")]
        public int lowestHeightOverSea = 0;
        [Tooltip("The highest height above the sea that this tree can grow.")]
        public int highestHeightOverSea = 10;
        [Tooltip("A bonus value added to a tree that is in an optimal height for its settings above.")]
        public float optimalHeightBonus = 0.5f;

        /// <summary>
        /// Calculates how optimal a height can be considered to be for this tree type
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
		public float GetOptimality(float height){
			float dist = Mathf.Max(height - lowestHeightOverSea, highestHeightOverSea - height);
			if(dist > (highestHeightOverSea - lowestHeightOverSea)/2f){
				return 0;
			}else{
				return dist / (highestHeightOverSea - lowestHeightOverSea) / 2f;  
			}
		}
	}
}