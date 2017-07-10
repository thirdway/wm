using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TycoonTerrain{
public class SimpleBuildingManager : MonoBehaviour {
		protected static SimpleBuildingManager instance;
		public static SimpleBuildingManager Instance{
			get{
				return instance;
			}
		}

		[Tooltip("The building types that should be available in the world. Note that when the world saves and loads the indices of this list is used. \nTo add new building types go to \n\n Assets->Tycoon Terrain->Create new building type \n\n You new building type scriptable object will be in the folder Assets/Tycoon Terrain/Game Data/BuildingTypes")]
		public List<SimpleBuildingType> buildingTypes;
		public System.Action OnBuildingsChanged;

		[Tooltip("Link to the world here.")]
		public WorldBehaviour world;

		[Tooltip(@"Construction utilities are stuff you want to add when constructing a building. It could be smoke generators or progress meters,
					scaffolding or whatever. This will be added as a child to the building's under onstruction game object, if it is not null.
					You can leave this as null if yyou do not wish to use it.")]
		public GameObject constructionUtilitiesPrefab;
		/// <summary>
		/// Stores a reference to buildings using its position as key. Note that this only stores one reference, not
		/// one reference per tile that building occupies
		/// </summary>
		public Dictionary<int, SimpleBuildingBehaviour> buildings = new Dictionary<int, SimpleBuildingBehaviour>();
		/// <summary>
		/// This dict stores a pointer a building for every tile that the building occupies. 
		/// IMPORTANT: Note the difference between this and the buildings which stores a pointer to a building ONLY if its center is in the tile!
		/// </summary>
		protected Dictionary<int, SimpleBuildingBehaviour> buildingTiles = new Dictionary<int, SimpleBuildingBehaviour>();
		/// <summary>
		/// Buildings placed in scene by designer
		/// 
		/// As these are never removed, loaded or saved we handle these separately
		/// </summary>
		protected List<SimpleBuildingBehaviour> sceneBuildings = new List<SimpleBuildingBehaviour>();

		void Awake(){
			instance = this;
		}

		// Use this for initialization
		void Start () {
		
		}
		
		// Update is called once per frame
		void Update () {
		
		}

		#region Save and Load
		public virtual void Save(string saveName){
			SaveDataManagement.SaveData<SimpleBuildingInstance[]>(GetBuildingInstances(), saveName);
		}
		
		public virtual void Load(string saveName){
			if(SaveDataManagement.HasSavedData(saveName)){
				SimpleBuildingInstance[] buildings = SaveDataManagement.LoadData<SimpleBuildingInstance[]>(saveName);
				foreach (var buildingInstance in buildings) {
					MapTile tile = world.GetTileFromKey(buildingInstance.key);
					InstantiateBuilding(tile.x, tile.y, buildingInstance);
				}
			}
		}

		/// <summary>
		/// We can't serialize dictionary so we transform to an array before saving. Don't use this in any other context
		/// </summary>
		SimpleBuildingInstance[] GetBuildingInstances(){
			SimpleBuildingInstance[] items = new SimpleBuildingInstance[this.buildings.Count];
			int counter = 0;
			foreach (var item in buildings) {
				item.Value.buildingInstanceData.key = item.Key;
				items[counter] = item.Value.buildingInstanceData;
				counter++;
			}
			
			return items;
		}

		public void Restart(){
			foreach (var building in buildings) {
				Destroy(building.Value.gameObject);
			}
			
			buildingTiles.Clear();
			buildings.Clear();
			StopAllCoroutines();
			foreach (var item in sceneBuildings) {
				item.Setup();
			}
		}

		#endregion Save and load

		#region Queries


		public void IsBuildingPositionAllowed(int x, int y, int direction, SimpleBuildingType building, out bool[,] tilesResults){
			
			int stopX = x + building.buildArea.width - 1;
			int stopY = y + building.buildArea.length - 1;
			
			//If we turned the building -90 or 90 degrees then switch width and length
			bool isTurned = direction % 2 == 1;
			if(isTurned){
				stopX = x + building.buildArea.length - 1;
				stopY = y + building.buildArea.width - 1;
				tilesResults = new bool[building.buildArea.length,building.buildArea.width];
			}else{
				tilesResults = new bool[building.buildArea.width,building.buildArea.length];
			}
			
			if(x <= 0 || y <= 0 || stopX >= world.tileMapSize -1 || stopY >= world.tileMapSize -1)
				return;
			
			for (int xT = x; xT <= stopX ; xT++) {
				for (int yT = y; yT <= stopY ; yT++) {
					MapTile tile = new MapTile(xT, yT);
					if(DoTileHaveBuilding(xT,yT))
						continue;
					
					//Compare this tile type to the correct corresponding one on building
					
					//First find local coordinates for building tiles
					int xLoc;
					int yLoc;
					switch (direction) {
					case 0:
						xLoc = xT - x;
						yLoc = yT - y;
						break;
					case 1:
						xLoc = yT - y;
						yLoc = xT - x;
						break;
					case 2: 
						xLoc = building.buildArea.width - (xT - x) - 1;
						yLoc = building.buildArea.length - (yT - y) - 1;
						break;
					case  3:
						xLoc = building.buildArea.width - (yT - y) - 1;
						yLoc = building.buildArea.length - (xT - x) - 1;
						break;
					default:
						xLoc = -1;
						yLoc = -1;
						break;
					}
					
					//now that we have the local coordinates of the specified tile we can compare it to the building allowed tile for said coordinate
					//Get the tile type
					MapTile.MapTileType worldTileType = world.GetTileType(tile);
					//Get the tile type
					MapTile.MapTileType buildingTileType = building.buildArea.GetTileType(xLoc, yLoc);
					
					//Check if this tile is ok
					if(MapTile.IsTileOk(buildingTileType, worldTileType) && world.IsTileEmpty(tile))
						tilesResults[xT - x,yT - y] = true;
					else
						tilesResults[xT - x,yT - y] = false;
				}
			}
			
		}

		/// <summary>
		/// Determines whether this building position is allowed at the specified x, y and direction. The rules set for this building
		/// (eg can it be placed on a slope) are applied.
		/// </summary>
		/// <returns><c>true</c> if this building position is allowed otherwise, <c>false</c>.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="building">Building.</param>
		public bool IsBuildingPositionAllowed(int x, int y, int direction, SimpleBuildingType building){
			
			int stopX = x + building.buildArea.width - 1;
			int stopY = y + building.buildArea.length - 1;
			
			if(x <= 0 || y <= 0 || stopX >= world.tileMapSize -1 || stopY >= world.tileMapSize -1)
				return false;
			
			//If we turned the building -90 or 90 degrees then switch width and length
			bool isTurned = direction % 2 == 1;
			if(isTurned){
				stopX = x + building.buildArea.length - 1;
				stopY = y + building.buildArea.width - 1;
			}
			
			for (int xT = x; xT <= stopX ; xT++) {
				for (int yT = y; yT <= stopY ; yT++) {
					MapTile tile = new MapTile(xT, yT);
					if(DoTileHaveBuilding(xT,yT))
						return false;
					
					//Compare this tile type to the correct corresponding one on building
					
					//First find local coordinates for building tiles
					int xLoc;
					int yLoc;
					switch (direction) {
					case 0:
						xLoc = xT - x;
						yLoc = yT - y;
						break;
					case 1:
						xLoc = yT - y;
						yLoc = xT - x;
						break;
					case 2: 
						xLoc = building.buildArea.width - (xT - x) - 1;
						yLoc = building.buildArea.length - (yT - y) - 1;
						break;
					case  3:
						xLoc = building.buildArea.width - (yT - y) - 1;
						yLoc = building.buildArea.length - (xT - x) - 1;
						break;
					default:
						xLoc = -1;
						yLoc = -1;
						break;
					}
					
					//now that we have the local coordinates of the specified tile we can compare it to the building allowed tile for said coordinate
					//Get the tile type
					MapTile.MapTileType worldTileType = world.GetTileType(tile);
					//Get the tile type
					MapTile.MapTileType buildingTileType = building.buildArea.GetTileType(xLoc, yLoc);
					
					//Check if this tile is ok
					if(MapTile.IsTileOk(buildingTileType, worldTileType))
						continue;
					else
						return false;
				}
			}
			
			if(!world.IsAreaEmpty(x, y, stopX, stopY) && this.IsAreaEmpty(x, y, stopX, stopY)){
				Debug.Log("Area not empty");
				return false;
			}
			
			/* Redundant now?
			if(!world.IsAreaAboveWater(x, y, stopX, stopY)){
				Debug.Log("Area not above water");
				return false;
			}*/
			
			return true;
		}

		public bool DoTileHaveBuilding(int x, int y){
			int key = world.GetTileCoordKey(x,y);
			return buildingTiles.ContainsKey(key);
		}
		
		public SimpleBuildingBehaviour GetBuildingInTile(int x, int y){
			int key = world.GetTileCoordKey(x,y);
			if(!buildingTiles.ContainsKey(key))
				return null;
			else
				return buildingTiles[key];
		}

		public bool IsAreaEmpty(int startX, int startY, int stopX, int stopY){
			for (int x = startX; x <= stopX; x++) {
				for (int y = startY; y <= stopY; y++) {
					if(DoTileHaveBuilding(x,y))
						return false;
				}	
			}
			
			return true;
		}

		/// <summary>
		/// Checks if any of the tiles in a hashset has a building
		/// </summary>
		/// <returns><c>true</c>, if tile have building was done, <c>false</c> otherwise.</returns>
		/// <param name="tileKeys">Tile keys.</param>
		public bool DoTileHaveBuilding(HashSet<int> tileKeys){
			foreach (var key in tileKeys) {
				if(buildingTiles.ContainsKey(key))
					return true;
			}
			return false;
		}

		public SimpleBuildingBehaviour GetBuildingByPositionKey(int key){
			if(buildings.ContainsKey(key)){
				return buildings[key];
			}else{
				Debug.LogWarning("Tried to find a building at key " + key.ToString() + " but failed. Check if this is correct.");
				return null;
			}
		}

		#endregion Queries

		#region Adding buildings

		public virtual void ConstructBuildingInTile(int x, int y, int buildingIndex, int direction){
			SimpleBuildingInstance newBuilding = new SimpleBuildingInstance(direction, buildingIndex);

			if(IsBuildingPositionAllowed(x,y,newBuilding.direction,buildingTypes[newBuilding.buildingTypeIndex])){
				int key = world.GetTileCoordKey(x,y);
				newBuilding.key = key;
				InstantiateBuilding(x,y,newBuilding);
			}else{
				Debug.Log("Building placement was rejected even though it had been approved in player controller. Maybe terrain was edited?");
			}
		}

		/// <summary>
		/// Adds a building to the world. This could be through construction or when loading a game.
		/// If you load a game then there might be finished buildings but they still must be properly registered!
		/// </summary>
		/// <param name="building">Building.</param>
		internal void RegisterBuilding(SimpleBuildingBehaviour building){
			building.transform.SetParent(transform);
			if(building.isSceneBuilding){
				if(!sceneBuildings.Contains(building)){ 
					sceneBuildings.Add(building);

					//Set the building to be in a correct tile (you could see this as rounding off the position)
					MapTile tile = world.GetTileFromWorldPosition(building.transform.position);
					Vector3 pos = world.GetWorldPositionFromTile(tile);
					building.transform.position = pos;

					//Next we need to make the tiles occupied with this building so that other vuildings can't be built in the same location
					int width = building.buildingType.buildArea.width;
					int length = building.buildingType.buildArea.length;

					//If we have an even number as width or length then we want the center to be moved by one step
					pos += new Vector3(width % 2 - 1, 0, length % 2 - 1) * world.worldData.tileWidth / 2f;
					
					//Calc tile coords
					int startX = (int)((pos.x - width * 0.5f)/world.worldData.tileWidth) ;
					int startY =(int)((pos.z - length * 0.5f)/world.worldData.tileWidth) ; 
					int stopX = startX + width;
					int stopY = startY + length;

					//We add a reference to this building in every tile it occupies
					for (int i = startX; i <= stopX; i++) {
						for (int j = startY; j <= stopY; j++) {
							int key = world.GetTileCoordKey(i,j);
							buildingTiles.Add(key, building);
						}
					}
				}
			}else{
				buildings.Add(building.buildingInstanceData.key, building);
			}
		}


		/// <summary>
		/// Instantiates the building by actually creating the gameobject and assigning it to the world
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="buildingInstance">Building instance.</param>
		protected void InstantiateBuilding(int x, int y, SimpleBuildingInstance buildingInstance){
			SimpleBuildingType buildingType = buildingTypes[buildingInstance.buildingTypeIndex];
			
			int width = buildingType.buildArea.width;
			int length = buildingType.buildArea.length;
			
			//If we turned the building -90 or 90 degrees then switch width and length
			if(buildingInstance.direction%2 == 1){
				width = buildingType.buildArea.length;
				length = buildingType.buildArea.width;
			}
			

			int startX = x;
			int stopX  = x + width - 1;
			int startY = y; 
			int stopY =  y + length - 1;
			buildingInstance.x = x;
			buildingInstance.y = y;
			
			//Calculate dead center of building area to place the actual game object
			Vector3 pos = (world.GetWorldPositionFromTile(startX,startY,true) + world.GetWorldPositionFromTile(stopX,stopY,true))/2;
			
			GameObject building = (GameObject)GameObject.Instantiate(buildingType.prefab, pos, Quaternion.identity);
			building.name = buildingType.name;

			Collider buildingCollider = building.GetComponent<Collider>();
			if(buildingCollider != null)
				buildingCollider.enabled = true;
			Vector3 scale = building.transform.localScale;
			scale = Vector3.Scale(buildingType.scale, scale);
			building.transform.localScale = scale;
			
			Vector3 rot = building.transform.eulerAngles;
			rot.y = 90 * buildingInstance.direction;
			building.transform.rotation = Quaternion.Euler(rot);
			
			if(buildingType.doLeanWithTerrain){
				if (world.isTileLeaningDownX (x, y))
					building.transform.Rotate (45, 0, 0);
				if (world.isTileLeaningUpX (x, y))
					building.transform.Rotate (-45, 0, 0);
				if (world.isTileLeaningDownY (x, y))
					building.transform.Rotate (-45, 0, 0);
				if (world.isTileLeaningUpY (x, y))
					building.transform.Rotate (45, 0, 0);
			}

			SimpleBuildingBehaviour buildingBehaviour = building.GetComponent<SimpleBuildingBehaviour>();
			buildingBehaviour.positionKey = world.GetTileCoordKey(x,y);
			buildingBehaviour.buildingType = buildingType;
			buildingBehaviour.buildingInstanceData = buildingInstance;
			buildingBehaviour.width = width;
			buildingBehaviour.length = length;
			buildingBehaviour.Setup();

			//Add construction utilities if applicable
			if(buildingBehaviour.underConstructionBuilding != null &&
			   constructionUtilitiesPrefab != null){
				GameObject constructionUtilities = Instantiate<GameObject>(constructionUtilitiesPrefab);
				constructionUtilities.transform.SetParent(buildingBehaviour.underConstructionBuilding.transform);
				constructionUtilities.transform.localPosition = Vector3.zero;
			}

			//We add a reference to this building in every tile it occupies
			for (int i = x; i <= stopX; i++) {
				for (int j = y; j <= stopY; j++) {
					int key = world.GetTileCoordKey(i,j);
					buildingTiles.Add(key, buildingBehaviour);
				}
			}
			
			if(OnBuildingsChanged != null)
				OnBuildingsChanged();
		}
		#endregion Adding buildings

		#region Removing buildings
		public void Bulldoze(int x, int y){
			int key = world.GetTileCoordKey(x, y);
			
			if(buildingTiles.ContainsKey(key)){
				//A building is only stored once in buildings but stored for every tile it occupies in WorldBehaviour.buildings
				SimpleBuildingBehaviour buildingBehaviour = buildingTiles[key];
				RemoveBuilding(buildingBehaviour);
			}
			
		}
		
		public void RemoveBuilding(SimpleBuildingBehaviour buildingBehaviour){
			int buildingPosKey = buildingBehaviour.positionKey;
			buildings.Remove(buildingPosKey);
			
			for (int i = 0; i < buildingBehaviour.width; i++) {
				for (int j = 0; j < buildingBehaviour.length; j++) {
					int tmpKey = world.GetTileCoordKey(buildingPosKey % world.tileMapSize + i, buildingPosKey / world.tileMapSize + j);
					buildingTiles.Remove(tmpKey);
					//Debug.Log("Removing building at x: " + (buildingPosKey % tileMapSize + i) + " y: " + (buildingPosKey / tileMapSize + j));
				}
			}
			
			GameObject.Destroy(buildingBehaviour.gameObject);
			
			if(OnBuildingsChanged != null)
				OnBuildingsChanged();
		}

		public void BulldozeArea(MapTile start, MapTile stop){
			BulldozeArea(start.x, start.y, stop.x, stop.y);
		}

		public void BulldozeArea(int xStart, int yStart, int xStop, int yStop){
			TycoonTerrain.WorldBehaviour.TerraformOrder order = new TycoonTerrain.WorldBehaviour.TerraformOrder(xStart < xStop ? xStart : xStop,
			                                                                                                    yStart < yStop ? yStart : yStop,
			                                                                                                    xStart > xStop ? xStart : xStop,
			                                                                                                    yStart > yStop ? yStart : yStop);
			for (int x = order.x; x < order.xStop; x++) {
				for (int y = order.y; y < order.yStop; y++) {
					Bulldoze(x,y);
				}	
			}
		}
		#endregion Removing buildings
	}
}
