using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TycoonTerrain{
	[RequireComponent(typeof(Image))]
	public class UIMeterBehaviour : MonoBehaviour {

		private float originalWidth;
		private RectTransform imageTransform;

		public float initialFillRatio = 1.0f;
		public bool removeIfZero = true;

		void Awake(){
			imageTransform = GetComponent<RectTransform>();
			originalWidth = imageTransform.rect.width;
			SetFillRatio(initialFillRatio);
		}
		// Use this for initialization
		void Start () {

		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void SetFillRatio(float newRatio){
			if(removeIfZero && newRatio <= 0){
				transform.parent.gameObject.SetActive(false);
			}
			imageTransform.sizeDelta = new Vector2(originalWidth * newRatio, imageTransform.rect.height);
		}
	}
}