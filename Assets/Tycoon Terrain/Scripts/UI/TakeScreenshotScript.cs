using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
	public class TakeScreenshotScript : MonoBehaviour {
		static int noOfScrShots = 0;
		public string fileName ="screenshot";
		public System.Action OnScreenshotFinished;
		public KeyCode screenshotKey = KeyCode.F10;

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	#if UNITY_EDITOR
		void LateUpdate(){
			if(Input.GetKeyDown(KeyCode.F10))
				OnScreenshot();
		}

		public void OnScreenshot(){
		
			/*Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			tex.ReadPixels(new Rect(0,0,Screen.width, Screen.height), 0,0);
			tex.Apply();

			byte[] bytes = tex.EncodeToPNG();
			Destroy(tex);

			string filename = Application.dataPath + "\\screenshot" +(noOfScrShots++).ToString() +".png"; 
			Debug.Log(filename);
			System.IO.File.WriteAllBytes(filename, bytes);*/


			string filename = Application.dataPath + "\\" +fileName + (noOfScrShots++).ToString() +".png"; 
			Application.CaptureScreenshot(filename);
			Debug.Log(filename);

			if(OnScreenshotFinished != null)
				OnScreenshotFinished();
		}
	#endif
	}
}