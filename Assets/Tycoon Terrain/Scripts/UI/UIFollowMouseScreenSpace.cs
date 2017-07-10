using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TycoonTerrain{
	public class UIFollowMouseScreenSpace : MonoBehaviour {

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			((RectTransform)transform).position = Input.mousePosition;
		}
	}
}