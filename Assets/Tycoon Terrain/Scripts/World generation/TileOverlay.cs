using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
	public class TileOverlay : MonoBehaviour {
		

		public bool isVisible = false;
		public float offsetY = 0.2f;
		public WorldBehaviour world;
		public Vector3 start;
		public Vector3 stop;
		public int xCoord;
		public int yCoord;
		public Material lineMaterial;
		public Color okColor = new Color(0f,1f,0f,0.5f);
        public Color notOkColor = new Color(1f, 0f, 0f, 0.5f);
        private Color currentColor;
        [HideInInspector]
		public bool[,] tiles;
		void Start () 
		{
            currentColor = okColor;
		}
		
		
		void OnPostRender() 
		{       
			// set the current material
			lineMaterial.SetPass( 0 );
			
			GL.Begin( GL.QUADS );
			
			if(isVisible)
			{
				GL.Color(currentColor);
				
				#region draw gl quads
                // Draw tiles that are OK in green and non-OK in red
				for (int x = 0; x < tiles.GetLength(0); x++) {
					for (int y = 0; y < tiles.GetLength(1); y++) {
						if(tiles[x,y]){
                            currentColor = okColor;
						}else{
                            currentColor = notOkColor;
						}
						GL.Color(currentColor);
						DrawTile(xCoord + x,yCoord + y);
					}
				}

				#endregion
				
			}
			
			
			GL.End();
		}

		void DrawTile(int x, int y){
			float tileWidth = world.worldData.tileWidth;
			Vector3 center = world.GetWorldPositionFromTile(x,y,true);

			Vector3 c1 = center + new Vector3(-0.4f * tileWidth, 0, -0.4f * tileWidth);
			c1.y = Mathf.Max(world.GetHeight(c1), world.GetWaterLevelHeight());
			Vector3 c2 = center + new Vector3(-0.4f * tileWidth, 0, 0.4f * tileWidth);
			c2.y = Mathf.Max(world.GetHeight(c2), world.GetWaterLevelHeight());
			Vector3 c3 = center + new Vector3(0.4f * tileWidth, 0, 0.4f * tileWidth);
			c3.y = Mathf.Max(world.GetHeight(c3), world.GetWaterLevelHeight());
			Vector3 c4 = center + new Vector3(0.4f * tileWidth, 0, -0.4f * tileWidth);
			c4.y = Mathf.Max(world.GetHeight(c4), world.GetWaterLevelHeight());

			GL.Vertex(c1);
			GL.Vertex(c2);
			GL.Vertex(c3);
			GL.Vertex(c4);
		}
	}
}