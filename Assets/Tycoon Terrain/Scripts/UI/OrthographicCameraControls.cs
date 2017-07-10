using UnityEngine;
using System.Collections;

public class OrthographicCameraControls : MonoBehaviour {
    // Default unity names for mouse axes
    public string mouseHorizontalAxisName = "Mouse X";
    public string mouseVerticalAxisName = "Mouse Y";
    public string scrollAxisName = "Mouse ScrollWheel";
    public Camera[] affectedOrtographicCameras;

    float scrollSensitivity = 10;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
        float speed = 10f;
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        Vector3 right = transform.right;

        if (Input.GetKey(KeyCode.A)) {
            transform.Translate(-right * Time.deltaTime * speed, Space.World);
        }
        if (Input.GetKey(KeyCode.S)) {
            transform.Translate(-forward * Time.deltaTime * speed, Space.World);
        }
        if (Input.GetKey(KeyCode.D)) {
            transform.Translate(right * Time.deltaTime * speed, Space.World);
        }
        if (Input.GetKey(KeyCode.W)) {
            transform.Translate(forward * Time.deltaTime * speed, Space.World);
        }

        if (Input.GetMouseButton(2)) {
            float translateX = -Input.GetAxis(mouseHorizontalAxisName) * speed;
            transform.Translate(translateX * right, Space.World);

            float translateZ = -Input.GetAxis(mouseVerticalAxisName) * speed;
            transform.Translate(translateZ * forward, Space.World);
        }

        Camera.main.orthographicSize -= Input.GetAxis(scrollAxisName) * scrollSensitivity;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 5, 200);
        foreach (Camera cam in affectedOrtographicCameras) {
            cam.orthographicSize = Camera.main.orthographicSize;
        }
    }
}
