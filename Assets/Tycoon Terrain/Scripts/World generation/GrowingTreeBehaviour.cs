using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
	public class GrowingTreeBehaviour : MonoBehaviour {
		public float regrowthRate = 0.1f;
		public float size = 0;
		public float maxSize = 1;
        public int key;
		public TreeInstance treeInstance;
        public System.Action<GrowingTreeBehaviour> OnFinishedGrowing;
		// Use this for initialization
		void Start () {
			maxSize = treeInstance.heightScale;
		}
		
		// Update is called once per frame
		void Update () {
			size = Mathf.Min (maxSize, size + Time.deltaTime * regrowthRate);
			transform.localScale = size * Vector3.one;

			if (size == maxSize) {
                OnFinishedGrowing(this);
			}
		}

		public float GrownRatio(){
			return size / maxSize;
		}
	}
}