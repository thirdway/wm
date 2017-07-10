using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VikingCrewDevelopment{
	public class ScaleToCamDistance : MonoBehaviour {
        public float minScale = 0.1f;
        public float maxScale = 10f;
        public float nominalSize = 32f;
        public float minDistance = 16f;
        public float maxDistance = 512f;
        
        // Use this for initialization
        void Start () {
		    
		}
		
		// Update is called once per frame
		void Update () {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

            float s = Mathf.InverseLerp(minDistance, maxDistance, distance);
            float scale = nominalSize * Mathf.Lerp(minScale, maxScale, s);
            transform.localScale = Vector3.one * scale;
		}
	}
}
