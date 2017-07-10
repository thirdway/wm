using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TycoonTerrain{
	[RequireComponent(typeof(Text))]
	public class UIFloatingTextBehaviour : MonoBehaviour {

		public float timeToLive = 1f;
		public Vector3 velocity;
		Text _text = null;
		Text text{
			get{
				if(_text == null)
					_text = GetComponent<Text>();
				return _text;
			}
		}

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			timeToLive -= Time.deltaTime;
			transform.position =transform.position + Time.deltaTime * velocity;
			//transform.LookAt(Camera.main.transform);
			transform.rotation = Camera.main.transform.rotation;
			//transform.Rotate(0, 180, 0); 

			// When text is about to die start fadin out
			if(0 < timeToLive && timeToLive < 1){
				Color c = text.color;
				c.a = timeToLive;
				text.color = c;
			}
			if(timeToLive < 0)
				gameObject.SetActive(false);
		}

		public void Setup(Vector3 position, string strText, Vector3 velocity, float timeToLive, Color color){
			this.timeToLive = timeToLive;
			this.velocity = velocity;
			text.text = strText;
			transform.position = position;
			text.color = color;
			//transform.LookAt(Camera.main.transform);
			transform.rotation = Camera.main.transform.rotation;
			//transform.Rotate(0, 180, 0); 
			if(timeToLive > 0)
				gameObject.SetActive(true);
		}
	}
}