using UnityEngine;
using System.Collections;


namespace TycoonTerrain{
	public class TurnToCamera : MonoBehaviour {
		public enum TurnToCameraType{
			TURN_TO_CAMERA_CENTER, //Aims at camera, 
			TURN_TO_CAMERA_PLANE, //Sets to the camera plane thus making it look better for ui

		}
        public bool doOnlyTurnYAxis = false;
		public TurnToCameraType turnType;
		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
			switch (turnType) {
			case TurnToCameraType.TURN_TO_CAMERA_CENTER:
				transform.LookAt(Camera.main.transform, Vector3.up);
				break;
			case TurnToCameraType.TURN_TO_CAMERA_PLANE:
				transform.up =  Camera.main.transform.up;
				transform.forward = Camera.main.transform.forward;

				break;
			default:
				break;
			}

            if (doOnlyTurnYAxis)
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
			//Vector3 rot = transform.rotation.eulerAngles;
			//transform.rotation = Quaternion.Euler(0, rot.y, 0);
		}
	}
}