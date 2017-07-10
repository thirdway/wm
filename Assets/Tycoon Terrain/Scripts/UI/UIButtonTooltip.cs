using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TycoonTerrain{
	public class UIButtonTooltip : MonoBehaviour {
		public ScriptableObject tooltipObject;
		public Text txtTooltip;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		public void OnMouseEnter(){
			txtTooltip.gameObject.SetActive(true);
			txtTooltip.text = tooltipObject.ToString();
			txtTooltip.transform.GetChild(0).GetComponent<Text>().text = tooltipObject.ToString();
			//txtTooltip.GetComponentInChildren<Text>().text = tooltipObject.ToString();
			
		}

		public void OnMouseExit(){
			txtTooltip.gameObject.SetActive(false);
		}
	}
}