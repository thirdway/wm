using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace TycoonTerrain{
	public class UIWorldSizePanelBehaviour : MonoBehaviour {

		public Text txtSize;
		public WorldBehaviour world;
        public BasicPlayerController player;
		int currentSizeExponent = 6;
		int minSizeExponent = 5;
		int maxSizeExponent = 9;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void OnIncreaseSize(){
			currentSizeExponent = Mathf.Clamp(currentSizeExponent+1, minSizeExponent, maxSizeExponent);
			world.worldData.heightMapSize = (int)Mathf.Pow(2, currentSizeExponent) + 1;
			txtSize.text = (world.worldData.heightMapSize - 1).ToString() + "x" + (world.worldData.heightMapSize - 1).ToString();
            player.ResetGame();
		}

		public void OnDecreaseSize(){
			currentSizeExponent = Mathf.Clamp(currentSizeExponent-1, minSizeExponent, maxSizeExponent);
			world.worldData.heightMapSize = (int)Mathf.Pow(2, currentSizeExponent) + 1;
			txtSize.text = (world.worldData.heightMapSize - 1).ToString() + "x" + (world.worldData.heightMapSize - 1).ToString();
            player.ResetGame();
        }
	}
}