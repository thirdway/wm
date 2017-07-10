using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamComp : MonoBehaviour {

	public LevelManager levelManager;
	public Camera cam;

	public Vector2 cursorPos;

	public bool isRotating;

	public bool canMoveLeft;
	public bool canMoveRight;
	public bool canMoveForward;
	public bool canMoveBack;

	public float camMoveSpeed;
	public float camRotSpeed;

	public Vector2 minMaxXPos;
	public Vector2 minMaxYPos;
	public Vector2 minMaxXRot;

	public GameObject centerPos;

	public int mouseSide;
	public float zoomSpeed;


	void Awake() {
		levelManager = GameObject.Find ("LevelManager").GetComponent<LevelManager> ();
		canMoveLeft = false;
		canMoveRight = false;
		canMoveForward = false;
		canMoveBack = false;
		isRotating = false;
	}
	
	void Update () {
		cursorPos = Input.mousePosition;
//		Debug.Log ("" + cursorPos + " / " + Screen.width);

		if (cursorPos.x <= 0) {
			canMoveLeft = true;
		}
		else if(cursorPos.x > 0)
		{
			canMoveLeft = false;
			if (cursorPos.x >= Screen.width) {
				canMoveRight = true;
			}
			else if (cursorPos.x < Screen.width) {
				canMoveRight = false;
			}
		}

		if (cursorPos.y <= 0) {
			canMoveBack = true;

		} else if (cursorPos.y > 0) {
			canMoveBack = false;
			if (cursorPos.y > Screen.height) {
				canMoveForward = true;
			} else if (cursorPos.y < Screen.height) {
				canMoveForward = false;
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			isRotating = true;

		}
		else if (Input.GetMouseButtonUp (1)) {
			isRotating = false;

		}
	}

	void LateUpdate()
	{
		if (isRotating == false) {
			if (canMoveLeft == true) {
				transform.Translate (Vector3.left * camMoveSpeed * Time.deltaTime);
			}
	
			if (canMoveRight == true) {
				transform.Translate (Vector3.right * camMoveSpeed * Time.deltaTime);
			}

			if (canMoveForward == true) {
				transform.Translate (Vector3.forward * camMoveSpeed * Time.deltaTime);
			}
	
			if (canMoveBack == true) {
				transform.Translate (Vector3.back * camMoveSpeed * Time.deltaTime);
			}

			if (transform.position.x < minMaxXPos.x) {
				transform.position = new Vector3 (minMaxXPos.x, transform.position.y, transform.position.z);
			}
			if (transform.position.x > minMaxXPos.y) {
				transform.position = new Vector3 (minMaxXPos.y, transform.position.y, transform.position.z);
			}
			if (transform.position.z < minMaxYPos.x) {
				transform.position = new Vector3 (transform.position.x, transform.position.y, minMaxYPos.x);
			}
			if (transform.position.z < minMaxYPos.y) {
				transform.position = new Vector3 (transform.position.x, transform.position.y, minMaxYPos.y);
			}	
		}
		else if(isRotating == true)
		{
			/*if(cursorPos.x < (Screen.width / 2))
			{
				mouseSide = -1;
			}
			else if(cursorPos.x > (Screen.width / 2))
			{
				mouseSide = 1;
			}
			transform.RotateAround (centerPos.transform.position, Vector3.up, mouseSide * camRotSpeed * Time.deltaTime);*/

			float h = camRotSpeed * Input.GetAxis("Mouse X");
		//	float v = camRotSpeed * Input.GetAxis("Mouse Y");
		//	transform.Rotate(v, h, 0);
			transform.RotateAround(centerPos.transform.position, Vector3.up, h);
		
		}

		if (Input.GetAxis ("Mouse ScrollWheel") < 0f) { // forward
			if (cam.orthographic == true) {
				if (cam.orthographicSize < 200) {
					cam.orthographicSize += zoomSpeed * Time.deltaTime;
				}
			}
			else if (cam.orthographic == false) {
				if (cam.fieldOfView < 200) {
					cam.fieldOfView += zoomSpeed * Time.deltaTime;
				}
			}
		} else if (Input.GetAxis ("Mouse ScrollWheel") > 0f) { // backwards
			if (cam.orthographic == true) {
				if (cam.orthographicSize > 50) {
					cam.orthographicSize -= zoomSpeed * Time.deltaTime;
				}
			}
			else if (cam.orthographic == false) {
				if (cam.fieldOfView > 50) {
					cam.fieldOfView -= zoomSpeed * Time.deltaTime;
				}
			}
		}
	}
}

