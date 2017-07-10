using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
	public class GridOverlay : MonoBehaviour {
	 
		public enum OverlayType{
			SQUARE,
			CROSS,
		}
		public bool isVisible = false;
		public float offsetY = 0.2f;
		public WorldBehaviour world;
		public Vector3 start;
		public Vector3 stop;
		public OverlayType overlayType = OverlayType.SQUARE;

		public Material lineMaterial;
		Color mainColor = new Color(1f,0f,0f,1f);

		void Start () 
		{

		}
        

		void OnPostRender() 
		{   
		 // set the current material
		 lineMaterial.SetPass( 0 );
		 
		 GL.Begin( GL.LINES );
		 
		 if(isVisible)
		 {
		     GL.Color(mainColor);

			#region draw gl lines
			Vector3 wsOffset = world.terrain.transform.position;
			Vector3 lsStart = start;
			Vector3 lsStop = stop;
				int startX =Mathf.RoundToInt( lsStart.x < lsStop.x ? lsStart.x :lsStop.x);
				int stopX = Mathf.RoundToInt( lsStart.x > lsStop.x ? lsStart.x :lsStop.x);

				int startZ =Mathf.RoundToInt( lsStart.z < lsStop.z ? lsStart.z :lsStop.z);
				int stopZ = Mathf.RoundToInt( lsStart.z > lsStop.z ? lsStart.z :lsStop.z);

				switch (overlayType) {
				case OverlayType.SQUARE:
					for (int x = startX; x < stopX; x++) {
						GL.Vertex3( x + wsOffset.x, world.GetHeight(x,startZ) + offsetY + wsOffset.y, startZ + wsOffset.z);
						GL.Vertex3( x+1+ wsOffset.x, world.GetHeight(x+1,startZ) + offsetY+ wsOffset.y, startZ + wsOffset.z);
						
						GL.Vertex3( x+ wsOffset.x, world.GetHeight(x,stopZ) + offsetY+ wsOffset.y, stopZ + wsOffset.z);
						GL.Vertex3( x+1+ wsOffset.x, world.GetHeight(x+1,stopZ) + offsetY+ wsOffset.y, stopZ + wsOffset.z);
					}
					
					for (int z = startZ; z < stopZ; z++) {
						GL.Vertex3( startX + wsOffset.x, world.GetHeight(startX,z) + offsetY+ wsOffset.y, z + wsOffset.z);
						GL.Vertex3( startX + wsOffset.x, world.GetHeight(startX,z+1) + offsetY+ wsOffset.y, z+1 + wsOffset.z);
						
						GL.Vertex3( stopX + wsOffset.x, world.GetHeight(stopX,z) + offsetY+ wsOffset.y, z + wsOffset.z);
						GL.Vertex3( stopX + wsOffset.x, world.GetHeight(stopX,z+1) + offsetY+ wsOffset.y, z+1 + wsOffset.z);
						
					}
					break;
				case OverlayType.CROSS:
					for (int x = startX; x < stopX; x++) {
						GL.Vertex3( x + wsOffset.x, world.GetHeight(x,(start.z + stop.z)/2) + offsetY+ wsOffset.y, (start.z + stop.z)/2 + wsOffset.z);
						GL.Vertex3( x+1 + wsOffset.x, world.GetHeight(x+1,(start.z + stop.z)/2) + offsetY+ wsOffset.y, (start.z + stop.z)/2 + wsOffset.z);

					}
					
					for (int z = startZ; z < stopZ; z++) {
						GL.Vertex3( (start.x + stop.x)/2  + wsOffset.x, world.GetHeight((start.x + stop.x)/2,z) + offsetY+ wsOffset.y, z + wsOffset.z);
						GL.Vertex3( (start.x + stop.x)/2 + wsOffset.x, world.GetHeight((start.x + stop.x)/2,z+1) + offsetY+ wsOffset.y, z+1 + wsOffset.z);

					}
					break;

				default:
				break;
				}

			

			#endregion
		  
		 }


		 GL.End();
		}
	}
}