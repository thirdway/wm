using UnityEngine;
using System.Collections.Generic;

namespace TycoonTerrain{
	public class FloatingTextManager : Singleton<FloatingTextManager> {

		public GameObject floatingTextPrefab;

		public Queue<UIFloatingTextBehaviour> floatingTextQueue = new Queue<UIFloatingTextBehaviour>();

		protected FloatingTextManager(){

		}


		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void AddText(Vector3 position, string text, Vector3 velocity, float timeToLive, Color color){
			UIFloatingTextBehaviour textBehaviour;

			if(floatingTextQueue.Count == 0 || floatingTextQueue.Peek().gameObject.activeInHierarchy){
				GameObject newText = (GameObject)GameObject.Instantiate(floatingTextPrefab);
				newText.transform.SetParent(transform);
				newText.transform.localScale = Vector3.one;
				textBehaviour = newText.GetComponent<UIFloatingTextBehaviour>();
			}else{
				textBehaviour = floatingTextQueue.Dequeue();
			}
			textBehaviour.Setup( position, text, velocity, timeToLive, color);

			floatingTextQueue.Enqueue(textBehaviour);
		}

		public void AddText(Vector3 position, string text, Vector3 velocity, float timeToLive){
			AddText(position, text, velocity, timeToLive, Color.yellow);
		}

	}
}