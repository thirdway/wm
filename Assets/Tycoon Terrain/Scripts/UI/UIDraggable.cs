using UnityEngine;
using System.Collections;

using UnityEngine.EventSystems;
namespace TycoonTerrain{
	public class UIDraggable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		
		public GameObject subscriber;
		public System.Action<Vector3, int> onDragDelegate;
		public int id;
		public string msgStr = "OnDrag";

        bool isDragging = false;
		RenderMode renderMode;
		Vector3 pointerOldPos;
		// Use this for initialization
		void Start () {
			renderMode = GetComponentInParent<Canvas>().renderMode;
		}
		
		// Update is called once per frame
		void Update () {
			if(isOver && Input.GetMouseButtonDown(0) ){
				pointerOldPos = Input.mousePosition;
                isDragging = true;
			}
            if (isDragging && isOver && Input.GetMouseButton(0))
                OnDrag();
            else
                isDragging = false;
            
        }

        void OnDisable() {
            isOver = false;
        }

        public bool isOver = false;



        public void OnPointerEnter(PointerEventData eventData) {
            Debug.Log("Mouse enter");
            isOver = true;
        }

        public void OnPointerExit(PointerEventData eventData) {
            Debug.Log("Mouse exit");
            isOver = false;
        }
        public void OnDrag(){
			
			Vector3 drag = Vector3.zero;
			
			switch (renderMode) {
			case RenderMode.ScreenSpaceOverlay:
				drag = Input.mousePosition - pointerOldPos;
				transform.position += drag;
				pointerOldPos = Input.mousePosition;
				
				break;
				
			case RenderMode.WorldSpace:
				Vector3 oldPos = transform.position;
				Vector3 newPos = Input.mousePosition;
				newPos.z = oldPos.z - Camera.main.transform.position.z;
				//newPos = Camera.main.ScreenToWorldPoint(newPos);
				//Debug.Log(Input.mousePosition);
				//Debug.Log(Camera.main.ScreenToWorldPoint(newPos));
				newPos = Camera.main.ScreenToWorldPoint(newPos);
				newPos.z = oldPos.z;
				transform.position = newPos;
				drag= newPos - oldPos;
				break;
				
			default:
				break;
			}
			
			
			
			
			
			if(subscriber != null)
				subscriber.SendMessage(msgStr, drag, SendMessageOptions.RequireReceiver);
			if(onDragDelegate != null)
				onDragDelegate(drag, id);
		}
	}
}