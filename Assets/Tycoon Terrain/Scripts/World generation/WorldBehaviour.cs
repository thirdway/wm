using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace TycoonTerrain{
	/// <summary>
	/// Map tile in the world
	/// </summary>
	public struct MapTile{
		public int x;
		public int y;


		public MapTile(int x, int y){
			this.x= x;
			this.y = y;
		}

		public enum MapTileType{
			FLAT = 0,//All corners are of the same height
			LEANING = 1,//Pairwise connected corners are of the same height
            OCEAN = 2,
			OTHER = 3,
            FLAT_OR_LEANING = 4,//This will never be returned by world but can be used for setting which types should be allowed for building for example
            ANY,//This will never be returned by world but can be used for setting which types of tiles should be allowed for a building for example

		}

		/// <summary>
		/// Sets the start and stop tiles so that start is in the bottom left corner and stop is in the top right corner
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="stop">Stop.</param>
		public static void SetStartStop(ref MapTile start, ref MapTile stop){
			MapTile tmpStart = start;
			MapTile tmpStop = stop;

			if(tmpStart.x > tmpStop.x){
				start.x = tmpStop.x;
				stop.x = tmpStart.x;
			}
			if(tmpStart.y > tmpStop.y){
				start.y = tmpStop.y;
				stop.y = tmpStart.y;
			}
		}

        public static bool IsTileOk(MapTileType buildingTileType, MapTileType worldTileType) {
            return buildingTileType == MapTile.MapTileType.ANY ||
                        (buildingTileType == MapTile.MapTileType.FLAT_OR_LEANING && (worldTileType == MapTile.MapTileType.FLAT || worldTileType == MapTile.MapTileType.LEANING)) ||
                        buildingTileType == worldTileType;
        }
	}
	/// <summary>
	/// World behaviour
	/// This is where all world gen and terraforming takes place! The data is in the field worldData for easy
	/// serializing.
	/// 
	/// The class is quite large and to make it easier to navigate it has been divided into #regions. If using Mono
	/// with standard settings these should be visible on the right hand side.
	/// 
	/// In general you will be using the region "Terrain queries" to find out if a tile has a building, is flat, is
	/// aboove water and so on. Note that a world has power of 2 number of tiles and power of 2 + 1 heightvalues, eg 32x32 tiles
	/// needs 33x33 heightvalues. 
	/// 
	/// To raise, lower, level terrain and construct buildings you use the "Terraforming orders"-region. Thses will then be placed
	/// in a queue and handled as fast as the game loop can. It will only do as much work in an update as it has time for. The most time
	/// consuming operation is editing heightmap on large terrains (>= 256x256 tiles). This is due to a bug in Unity which makes it 
	/// impossible to edit only parts of the heightmap (or maybe an undocumented feature). The whole heightmap needs to be edited
	/// in order to recalculate the physics collider. If you have requirements for larger maps than 512x512 I recommend either not using
	/// terrain collider or splitting the terrain into manageable chunks (say several terrains of 128x128) instead. This will require 
	/// some serious coding though. In a future update this is something I might adress if there is significant interest among users.
	/// 
	/// A word of warning, do not dabble with the region "Coroutines" where heavy work is amortized unless you know what you are doing.
	/// (or really determined) You may introduce timing errors and race conditions which are hard to debug. Try solve your task using the 
	/// other regions first.
	///
	/// A final important note: The heightmap is stored with y-values first and x-values second. I don't know why (it is undocumented in the 
	/// manual but it is something we have to be aware of. You'll note that to accomodate this we call all public methods in this class
	/// using the normal notation with (x,y)-coordinates but in some private methods these are switched when dealing with heightmaps.
	/// </summary>
	public class WorldBehaviour : MonoBehaviour {

		//Game object references that needs to be assigned
		[Tooltip("The unity terrain object that this behaviour should affect")]
		public Terrain terrain;
		[Tooltip("Link to the water transform here.")]
		public Transform water;

		//These are the indices in the unity terrain object that are used to paint the terrain
		[Tooltip("The index of the unity terrain object texture that should be drawn where there is grass.")]
		public int terrainGrassTextureIndex = 0;
		[Tooltip("The index of the unity terrain object texture that should be drawn where the terrain edge should be. note that this texture will be very stretched so use a solid color unless you are using a custom shader of some sort.")]
		public int terrainEdgeTextureIndex = 1;
		[Tooltip("The index of the unity terrain object texture that should be drawn where there is dirt.")]
		public int terrainDirtTextureIndex = 2;



		/// <summary>
		/// The tree types used by the world generator. If you like more types of trees then create a prefab, assign it to the terrain.
		/// Then, to create a new scriptable object with data for this tree in the menu select Assets->Tycoon Terrain->Create new tree type 
		/// You new tree type scriptable object will be in the folder Tycoon Terrain/Game Data/TreeTypes nad in there you set the data for 
		/// the tree like at what altitudes it should grow etc. When that is done add it to this list and it will be generated in nice
		/// forests at world-gen. These trees become regular unity terrain trees but can be bulldozed just like everything else
		/// </summary>
		[Tooltip("The tree types used by the world generator. If you like more types of trees then create a prefab, assign it to the terrain. Then, to create a new scriptable object with data for this tree in the menu select\n\n Assets->Tycoon Terrain->Create new tree type \n\n You new tree type scriptable object will be in the folder Tycoon Terrain/Game Data/TreeTypes and in there you set the data for the tree like at what altitudes it should grow etc. When that is done add it to this list and it will be generated in nice forests at world-gen. These trees become regular unity terrain trees but can be bulldozed just like everything else")]
		public List<TreeType> treeTypes;

		/// <summary>
		/// The world data where everything that is needed to know about a world is stored. All heightmaps, treemaps etc are stored in this class
		/// The reason for keeping it separated from this class is to have on pure dataholder object that we can easily serialize.
		/// Follow the breadcrumbs of the Save()-method to see how it is done.
		/// </summary>
		[Tooltip("The world data where everything that is needed to know about a world is stored. All heightmaps, treemaps etc are stored in this class. The reason for keeping it separated from this class is to have on pure dataholder object that we can easily serialize. Follow the breadcrumbs of the Save()-method to see how it is done.")]
		public WorldData worldData;

        [Tooltip("The layers that should block the world in some sense. This is used e.g. when checking if a tree should regrow. If any of these layers are presentt in the tile ten it will not start regrowing. IMPORTANT: The terrains own layer must be included in this mask.")]
        public LayerMask blockingLayers;

		HashSet<int> forestedAreas = new HashSet<int>();

		/// <summary>
		/// Gets the number of tiles along one side of the map
		/// </summary>
		/// <value>The size of the tile map.</value>
		public int tileMapSize{
			get{
				return worldData.heightMapSize - 1;
			}
		}

		/// <summary>
		/// Keeps track of the tiles that are currently regrowing to update their splatmaps. See coroutine Terraform() but don't change it unless you REALLY need to (and know what you're doing)
		/// Key in dict is Tile-key
		/// </summary>
		Dictionary<int, Regrowth> regrowingTiles = new Dictionary<int, Regrowth>();
		/// <summary>
		/// The queue of terraform orders that are waiting to be handled. They are dealt with in coroutine Terraform()
		/// </summary>
		Queue<TerraformOrder> terraformQueue = new Queue<TerraformOrder>();
		/// <summary>
		/// When user wants to delete a tree ALL trees of the map needs to be iterated. We speed things up by keeping track of all
		/// trees we need to get rid of and do them all at once.. See coroutine RemoveTrees()
		/// </summary>
		HashSet<int> tilesToRemoveTrees = new HashSet<int>();
		Dictionary<int,GrowingTreeBehaviour> growingTrees = new Dictionary<int,GrowingTreeBehaviour>();


		#region Events
		/// <summary>
		/// Fires when the entire world was generated anew. This means that the world size may have changed
		/// </summary>
		public System.Action OnNewWorldWasGenerated;
		/// <summary>
		/// Fires when the world was edited by eg applying some terraforming. World will retain its size though.
		/// </summary>
		public System.Action OnWorldWasEdited;
		/// <summary>
		/// Fires when a tile is bulldozed. You may want this to result in removing buildings or some other objects 
		/// you have in your world.
		/// </summary>
		public System.Action<MapTile> OnTileWasBulldozed;
		#endregion

		// Use this for initialization
		void Start () {

		}
		


		#region Terraforming orders
		/*
		 * These methods are what you want to use to terraform the world. They will be performed by coroutines found at the bottom of this file
		 */


		public void LevelTerrain(MapTile start, MapTile stop){
			LevelTerrain(start.x, start.y, stop.x, stop.y);
		}

		/// <summary>
		/// Levels the terrain in an area defined by its two opposing corners.
		/// 
		/// Note that xStart can be greater then xStop, that will be dealed with internally
		/// </summary>
		/// <param name="xStart">X start.</param>
		/// <param name="yStart">Y start.</param>
		/// <param name="xStop">X stop.</param>
		/// <param name="yStop">Y stop.</param>
		public void LevelTerrain(int xStart, int yStart, int xStop, int yStop){
			int desiredTerrainHeight = worldData.heightMap[yStart, xStart];
			LevelTerrain(xStart, yStart, xStop, yStop, desiredTerrainHeight);
		}

		public void LevelTerrain(int xStart, int yStart, int xStop, int yStop, int desiredTerrainHeight){
			TerraformOrder order = new TerraformOrder(yStart < yStop ? yStart : yStop,
			                                          xStart < xStop ? xStart : xStop,
			                                          yStart > yStop ? yStart : yStop,
			                                          xStart > xStop ? xStart : xStop,
			                                          desiredTerrainHeight);
			terraformQueue.Enqueue(order);
		}

		public void LowerTerrain(MapTile tile){
			LowerTerrain(tile.x, tile.y);
		}

		public void LowerTerrain(int x, int y){
			// Note the switch of x/y here! (see note in class description)
			TerraformOrder order = new TerraformOrder(y,x, worldData.heightMap[y,x] - 1, true);
			terraformQueue.Enqueue(order);
		}

		public void RaiseTerrain(MapTile tile){
			RaiseTerrain(tile.x, tile.y);
		}

		public void RaiseTerrain(int x, int y){
			// Note the switch of x/y here! (see note in class description)
			TerraformOrder order = new TerraformOrder(y,x, worldData.heightMap[y,x] + 1, false);
			terraformQueue.Enqueue(order);
		}

		public void BullDozeTile(MapTile tile){
			BullDozeTile(tile.x, tile.y);
		}

		public void BullDozeTile(int x, int y){
			// Yeah x and y are not switched here as we do not work with the heightmap
			TerraformOrder order = new TerraformOrder(x,y);
			terraformQueue.Enqueue(order);
		}

		/// <summary>
		/// Bulldozes the terrain in an area defined by its two opposing corners.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="stop">Stop.</param>
		public void BulldozeArea(MapTile start, MapTile stop){
			BulldozeArea(start.x, start.y, stop.x, stop.y);
		}

		/// <summary>
		/// Bulldozes the terrain in an area defined by its two opposing corners.
		/// 
		/// Note that xStart can be greater then xStop, that will be dealed with internally
		/// </summary>
		/// <param name="xStart">X start.</param>
		/// <param name="yStart">Y start.</param>
		/// <param name="xStop">X stop.</param>
		/// <param name="yStop">Y stop.</param>
		public void BulldozeArea(int xStart, int yStart, int xStop, int yStop){
			TerraformOrder order = new TerraformOrder(xStart < xStop ? xStart : xStop,
			                                          yStart < yStop ? yStart : yStop,
			                                          xStart > xStop ? xStart : xStop,
			                                          yStart > yStop ? yStart : yStop);
			terraformQueue.Enqueue(order);
		}
		#endregion



		#region Cost calculation

		/*
		 * You can use these functions to check if a specific terraform order is afforded and deduct the cost before
		 * giving the order
		 */

		public TerraformCostCalculation GetLevelTerrainCost(MapTile start, MapTile stop){
			return GetLevelTerrainCost(start.x, start.y, stop.x, stop.y);
		}

		public TerraformCostCalculation GetLevelTerrainCost(int xStart, int yStart, int xStop, int yStop){
			int desiredTerrainHeight = worldData.heightMap[yStart, xStart];
			TerraformOrder order = new TerraformOrder(yStart < yStop ? yStart : yStop,
			                                          xStart < xStop ? xStart : xStop,
			                                          yStart > yStop ? yStart : yStop,
			                                          xStart > xStop ? xStart : xStop,
			                                          desiredTerrainHeight);
			return CalculateTerraformCost(order);
		}

		public TerraformCostCalculation GetLowerTerrainCost(MapTile tile){
			return GetLowerTerrainCost(tile.x, tile.y);
		}
		public TerraformCostCalculation GetLowerTerrainCost(int x, int y){
			// Note the switch of x/y here!
			TerraformOrder order = new TerraformOrder(y,x, worldData.heightMap[y,x] - 1, true);
			return CalculateTerraformCost(order);
		}

		public TerraformCostCalculation GetRaiseTerrainCost(MapTile tile){
			return GetRaiseTerrainCost(tile.x, tile.y);
		}
		public TerraformCostCalculation GetRaiseTerrainCost(int x, int y){
			// Note the switch of x/y here!
			TerraformOrder order = new TerraformOrder(y,x, worldData.heightMap[y,x] + 1, false);
			return CalculateTerraformCost(order);
		}

		public TerraformCostCalculation GetBullDozeTileCost(MapTile tile){
			return GetBullDozeTileCost(tile.x, tile.y);
		}

		public TerraformCostCalculation GetBullDozeTileCost(int x, int y){
			TerraformOrder order = new TerraformOrder(x,y);
			return CalculateTerraformCost(order);
		}

		public TerraformCostCalculation GetBulldozeAreaCost(MapTile start, MapTile stop){
			return GetBulldozeAreaCost(start.x, start.y, stop.x, stop.y);
		}

		public TerraformCostCalculation GetBulldozeAreaCost(int xStart, int yStart, int xStop, int yStop){
			TerraformOrder order = new TerraformOrder(xStart < xStop ? xStart : xStop,
			                                          yStart < yStop ? yStart : yStop,
			                                          xStart > xStop ? xStart : xStop,
			                                          yStart > yStop ? yStart : yStop);
			return CalculateTerraformCost(order);
		}

		HashSet<int> bulldozedTileTracker = new HashSet<int>();
		Dictionary<int,int> terrainHeightChangeTracker = new Dictionary<int, int>();


		public struct TerraformCostCalculation{
			/// <summary>
			/// The bulldozed tiles stored as tile keys
			/// </summary>
			public HashSet<int> bulldozedTiles;
			/// <summary>
			/// The height changed tiles stored as tile key, level difference
			/// </summary>
			public Dictionary<int,int> heightChangedTiles;

			public int GetNumberOfHeightChanges(){
				int raisedTiles = 0;
				// Note that if a tile was raised x steps up or down this will count x times
				foreach (var item in heightChangedTiles) {
					raisedTiles += item.Value;	
				}
				return raisedTiles;
			}
		}
		/// <summary>
		/// Calculates the terraform cost of an order. Note that this is not amortized as most polling of data cacn be performed
		/// reasonable fast if you just cache some values like tree and building hashes
		/// 
		/// If you add a new terraform type then you need to add its cost here
		/// </summary>
		/// <returns>The terraform cost.</returns>
		/// <param name="order">Order.</param>
		TerraformCostCalculation CalculateTerraformCost(TerraformOrder order){
			bulldozedTileTracker.Clear();
			terrainHeightChangeTracker.Clear();

			switch (order.orderType) {
			case TerraformOrder.OrderType.BULLDOZE:
				bulldozedTileTracker.Add(GetTileCoordKey(order.y,order.x));
				break;
			case TerraformOrder.OrderType.BULLDOZE_AREA:
				// Bulldoze the explicit area
				for (int x = order.x; x < order.xStop; x++) {
					for (int y = order.y; y < order.yStop; y++) {
						bulldozedTileTracker.Add(GetTileCoordKey(y,x));
					}	
				}
				break;
			case TerraformOrder.OrderType.RAISE_LOWER:
				GetTerrainHeightChangeCost(order.x,order.y,order.desiredHeight, order.isLowering);
				break;
			case TerraformOrder.OrderType.LEVEL:

				// Calculate how many tiles will be raised during flattening
				for (int x = order.x; x <= order.xStop; x++) {
					for (int y = order.y; y <= order.yStop; y++) {
						if(worldData.heightMap[x,y] != order.desiredHeight){
							terrainHeightChangeTracker.Add(GetHeightCoordKey(x,y), Mathf.Abs( worldData.heightMap[x,y] - order.desiredHeight));
							bulldozedTileTracker.Add(GetTileCoordKey(y,x));
							bulldozedTileTracker.Add(GetTileCoordKey(y-1,x));
							bulldozedTileTracker.Add(GetTileCoordKey(y,x-1));
							bulldozedTileTracker.Add(GetTileCoordKey(y-1,x-1));

						}
					}	
				}
				
				// Calculate how many adjacent heights that are affected by this flattening
				for (int x = order.x - 1; x <= order.xStop + 1; x++) {
					int y = order.y - 1;

					if(x > 0 && y > 0 && x < worldData.heightMapSize-1 && y < worldData.heightMapSize-1 && 
					   Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1 &&
					   !terrainHeightChangeTracker.ContainsKey(GetHeightCoordKey(x,y))
					   ){
						if(worldData.heightMap[x,y] > order.desiredHeight ){
							GetTerrainHeightChangeCost(x,y, order.desiredHeight + 1, true);	
						}else{
							GetTerrainHeightChangeCost(x,y,order.desiredHeight - 1, false);
						}
					}
					y = order.yStop + 1;
					if(x > 0 && y > 0 && x < worldData.heightMapSize-1 && y < worldData.heightMapSize-1 && 
					   Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1 &&
					   !terrainHeightChangeTracker.ContainsKey(GetHeightCoordKey(x,y))
					   ){
						if(worldData.heightMap[x,y] > order.desiredHeight){
							GetTerrainHeightChangeCost(x,y, order.desiredHeight + 1, true);	
						}else{
							GetTerrainHeightChangeCost(x,y,order.desiredHeight - 1, false);
						}
					}
				}
				
				for (int y = order.y - 1; y <= order.yStop + 1; y++) {
					int x = order.x - 1;
					if(x > 0 && y > 0 && x < worldData.heightMapSize-1 && y < worldData.heightMapSize-1 && 
					   Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1 &&
					   !terrainHeightChangeTracker.ContainsKey(GetHeightCoordKey(x,y))
					   ){
						if(worldData.heightMap[x,y] > order.desiredHeight ){
							GetTerrainHeightChangeCost(x,y, order.desiredHeight + 1, true);	
						}else{
							GetTerrainHeightChangeCost(x,y,order.desiredHeight - 1, false);
						}
					}
					x = order.xStop + 1;
					if(x > 0 && y > 0 && x < worldData.heightMapSize-1 && y < worldData.heightMapSize-1 && 
					   Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1  &&
					   !terrainHeightChangeTracker.ContainsKey(GetHeightCoordKey(x,y))
					   ){
						if(worldData.heightMap[x,y] > order.desiredHeight){
							GetTerrainHeightChangeCost(x,y, order.desiredHeight + 1, true);	
						}else{
							GetTerrainHeightChangeCost(x,y,order.desiredHeight - 1, false);
						}
					}
				}
				break;
			
			default:
				Debug.LogError(order.orderType + " cost calculation not implemented yet");
				break;
			}

			//Debug.Log("Affected number of tiles: " + terrainHeightChangeTracker.Count);
			//Debug.Log("Raised tiles: " + raisedTiles);
			//Debug.Log("Bulldozed tiles: " + bulldozedTiles);
			//Debug.Log("Total cost: " + cost);
			TerraformCostCalculation terraformCost = new TerraformCostCalculation();
			terraformCost.bulldozedTiles = bulldozedTileTracker;
			terraformCost.heightChangedTiles = terrainHeightChangeTracker;
			return terraformCost;
		}

		/// <summary>
		/// Calculates the cost of a terrain height change. Note that it will include the cost of other tile height changes that are
		/// results of the first change.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="height">Height.</param>
		/// <param name="isLowering">If set to <c>true</c> is lowering.</param>
		void GetTerrainHeightChangeCost(int x, int y, int height, bool isLowering){
			if(x <= 0 || y <= 0 || x >= worldData.heightMapSize-1 || y >= worldData.heightMapSize-1 || worldData.heightMap[x,y] == height)
				return;

			if(isLowering && worldData.heightMap[x,y] > height ||
			   !isLowering && worldData.heightMap[x,y] < height){
				int key = GetHeightCoordKey(x,y);
				int cost = Mathf.Abs( worldData.heightMap[x,y] - height);
				if(!terrainHeightChangeTracker.ContainsKey(key) || terrainHeightChangeTracker[key] < cost) 
					terrainHeightChangeTracker.Add(key,cost);

			}else{
				//did nothing
				return;
			}
			
			// Check 8-neighbourhood and adapt it to the new change
			for (int i = Mathf.Max(1, x-1); i <= Mathf.Min(worldData.heightMapSize-1, x+1); i++) {
				for (int j = Mathf.Max(1, y-1); j <=Mathf.Min(worldData.heightMapSize-1, y+1); j++) {
					int key = GetHeightCoordKey(i,j);
					if(worldData.heightMap[i,j] > height + 1 && !terrainHeightChangeTracker.ContainsKey(key)){
						GetTerrainHeightChangeCost(i,j,height + 1, isLowering);
					}else if(worldData.heightMap[i,j] < height - 1 && !terrainHeightChangeTracker.ContainsKey(key)){
						GetTerrainHeightChangeCost(i,j,height - 1, isLowering);
					}
				}	
			}

			bulldozedTileTracker.Add(GetTileCoordKey(y,x));
			bulldozedTileTracker.Add(GetTileCoordKey(y-1,x));
			bulldozedTileTracker.Add(GetTileCoordKey(y,x-1));
			bulldozedTileTracker.Add(GetTileCoordKey(y-1,x-1));

		}

		#endregion

		#region Terrain queries
		/*
		 * These methods can be used to find out the properties of terrain in different tiles
		 */


		/// <summary>
		/// Gets the world position of the center of a tile
		/// </summary>
		/// <returns>The tile world position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="doIncludeWater">If set to true will return watyer level height in case
		/// the tilw is covered with water.</param>
		public Vector3 GetWorldPositionFromTile(int x, int y, bool doIncludeWater = false){
			Vector3 pos = new Vector3();

			pos.x = Mathf.Round(x * worldData.tileWidth ) + worldData.tileWidth /2f;
			pos.z = Mathf.Round(y * worldData.tileWidth ) + worldData.tileWidth /2f;
			pos.y = GetHeight(pos.x, pos.z);

			pos += terrain.transform.position;

			if(doIncludeWater){
				pos.y = Mathf.Max(pos.y, GetWaterLevelHeight());
			}

			return pos;
		}

		/// <summary>
		/// Gets the world position of the center of a tile
		/// </summary>
		public Vector3 GetWorldPositionFromTile(MapTile tile){
			return GetWorldPositionFromTile(tile.x, tile.y);
		}

		/// <summary>
		/// Gets the tile that holds a world position.Y value of position is not considered.
		/// </summary>
		/// <returns>The tile from world position.</returns>
		/// <param name="pos">Position.</param>
		public MapTile GetTileFromWorldPosition(Vector3 pos){
			MapTile tile = new MapTile();
			tile.x = Mathf.Clamp( (int)(pos.x / worldData.tileWidth), 0, tileMapSize - 1);
			tile.y = Mathf.Clamp( (int)(pos.z / worldData.tileWidth), 0, tileMapSize - 1);
			return tile; 
		}

        public Vector3 GetCenterOfWorld() {
            return GetWorldPositionFromTile(tileMapSize / 2, tileMapSize / 2, true);
        }

		/// <summary>
		/// Gets the height of a position in the heightmap. This means that this does not get the height of a TILE but
		/// the height of a corner in a tile (as tiles themselves do not really have a height, the corners do...)
		/// 
		/// If queried from a tile's coordinates then this will return the height of the tile's lower left corner
		/// </summary>
		/// <returns>The height in world coordinates</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public float GetHeight(float x, float y){
			if(terrain != null){
				return terrain.terrainData.GetInterpolatedHeight( x / (tileMapSize * worldData.tileWidth), y / (tileMapSize * worldData.tileWidth));
			}else{
				return 0;
			}
		}

		/// <summary>
		/// Gets the height corresponding to a certain worldposition
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="position">Position.</param>
		public float GetHeight(Vector3 worldPosition){
			worldPosition -= terrain.transform.position;
			return GetHeight(worldPosition.x, worldPosition.z) + terrain.transform.position.y;
		}
		 	
		/// <summary>
		/// Gets the height of a tile counted in discrete steps. Note that this differs from world space height based
		/// on how far one step is. If
		/// </summary>
		/// <returns>The height in steps.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int GetHeightInSteps(int x, int y){
			return worldData.heightMap[y,x]; //Note switch of x and y
		}

		public Vector3 GetNormal(Vector3 position){
			if(terrain != null){
				return terrain.terrainData.GetInterpolatedNormal(0.5f * position.x / (tileMapSize), 0.5f * position.z / (tileMapSize));
			}else{
				return Vector3.up;
			}
		}

		/// <summary>
		/// Gets the height of the water level in world space.
		/// </summary>
		/// <returns>The water level height.</returns>
		public float GetWaterLevelHeight(){
			return worldData.heightPerTile * ( worldData.heightMapSize * 0.5f + worldData.waterLevel  * 2 + terrain.transform.position.y);
		}


        public MapTile.MapTileType GetTileType(Vector3 worldPosition) {
            return GetTileType(GetTileFromWorldPosition(worldPosition));
        }

        /// <summary>
        /// Returns what type of topography a tile has. You could use this to devide where buildings could be 
        /// constructed for example
        /// </summary>
        /// <returns>The tile type.</returns>
        /// <param name="tile">Tile.</param>
        public MapTile.MapTileType GetTileType(MapTile tile){
			if (!IsTileAboveWater (tile))
				return MapTile.MapTileType.OCEAN;
			if(IsTileFlat(tile))
				return MapTile.MapTileType.FLAT;
			if (isTileLeaningDownX (tile.x, tile.y) ||
				isTileLeaningDownY (tile.x, tile.y) ||
				isTileLeaningUpX (tile.x, tile.y) ||
				isTileLeaningUpY (tile.x, tile.y))
				return MapTile.MapTileType.LEANING;

			return MapTile.MapTileType.OTHER;
		}


		/// <summary>
		/// Checks if a tile is entirely flat. You might want to run this check for building constructions etc
		/// </summary>
		/// <returns><c>true</c>, if tile is flat, <c>false</c> otherwise.</returns>
		public bool IsTileFlat(int x, int y){
			// Note the switch of x/y here!
			return 	worldData.heightMap[y,x] == worldData.heightMap[y+1,x] &&
					worldData.heightMap[y+1,x] == worldData.heightMap[y,x +1] &&
					worldData.heightMap[y,x +1] == worldData.heightMap[y+1,x+1];
		}

		public bool IsTileFlat(MapTile tile){
			return IsTileFlat (tile.x, tile.y);
		}
		public bool IsAreaFlat(int startX, int startY, int stopX, int stopY){
			int height = worldData.heightMap[startY,startX];
			for (int x = startX; x <= stopX + 1; x++) {
				for (int y = startY; y <= stopY + 1; y++) {
					if(worldData.heightMap[y,x] != height)
						return false;
				}	
			}

			return true;
		}

		/// <summary>
		/// Checks if a tile is flat but leaning. You might want roads or railroads to be allowed construction on such a tile for example.
		/// </summary>
		/// <returns><c>true</c>, if tile leaning in direction specified, <c>false</c> otherwise.</returns>
		public bool isTileLeaningUpX(int x, int y){
			return 	worldData.heightMap[y,x] == worldData.heightMap[y,x + 1] + 1 &&
					worldData.heightMap[y+1,x] == worldData.heightMap[y+1,x + 1] + 1 ;
		}
        public bool isTileLeaningUpX(MapTile tile) {
            return isTileLeaningUpX(tile.x, tile.y);
        }
        /// <summary>
        /// Checks if a tile is flat but leaning. You might want roads or railroads to be allowed construction on such a tile for example.
        /// </summary>
        /// <returns><c>true</c>, if tile leaning in direction specified, <c>false</c> otherwise.</returns>
        public bool isTileLeaningDownX(int x, int y){
			return 	worldData.heightMap[y,x] == worldData.heightMap[y,x + 1] - 1 &&
					worldData.heightMap[y+1,x] == worldData.heightMap[y+1,x + 1] - 1 ;
		}
        public bool isTileLeaningDownX(MapTile tile) {
            return isTileLeaningDownX(tile.x, tile.y);
        }
        /// <summary>
        /// Checks if a tile is flat but leaning. You might want roads or railroads to be allowed construction on such a tile for example.
        /// </summary>
        /// <returns><c>true</c>, if tile leaning in direction specified, <c>false</c> otherwise.</returns>
        public bool isTileLeaningUpY(int x, int y){
			return 	worldData.heightMap[y,x] == worldData.heightMap[y+1,x ] + 1 &&
				worldData.heightMap[y,x+1] == worldData.heightMap[y+1,x + 1] + 1 ;
		}
        public bool isTileLeaningUpY(MapTile tile) {
            return isTileLeaningUpY(tile.x, tile.y);
        }
        /// <summary>
        /// Checks if a tile is flat but leaning. You might want roads or railroads to be allowed construction on such a tile for example.
        /// </summary>
        /// <returns><c>true</c>, if tile leaning in direction specified, <c>false</c> otherwise.</returns>
        public bool isTileLeaningDownY(int x, int y){
			return 	worldData.heightMap[y,x] == worldData.heightMap[y+1,x] - 1 &&
				worldData.heightMap[y,x+1] == worldData.heightMap[y+1,x + 1] - 1 ;
		}
        public bool isTileLeaningDownY(MapTile tile) {
            return isTileLeaningDownY(tile.x, tile.y);
        }

        public bool IsTileAboveWater(int x, int y){
			return 	worldData.heightMap[y,x] > worldData.waterLevel &&
					worldData.heightMap[y+1,x] > worldData.waterLevel &&
					worldData.heightMap[y,x +1] > worldData.waterLevel &&
					worldData.heightMap[y+1,x+1] > worldData.waterLevel;
		}

		public bool IsTileAboveWater(MapTile tile){
			return IsTileAboveWater(tile.x, tile.y);
		}

		public bool IsAreaAboveWater(int startX, int startY, int stopX, int stopY){
			for (int x = startX; x <= stopX; x++) {
				for (int y = startY; y <= stopY; y++) {
					if(!IsTileAboveWater(x,y))
						return false;
				}	
			}
			
			return true;
		}

		/// <summary>
		/// Determines whether a tile has adjacent tiles with trees in them. 
		/// Note that this methd uses the variable "minimumAdjacentTreeSizeForRegrowth" to determine if a tree is big enough to count
		/// so if that variable is set to 0 in the inspector then this method will ALWAYS return true!
		/// </summary>
		/// <returns><c>true</c> if this instance has adjacent tree the specified x y; otherwise, <c>false</c>.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public bool HasAdjacentTree(int x, int y){
			for (int xI = x-1; xI <= x+1; xI++) {
				for (int yI = y-1; yI <= y+1; yI++) {
					if(GetTreeSize(xI, yI) >= worldData.minimumAdjacentTreeSizeForRegrowth)
						return true;
				}
			}
			return false;
		}

		public bool IsTileEmptyFromPhysics(int x, int y){
			float height = 100;
			Vector3 pos = GetWorldPositionFromTile(x,y) + height * Vector3.up;
			RaycastHit hit;
			//Check if the ray hits terrain. If it does then the tile is considered empty
			if(Physics.Raycast(pos, Vector3.down, out hit, height + 10))
				return (hit.collider.gameObject.layer == terrain.gameObject.layer);

			Debug.LogWarning("Raycast should have hit terrain at least. Better check if something is wrong");
			return true;
		}

		public bool IsTileEmpty(MapTile tile){
			return IsTileEmpty(tile.x, tile.y);
		}

		public bool IsTileEmpty(int x, int y){
			if(DoTileHaveForest(x,y))
				return false;

			return true;
		}

		public bool IsAreaEmpty(int startX, int startY, int stopX, int stopY){
			for (int x = startX; x <= stopX; x++) {
				for (int y = startY; y <= stopY; y++) {
					if(!IsTileEmpty(x,y))
						return false;
				}	
			}
			
			return true;
		}





		public bool DoTileHaveForest(int x, int y){
			int key = GetTileCoordKey(x,y);
			return forestedAreas.Contains(key);
		}

		public float GetTreeSize(int x, int y){
			if(DoTileHaveForest(x,y)){
				int key = GetTileCoordKey(x,y);
				if(growingTrees.ContainsKey(key) && growingTrees[key] != null){
					return GetTreeLifeForce(x,y,growingTrees[key].treeInstance.prototypeIndex) * growingTrees[key].GrownRatio();
				}else{

					float wood = 0;
					for (int i = 0; i < treeTypes.Count; i++) {
						float lifeForce = GetTreeLifeForce(x,y,i);
						//Debug.Log("Tree life "  + lifeForce);
						if(lifeForce > 0)
							wood += lifeForce;
					}
					return Mathf.Min(wood, 1);
				}
			}else{
				return 0;
			}
		}

		/// <summary>
		/// Gets the total size of the trees in an area. Use this to calculate how much wood you get for bulldozing an area for example.
		/// </summary>
		/// <returns>The total tree size.</returns>
		public float GetTotalTreeSize(int xStart, int yStart, int xStop, int yStop){
			if (xStart == xStop || yStart == yStop)
				return 0;
			return GetTotalTreeSizeIndicesFixed( xStart < xStop ? xStart : xStop,
				yStart < yStop ? yStart : yStop,
				(xStart > xStop ? xStart : xStop) - 1,
				(yStart > yStop ? yStart : yStop) - 1);
		}

		private float GetTotalTreeSizeIndicesFixed(int xStart, int yStart, int xStop, int yStop){

			float totalTreeSize = 0;
			// Calculate how many tiles will be raised during flattening
			for (int x = xStart; x <= xStop; x++) {
				for (int y = yStart; y <= yStop; y++) {
					
					totalTreeSize += GetTreeSize(x,y);
				}	
			}
			
			return totalTreeSize;
		}

		public float GetTileHeightOverWater(int x, int y){
			return (worldData.heightMap[y,x] + worldData.heightMap[y+1,x] + worldData.heightMap[y,x +1] + worldData.heightMap[y+1,x+1]) / 4f - worldData.waterLevel;
		}

		public float GetSoilFertility(int x, int y){
			float fertilityFromHeight = (worldData.maxHeightForSoilFertility - GetTileHeightOverWater(x,y)) / worldData.maxHeightForSoilFertility;
			float fertilityFromNoiseMap = worldData.soilFertilityMap[x,y];
			float fertilityFromSettings = (worldData.soilFertility - 0.5f) * 4;
			return Mathf.Clamp01(fertilityFromHeight + fertilityFromNoiseMap + fertilityFromSettings);
		}



		/// <summary>
		/// We use this as an individual key for the height coordinate to access it in dictionaries and hashsets
		/// 
		/// Note the slight difference to GetTileCoordKey! This one is to be used for heightmap based queries only!
		/// </summary>
		/// <returns>The height coordinate key.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		int GetHeightCoordKey(int x, int y){
			return worldData.heightMapSize * y + x;
		}

		/// <summary>
		/// We use this as an individual key for the tile coordinate to access it in dictionaries and hashsets
		/// 
		/// Note the slight difference to GetHeightCoordKey! This one is to be used for tilemap based queries only!
		/// </summary>
		/// <returns>The tile coordinate key.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int GetTileCoordKey(int x, int y){
			return tileMapSize * y + x;
		}
		public int GetTileCoordKey(float x, float y){
			return tileMapSize * Mathf.FloorToInt( y / worldData.tileWidth) + Mathf.FloorToInt(x/ worldData.tileWidth);
		}
		public int GetTileCoordKey(MapTile tile){
			return GetTileCoordKey(tile.x, tile.y);
		}

		int GetTileXFromKey(int key){
			return key % tileMapSize;
		}
		int GetTileYFromKey(int key){
			return key / tileMapSize;
		}

		public MapTile GetTileFromKey(int key){
			return new MapTile(GetTileXFromKey(key), GetTileYFromKey(key));
		}
		#endregion


		#region Terrain terraforming
		/*
		 * These methods are used to actually perform the terraforming of terrain. They are amortized over several frames
		 * for performance reasons and should only be called directly by the coroutines that perform the terraforming.
		 * 
		 * Mere mortals should instead create terraform orders and place them in queue.
		 */


		void SetTerrainHeight(int x, int y, int height, bool isLowering){
			if(x <= 0 || y <= 0 || x >= worldData.heightMapSize-1 || y >= worldData.heightMapSize-1 || worldData.heightMap[x,y] == height)
				return;
			if(isLowering && worldData.heightMap[x,y] > height ||
			   !isLowering && worldData.heightMap[x,y] < height){
				worldData.heightMap[x,y] = height;
				worldData.heightMapFloats[x,y] = (worldData.heightMap[x,y] + worldData.heightMapSize / 2)  / (float)worldData.heightMapSize ;

				// UNITY BUG
				//Due to a unity bug this does not update the terrain collider. Until that is fixed we have to update the ENTIRE terrain
				//for any small change to the heightmap to take effect. I will change this when Unity gets its **** together...
				//floatCache[0,0] = heightMapFloats[x,y];
				//terrain.terrainData.SetHeights(y,x, floatCache);

			}else{
				//did nothing
				return;
			}

			// Check 8-neighbourhood and adapt it to the new change
			for (int i = Mathf.Max(1, x-1); i <= Mathf.Min(worldData.heightMapSize-1, x+1); i++) {
				for (int j = Mathf.Max(1, y-1); j <=Mathf.Min(worldData.heightMapSize-1, y+1); j++) {
					if(worldData.heightMap[i,j] > height + 1){
						terraformQueue.Enqueue(new TerraformOrder(i,j,height + 1, isLowering));
					}else if(worldData.heightMap[i,j] < height - 1){
						terraformQueue.Enqueue(new TerraformOrder(i,j,height - 1, isLowering));
					}
				}	
			}

			//Bulldoze the 4 affected tiles
			BullDoze(y,x);
			BullDoze(y-1,x);
			BullDoze(y,x-1);
			BullDoze(y-1,x-1);

		}

		void BullDoze(int x, int y){
			if(x <= 1 || y <= 1 || x >= tileMapSize || y >= tileMapSize)
				return;

			int key = GetTileCoordKey(x, y);
			tilesToRemoveTrees.Add(key);

			// Draw some brown where we just bulldozed
			float[,,] bulldozedPatch = new float[1,1,terrain.terrainData.alphamapLayers];//Could cache this if necessary but then make sure number of alphamaplayers don't change!
			bulldozedPatch[0,0,terrainGrassTextureIndex] = 0;
			bulldozedPatch[0,0,terrainDirtTextureIndex] = 1;
			terrain.terrainData.SetAlphamaps(x,y, bulldozedPatch);

			// Set the tile to regrow over time
			regrowingTiles[GetTileCoordKey(x,y)] = new Regrowth(x, y, Time.time, Time.time + worldData.timeToRegrowGrass); 
		}
		 #endregion


		#region Generation
		/// <summary>
		/// Generate the world
		/// 
		/// doCreateNewWorldData = true if you need a new world,
		/// if you just loaded on and need to instantiate it then 
		/// doCreateNewWorldData = false
		/// </summary>
		/// <param name="doCreateNewWorldData">If set to <c>true</c> do create new world data procedurally, otherwise create according
		/// to what is in worldData.</param>
		public void Generate(bool doCreateNewWorldData){
            //We use this to store current random seed when we need to set it to a predetermined value
            Random.State oldSeed;
            StopAllCoroutines();
            foreach (var tree in growingTrees) {
                Destroy(tree.Value.gameObject);
            }
            growingTrees.Clear();
			terraformQueue.Clear();
			regrowingTiles.Clear();
			tilesToRemoveTrees.Clear();
			forestedAreas.Clear();

			worldData.heightMapSize = Mathf.Clamp(worldData.heightMapSize,33,513);
			worldData.heightMapSize = Mathf.ClosestPowerOfTwo(worldData.heightMapSize - 1)+1;

			//heightMap = new int[heightMapSize,heightMapSize];
			worldData.heightMapFloats = new float[worldData.heightMapSize,worldData.heightMapSize];

			int highestPoint = int.MinValue;
			int lowestPoint = int.MaxValue;
			float averageHeight = 0;

			//SETTINGS

			terrain.terrainData.heightmapResolution = worldData.heightMapSize;
			terrain.terrainData.alphamapResolution = tileMapSize;
			terrain.terrainData.baseMapResolution = tileMapSize;
			terrain.terrainData.SetDetailResolution(tileMapSize,8);
			terrain.terrainData.size = new Vector3((tileMapSize)*worldData.tileWidth, worldData.heightPerTile * (tileMapSize)*worldData.tileWidth,(tileMapSize)*worldData.tileWidth);
			terrain.heightmapMaximumLOD = 0;
			terrain.Flush();

			//GENERATE MAPS
			if(doCreateNewWorldData){
                worldData.soilFertilityRandomSeed = Random.Range(int.MinValue, int.MaxValue) ;
                worldData.treeMapsRandomSeed = Random.Range(int.MinValue, int.MaxValue);

                worldData.heightMap = MapGenerator.DiamondSquare(worldData.heightMapSize, worldData.heightRandomType, worldData.hillyness);

				worldData.removedTrees.Clear();
				worldData.treeMaps.Clear();

                oldSeed = Random.state;
                Random.InitState( worldData.treeMapsRandomSeed);
                for (int i = 0; i < treeTypes.Count; i++) {
					worldData.treeMaps.Add(MapGenerator.CalcPerlinNoiseMap(tileMapSize));
				}
                Random.state = oldSeed;
				
			}

			#region Heightmap
			//TERRAIN HEIGHT
			for (int x = 0; x < worldData.heightMapSize; x++) {
				for (int y = 0; y < worldData.heightMapSize; y++) {
					if(x == 0 || y == 0 ||x == worldData.heightMapSize-1 || y == worldData.heightMapSize-1){
						worldData.heightMapFloats[x,y] = 0;
					}else{
						worldData.heightMapFloats[x,y] = (worldData.heightMap[x,y] + worldData.heightMapSize / 2)  / (float)worldData.heightMapSize ;
						averageHeight += worldData.heightMap[x,y] / ((float)(tileMapSize) * (tileMapSize));
						if(worldData.heightMap[x,y] > highestPoint)
							highestPoint = worldData.heightMap[x,y];
						if(worldData.heightMap[x,y] < lowestPoint)
							lowestPoint = worldData.heightMap[x,y];
					}
				}
			}
			
			
			terrain.terrainData.SetHeights(0,0, worldData.heightMapFloats);
			#endregion

			#region Water
			// WATER HEIGHT
			if(worldData.wateryness > 0.5f){
				worldData.waterLevel = Mathf.RoundToInt(Mathf.Lerp(averageHeight, highestPoint-1, 2 * (worldData.wateryness - 0.5f)));
			}else{
				worldData.waterLevel = Mathf.RoundToInt(Mathf.Lerp(lowestPoint, averageHeight - 1, 2 * worldData.wateryness));
			}
			if(water != null){
				Vector3 waterScale = terrain.terrainData.size;
				//waterScale.y = worldData.heightMapSize * (2 * worldData.heightPerStep) + worldData.waterLevel / worldData.heightPerStep;
				waterScale.y = worldData.heightPerTile *  (worldData.heightMapSize * worldData.tileWidth / 2f + worldData.waterLevel * worldData.tileWidth + 0.25f);
				water.localScale = waterScale;
				
				Vector3 waterPos = terrain.terrainData.size / 2f;
				Debug.Log("Highest point: " + highestPoint + " Lowest point: " + lowestPoint + " Average: " + averageHeight);
				
				waterPos.y = waterScale.y/2 - 1;
				water.position = waterPos + terrain.transform.position;
				Debug.Log("Water level: " + waterPos.y + " integer water level: " + worldData.waterLevel );
			}
             
			#endregion
			
			#region Tree creation
			List<TreeInstance> trees = new List<TreeInstance>();
            oldSeed = Random.state;
            Random.InitState( worldData.treeMapsRandomSeed);
			for (int x = 1; x < tileMapSize-1; x++) {
				for (int y = 1; y < tileMapSize-1; y++) {
					//Create trees in this tile if they should be there
					int key = GetTileCoordKey( x, y);
					if(worldData.removedTrees.Contains(key))
						continue;

					for (int i = 0; i < treeTypes.Count; i++) {
						TreeType treeType = treeTypes [i];
						float lifeForce = GetTreeLifeForce(x,y, i);
						
						if(lifeForce > 0){
							forestedAreas.Add(key);
							TreeInstance newTree = GetRandomTree(x, y, lifeForce, treeType);
							trees.Add(newTree);
						}
					}
				}
			}
			terrain.terrainData.treeInstances = trees.ToArray();

            Random.state = oldSeed;
			#endregion

			#region Splatmaps
			oldSeed = Random.state;
			Random.InitState( worldData.soilFertilityRandomSeed);
			//worldData.soilFertilityMap = MapGenerator.CalcPerlinNoiseMap(tileMapSize, 5f, -1f, 1f);
			worldData.soilFertilityMap = MapGenerator.CalcPerlinNoiseMap(tileMapSize, 5f, -1f, 1f);
			Random.state = oldSeed;
			//PAINT EDGES BROWN
			float[,,] splatMapData = new float[terrain.terrainData.alphamapWidth,terrain.terrainData.alphamapHeight, terrain.terrainData.alphamapLayers];
			
			for (int x = 0; x < terrain.terrainData.alphamapResolution; x++) {
				for (int y = 0; y < terrain.terrainData.alphamapResolution; y++) {
					//If on an edge then make it brown
					if(x <= 1 || y <= 1 ||x >= terrain.terrainData.alphamapResolution-2 || y >= terrain.terrainData.alphamapResolution-2){
						splatMapData[x,y,terrainGrassTextureIndex] = 0f;
                        splatMapData[x, y, terrainEdgeTextureIndex] = 1f;
                        splatMapData[x,y,terrainDirtTextureIndex] = 0f;
					}else{
						//Here different biomes could be drawn if you want

						
						if(IsTileAboveWater(y,x)){
							float  fertility = GetSoilFertility(y,x);
							splatMapData[x,y,terrainGrassTextureIndex] = fertility;
							splatMapData[x,y,terrainDirtTextureIndex] = 1-fertility;
						}else{
							splatMapData[x,y,terrainGrassTextureIndex] = 0f;
							splatMapData[x,y,terrainEdgeTextureIndex] = 0f;
							splatMapData[x,y,terrainDirtTextureIndex] = 1f;
						}
					}
				}
			}
			
			terrain.terrainData.SetAlphamaps(0,0, splatMapData);
			#endregion

			//Camera.main.transform.position = new Vector3(0, 1.5f *  worldData.heightMapSize +  worldData.waterLevel /  worldData.heightPerStep, 0);

			if(OnNewWorldWasGenerated != null)
				OnNewWorldWasGenerated();
			if(OnWorldWasEdited != null)
				OnWorldWasEdited();

			StartCoroutine(RegrowGrass());
			StartCoroutine(RegrowTrees());
			StartCoroutine(Terraform());
			StartCoroutine(RemoveTrees());
		}


		float GetTreeLifeForce(int x, int y, int treeTypeIndex){
			TreeType treeType = treeTypes [treeTypeIndex];
			//Normalize treemaps so that different treetypes compete by using a total
			float total = 0;
			for (int i = 0; i < treeTypes.Count; i++) {
				total += worldData.treeMaps[i][x,y];
			}
			float lifeForce = worldData.treeMaps[treeTypeIndex][x,y];
			if(total > 1)
				lifeForce /= total;

			float height = GetTileHeightOverWater(x,y);
			if(height >= treeType.lowestHeightOverSea &&
			   height <= treeType.highestHeightOverSea)
				lifeForce += treeType.GetOptimality(height);
			else
				lifeForce = 0;
			
			lifeForce += worldData.treeyness - 1;

			return Mathf.Max(0, lifeForce);
		}

		TreeInstance GetRandomTree(int x, int y, float lifeForce, TreeType treeType){
			TreeInstance newTree = new TreeInstance();
			newTree.prototypeIndex = treeType.treePrototypeIndex;
			//Size
			newTree.heightScale = treeType.scale * ((lifeForce) + Random.Range(0.1f,0.2f));
			newTree.widthScale = newTree.heightScale * Random.Range(0.9f,1.1f);
			//Color
			float colorIntensity = treeType.baseColorIntensity + Random.Range(-treeType.colorVariation, treeType.colorVariation);
			newTree.color = new Color(colorIntensity,colorIntensity,colorIntensity);
			newTree.lightmapColor = new Color(colorIntensity,colorIntensity,colorIntensity);
			
			//Position
			Vector3 v = new Vector3((x+Random.Range(0.1f,0.9f))/(float)tileMapSize , 0, (y+Random.Range(0.1f,0.9f))/(float)tileMapSize );
			v.y = terrain.terrainData.GetInterpolatedHeight(v.x, v.z)/terrain.terrainData.size.y;
			newTree.position = v;

			return newTree;
		}

		void PlantTree(TreeInstance tree){
			GameObject treePrefab = terrain.terrainData.treePrototypes[tree.prototypeIndex].prefab;
			Vector3 pos = tree.position;
			pos.x *= terrain.terrainData.size.x;
			pos.y *= terrain.terrainData.size.y;
			pos.z *= terrain.terrainData.size.z;
			GameObject growingTree = GameObject.Instantiate(treePrefab, pos, Quaternion.identity) as GameObject;
			growingTree.name = "Growing " + treePrefab.name;
			growingTree.transform.SetParent(terrain.transform);
			growingTree.transform.localScale = Vector3.zero;
			GrowingTreeBehaviour growingTreeBehaviour = growingTree.AddComponent<GrowingTreeBehaviour>();
			growingTreeBehaviour.treeInstance = tree;
			growingTreeBehaviour.regrowthRate = worldData.treeRegrowthRate * Random.Range (0.9f, 1.1f);
            int key = GetTileCoordKey (pos.x, pos.z);
            growingTreeBehaviour.key = key;
            growingTreeBehaviour.OnFinishedGrowing += HandleTreeFinishedRegrowing;

			growingTrees[key] = growingTreeBehaviour;
			forestedAreas.Add(key);
		}

        void HandleTreeFinishedRegrowing(GrowingTreeBehaviour tree) {
            //Add the tree to the unity terrain instead
            //(when growing it was a normal gameobject, now we can set it as a statis terrain object)
            terrain.AddTreeInstance(tree.treeInstance);
            growingTrees.Remove(tree.key);
            GameObject.Destroy(tree.gameObject);
        }

		public void Save(string saveName){
			SaveDataManagement.SaveData<WorldData>(worldData, saveName);
		}

		public void Load(string saveName){
			if(SaveDataManagement.HasSavedData(saveName)){
				worldData = SaveDataManagement.LoadData<WorldData>(saveName);
				Generate(false);
			}
		}


		#endregion


		#region Coroutines
		IEnumerator RemoveTrees(){
			WaitForSeconds wait = new WaitForSeconds(0.1f);
			while(true){
				
				if(terraformQueue.Count == 0 && tilesToRemoveTrees.Count > 0){
					
					List<int> treeIndicesToRemove = new List<int>();
					List<TreeInstance> trees = new List<TreeInstance>( terrain.terrainData.treeInstances);

					foreach (var item in tilesToRemoveTrees) {
						if(growingTrees.ContainsKey(item) && growingTrees[item] != null){
							Destroy(growingTrees[item].gameObject);
							worldData.removedTrees.Add(item);
							forestedAreas.Remove(item);
						}
					}

					for (int i = 0; i < trees.Count; i++) {
						TreeInstance tree = trees[i];
						float treeX = tree.position.x * tileMapSize ;
						float treeY = tree.position.z * tileMapSize ;
						int key = GetTileCoordKey(Mathf.FloorToInt(treeX), Mathf.FloorToInt(treeY));
						
						if(tilesToRemoveTrees.Contains(key)){
							treeIndicesToRemove.Add(i);
							worldData.removedTrees.Add(key);
							forestedAreas.Remove(key);
						}
						
					}
					tilesToRemoveTrees.Clear();
					
					//Working backwards through the list so we don't screw up the indices, remove trees...
					for (int i = treeIndicesToRemove.Count-1; i >= 0; i--) {
						//Debug.Log(trees[treeIndicesToRemove[i]] + " " + treeIndicesToRemove[i]);
						trees.RemoveAt(treeIndicesToRemove[i]);
					}
					
					terrain.terrainData.treeInstances = trees.ToArray();
					
					// Even though this is a costly operation we do not yield during it as that could create race conditions where
					// indices are changed
				}
				
				yield return wait;
			}
		}

		IEnumerator RegrowTrees(){
			WaitForSeconds wait = new WaitForSeconds(1f);
			HashSet<int> replantedTrees = new HashSet<int> ();
			while (true) {
                //We don't even run this if there are more pressing matters at hand
				if(terraformQueue.Count == 0 && tilesToRemoveTrees.Count == 0){
                    //Check for each tree that has been removed if it should be replanted
                    foreach (var item in worldData.removedTrees) {

                        int x = GetTileXFromKey(item);
                        int y = GetTileYFromKey(item);
                        //Trees will only start growing next to another tree that can seed the ground
                        bool hasTreeNeighbour = HasAdjacentTree(x, y);
                        //Trees will only grow in empty tiles
                        bool isTileEmpty = IsTileEmpty(x, y);
                        //Trees will not grow in tiles where there is a blocking physics item, e.g. a player standing there
                        bool isEmptyFromPhysics = IsTileEmptyFromPhysics(x, y);
                        if (!hasTreeNeighbour || !isTileEmpty || !isEmptyFromPhysics)
                            continue;

                        for (int i = 0; i < treeTypes.Count; i++) {
                            if (Random.Range(0f, 1f) < worldData.treeRegrowChance) {//We COULD add a variable to the tree type for regrow chance if we wanted to...
                                //Need to check again for forest in case there was several types of trees growing here
                                if (DoTileHaveForest(x, y))
                                    continue;

                                TreeType treeType = treeTypes[i];
                                float lifeForce = GetTreeLifeForce(x, y, i);

                                if (lifeForce > 0) {
                                    TreeInstance newTree = GetRandomTree(x, y, lifeForce, treeType);
                                    PlantTree(newTree);
                                    replantedTrees.Add(item);
                                }
                            }
                        }
                        
                    }
					
					foreach (var item in replantedTrees) {
						worldData.removedTrees.Remove(item);
					}
					replantedTrees.Clear();
				}
				yield return wait;
			}
		}

		IEnumerator RegrowGrass(){
			List<int> finishedHashes = new List<int>();
			float[,,] currentGrowth = new float[1,1,terrain.terrainData.alphamapLayers];
			
			long allowedTimePerIteration = 1000/120; //Set this to the 1000/x fps you need
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			int currentIndex = 0;
			
			WaitForSeconds wait = new WaitForSeconds(0.25f);
			while(true){
				while(regrowingTiles.Count == 0)
					yield return wait;
				
				//float startTime = Time.time;
				stopwatch.Start();
				int handledThisIteration = 0;
				
				
				if(currentIndex >= regrowingTiles.Values.Count)
					currentIndex = 0;
				int count = 0;
				foreach (var keyValPair in regrowingTiles) {
					//We use this to skip the ones recently handled so that every tile gets a chance to regrow, not just the first ones in the dict
					count++;
					if(count < currentIndex)
						continue;
					Regrowth regrowth = keyValPair.Value;
					// In order to avoid synching trouble we can't do this work if there is terraformin going on...
					if(terraformQueue.Count > 0)
						break;
					
					int x = regrowth.x;
					int y = regrowth.y;
					
					//If this tile is under water then do not let it regrow
					if(!IsTileAboveWater(x,y)){
						finishedHashes.Add(keyValPair.Key);
						continue;
					}
					
					float ratio =( Time.time- regrowth.startTime) / (regrowth.endTime - regrowth.startTime);
					ratio = Mathf.Clamp01(ratio);
					float soilFertility = GetSoilFertility(x,y);
					if(ratio >= soilFertility){
						ratio = soilFertility;
						finishedHashes.Add(keyValPair.Key);
					}
					
					currentGrowth[0,0,terrainGrassTextureIndex] = ratio;
					currentGrowth[0,0,terrainDirtTextureIndex] = (1-ratio);
					
					terrain.terrainData.SetAlphamaps(x,y, currentGrowth);
					handledThisIteration++;
					currentIndex++;
					if(currentIndex >= regrowingTiles.Values.Count)
						currentIndex = 0;
					
					// If we spent too much time here then yield
					if(stopwatch.ElapsedMilliseconds >= allowedTimePerIteration){
						//Debug.Log("Had only time to regrow " + handledThisIteration +  " then had to yield. " );
						stopwatch.Stop();
						stopwatch.Reset();
						handledThisIteration = 0;
						yield return null;
						stopwatch.Start();
					}
				}
				
				foreach (var key in finishedHashes) {
					regrowingTiles.Remove(key);
				}
				
				finishedHashes.Clear();
				stopwatch.Stop();
				stopwatch.Reset();
				
				if(regrowingTiles.Count == 0)
					yield return wait;
				else
					yield return null;
			}
		}
		
		IEnumerator Terraform(){
			
			//long allowedTimePerIteration = 1000/heightMapSize; //Set this to the 1000/x fps you need
			
			/* Note that since we can't use amortized processing properly due to unity terrain engine bug (you can't assign small areas of heightmap,
			 * only assignments to THE ENTIRE HEIGHTMAP will rebuild the terrain collider, there will be an extra overhead cost after 
			 * each loop from applying changes to heightmap so this FPS will in reality be much lower especially on large maps.
			 * 
			 * The bug ir reported and will hopefully be handled: 
			 * 
			*/
			long allowedTimePerIteration = 1000/60; //Set this to the 1000/x fps you need
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			
			WaitForSeconds wait = new WaitForSeconds(0.1f);
			
			while(true){
				stopwatch.Start();
				
				int handledThisIteration = 0;
				bool isHeightMapDirty = false;
				while(terraformQueue.Count > 0){
					TerraformOrder order = terraformQueue.Dequeue();
					
					switch (order.orderType) {
					case TerraformOrder.OrderType.BULLDOZE:
						BullDoze(order.x, order.y);
						break;
					case TerraformOrder.OrderType.BULLDOZE_AREA:
						// Bulldoze the explicit area
						for (int x = order.x; x < order.xStop; x++) {
							for (int y = order.y; y < order.yStop; y++) {
								BullDoze(x,y);
							}	
						}
						break;
					case TerraformOrder.OrderType.RAISE_LOWER:
						SetTerrainHeight(order.x,order.y,order.desiredHeight, order.isLowering);
						isHeightMapDirty = true;
						break;
					case TerraformOrder.OrderType.LEVEL:
						//Debug.Log("x " + order.x + " x stop " + order.xStop +" y " + order.y +" y stop " + order.yStop);
						
						// Flatten the explicit area
						for (int x = order.x; x <= order.xStop; x++) {
							for (int y = order.y; y <= order.yStop; y++) {
								//Change height if needed
								if(worldData.heightMap[x,y] != order.desiredHeight){
									worldData.heightMap[x,y] = order.desiredHeight;
									worldData.heightMapFloats[x,y] = (worldData.heightMap[x,y] + worldData.heightMapSize / 2)  / (float)worldData.heightMapSize ;
									
									//Now bulldoze those areas that are affected
									BullDoze(y,x);
									
									//Also handle edge cases...
									if(x == order.x || y == order.y){
										BullDoze(y-1,x);
										BullDoze(y,x-1);
										BullDoze(y-1,x-1);
									}
								}
							}	
						}
						
						// Set all heights that are affected by this flattening
						#region Set adjacent heights
						for (int x = order.x - 1; x <= order.xStop + 1; x++) {
							int y = order.y - 1;
							if(x > 0 && y > 0 && x < worldData.heightMapSize -1 && y < worldData.heightMapSize -1 && Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1){
								if(worldData.heightMap[x,y] > order.desiredHeight){
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight + 1, true));
								}else{
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight - 1, false));
								}
							}
							y = order.yStop + 1;
							if(x > 0 && y > 0 && x < worldData.heightMapSize -1 && y < worldData.heightMapSize -1 && Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1){
								if(worldData.heightMap[x,y] > order.desiredHeight){
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight + 1, true));
								}else{
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight - 1, false));
								}
							}
						}
						
						for (int y = order.y - 1; y <= order.yStop + 1; y++) {
							int x = order.x - 1;
							if(x > 0 && y > 0 && x < worldData.heightMapSize -1 && y < worldData.heightMapSize -1 && Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1){
								if(worldData.heightMap[x,y] > order.desiredHeight){
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight + 1, true));
								}else{
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight - 1, false));
								}
							}
							x = order.xStop + 1;
							if(x > 0 && y > 0 && x < worldData.heightMapSize -1 && y < worldData.heightMapSize -1 && Mathf.Abs(worldData.heightMap[x,y] - order.desiredHeight) > 1){
								if(worldData.heightMap[x,y] > order.desiredHeight){
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight + 1, true));
								}else{
									terraformQueue.Enqueue(new TerraformOrder(x,y,order.desiredHeight - 1, false));
								}
							}
						}
						#endregion

						isHeightMapDirty = true;
						break;
					
					default:
						break;
					}
					
					handledThisIteration++;
					
					// If we spent too much time here then yield
					if(stopwatch.ElapsedMilliseconds >= allowedTimePerIteration){
						Debug.Log("Had only time to terraform " + handledThisIteration +  " then had to yield." );
						stopwatch.Stop();
						stopwatch.Reset();
						handledThisIteration = 0;

						if(isHeightMapDirty){
							terrain.terrainData.SetHeights(0,0, worldData.heightMapFloats);
							isHeightMapDirty = false;
						}

						if(OnWorldWasEdited != null)
							OnWorldWasEdited();
						yield return null;
						stopwatch.Start();
					}
				}
				
				if(handledThisIteration > 0){
					if(isHeightMapDirty){
						terrain.terrainData.SetHeights(0,0, worldData.heightMapFloats);
						isHeightMapDirty = false;
					}
					if(OnWorldWasEdited != null)
						OnWorldWasEdited();
				}
				
				stopwatch.Stop();
				stopwatch.Reset();
				yield return wait;
			}
		}
		#endregion


		#region private structs
		private struct Regrowth{
			public float startTime;
			public float endTime;
			public int x;
			public int y;
			
			public Regrowth(int x, int y, float startTime, float endTime){
				this.x = x;
				this.y = y;
				this.startTime = startTime;
				this.endTime = endTime;
			}
		}
		
		private struct TerraformTileCost{
			public int x;
			public int y;
			public int deltaHeight;
			
			public TerraformTileCost(int x, int y, int deltaHeight){
				this.x = x;
				this.y = y;
				this.deltaHeight = deltaHeight;
			}
		}
		
		internal struct TerraformOrder{
			public int x;
			public int y;
			public int xStop;
			public int yStop;
			public int desiredHeight;
			/// <summary>
			/// This variable is used in amortized terraforming to not get race conditions between different Terraform orders.
			/// Essentially if an order has the purpose to raise the terrain it can never lower it and vice versa
			/// </summary>
			public bool isLowering;
			public OrderType orderType;
			
			public enum OrderType{
				RAISE_LOWER,
				LEVEL,
				BULLDOZE,
				BULLDOZE_AREA,
			}
			/// <summary>
			/// Creates an order to raise or lower terrain in one spot
			/// </summary>
			public TerraformOrder(int x, int y, int desiredHeight, bool isLowering){
				this.x = x;
				this.y = y;
				this.xStop = 0;
				this.yStop = 0;
				this.desiredHeight = desiredHeight;
				this.isLowering = isLowering;
				this.orderType = OrderType.RAISE_LOWER;
			}
			/// <summary>
			/// Creates an order to bulldoze one tile
			/// </summary>
			public TerraformOrder(int x, int y){
				this.x = x;
				this.y = y;
				this.xStop = x;
				this.yStop = y;
				this.desiredHeight = 0;
				this.isLowering = false;
				this.orderType = OrderType.BULLDOZE;
			}

			/// <summary>
			/// Creates an order to bulldoze an area of several tiles
			/// </summary>
			public TerraformOrder(int x, int y, int xStop, int yStop){
				this.x = x;
				this.y = y;
				this.xStop = xStop;
				this.yStop = yStop;
				this.desiredHeight = 0;
				this.isLowering = false;
				this.orderType = OrderType.BULLDOZE_AREA;
			}

			/// <summary>
			/// Creates an order to level terrain in an area
			/// </summary>
			public TerraformOrder(int x, int y, int xStop, int yStop, int desiredHeight){
				this.x = x;
				this.y = y;
				this.xStop = xStop;
				this.yStop = yStop;
				this.desiredHeight = desiredHeight;
				this.isLowering = false;
				this.orderType = OrderType.LEVEL;
			}
		}
		
		#endregion

	}
}

