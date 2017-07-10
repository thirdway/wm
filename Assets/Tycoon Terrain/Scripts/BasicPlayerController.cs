using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TycoonTerrain{
	public class BasicPlayerController : MonoBehaviour, IPlayerController {
		public enum TerraFormType{
			NONE,
			BULLDOZE,
			RAISE_TERRAIN,
			LOWER_TERRAIN,
			LEVEL_TERRAIN,
			BULLDOZE_AREA,
			BUILD,
		}
		protected static readonly string BUILDINGS_KEY = "_buildings";
		protected static readonly string CAMERA_KEY = "_camera";
        protected static readonly string RESOURCES_KEY = "_resources";

        /// <summary>
        /// The grid overlay draws lines on the grid for certain terraforming operations such as "bulldoze area"
        /// </summary>
        [Tooltip("The grid overlay draws lines on the grid for certain terraforming operations such as \"bulldoze area\"")]
		public GridOverlay gridOverlay;
		/// <summary>
		/// The tile overlay draws squares on the tile grid for certain terraforming operations such as "construct building"
		/// </summary>
		[Tooltip("The tile overlay draws squares on the tile grid for certain terraforming operations such as \"construct building\"")]
		public TileOverlay tileOverlay;
        [Tooltip("Link to world object")]
        public WorldBehaviour world;
        [Tooltip("Link to building manager object in scene")]
        public SimpleBuildingManager buildingManager;
        [Tooltip("Link to resource manager object in scene")]
        public ResourceManager resourceManager;
        

        [Tooltip("Parent object for all the icons shown in worldspace for terraforming")]
        public PlayerDisplayBehaviour playerDisplay;
        [Tooltip("Soundclip to play when terraforrming")]
        public AudioClip explosionClip;
        [HideInInspector]
		public int currentBuildingIndex = 0;
        [Tooltip("Set a unique value here to use as save name. You probably want to let te player set this at game start or when saving.")]
        public string worldSaveName = "SavedWorld 1";
        [Tooltip("Set to true if you want the game to load directly on start. Otherwise it will generate a new world. If there is no save a new world will be generated anyway.")]
        public bool doLoadOnStart = true;
        [Tooltip("Set to true to get more debug messages to log")]
        public bool doDebug = false;
        [Tooltip("This is the layer that we detect terrain on when user wants to interact with it for e.g. terraforming actions")]
        public LayerMask terrainMask;
        [Tooltip("This is the layer that we detect buildings on when user wants to interact with them for e.g. opening up a building window")]
        public LayerMask buildingMask;
        /// <summary>
        /// The direction that we will build current building in. 0 means normal, 1 means turned 90 degrees right and so on
        /// </summary>
        protected int direction = 0;
		protected TerraFormType currentTerraform = TerraFormType.NONE;
		protected UnityEngine.EventSystems.EventSystem eventSystem;

        #region Events
        public BuildingEvent OnBuildingClicked;
        #endregion

        #region Controls and Input
        protected Vector3 dragStart;
		protected Vector3 dragEnd;
		protected Vector3 currentPointerPos;
		protected MapTile dragStartTile;
		protected MapTile dragStopTile;
		protected MapTile currentTile;
		/// <summary>
		/// Use this to keep track of where user started holding down mouse button
		/// </summary>
		protected Vector3 startPos = Vector3.zero;
		
		/// <summary>
		/// Keep track of wether user is currently interacting eg holding down mousr button to level terrain
		/// </summary>
		protected bool isDragging = false;
		protected bool didRelease = false;
		protected bool didPress = false;
		#endregion Controls and Input

		// Use this for initialization
		protected virtual void Start () {
			eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
			
			if(doLoadOnStart && SaveDataManagement.HasSavedData(worldSaveName)){
				LoadGame();
			}else{
				ResetGame();
			}
		}

		// Update is called once per frame
		void Update () {
			
			//Check if pointer is above a gui object, if it is then we should not interact with the world
			if(eventSystem.IsPointerOverGameObject())
				return;
			
			HandleInput();
			HandleTerraforming();
			
		}



		void HandleInput(){
            if (currentTerraform == TerraFormType.BUILD) {//TODO: Change to something else, not conflicting with WASD
                if (Input.GetKeyDown(KeyCode.A)) {
                    direction--;
                    if (direction <= -1)
                        direction = 3;
                    direction %= 4;
                }
                if (Input.GetKeyDown(KeyCode.D)) {
                    direction++;
                    if (direction <= -1)
                        direction = 3;
                    direction %= 4;
                }
            } 
			didRelease = Input.GetMouseButtonUp(0);
			didPress = Input.GetMouseButtonDown(0);
			isDragging = Input.GetMouseButton(0);

			//Setup raycast
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			//Cast ray to see if we are interacting with terrain
			if (Physics.Raycast(ray, out hit, 2048f, terrainMask)){
				currentPointerPos = GetHeightMapCoords(hit.point) + (IsTileOperation(currentTerraform) ? HalfTileSize() : Vector3.zero);
				currentTile = world.GetTileFromWorldPosition(currentPointerPos);

				if(didPress){
					startPos = currentPointerPos;
					dragStartTile = currentTile;
				}
				if(didRelease){
					dragStopTile = currentTile;
				}
			}

            if (currentTerraform == TerraFormType.NONE && Input.GetMouseButtonDown(0) && !eventSystem.IsPointerOverGameObject() && Physics.Raycast(ray, out hit, 2048f, buildingMask)) {
                var buildingScript = hit.collider.GetComponent<SimpleBuildingBehaviour>();
                if(buildingScript != null) {
                    buildingScript.HandleBuildingClicked();
                    OnBuildingClicked.Invoke(buildingScript);
                }
                
            }
        }

		void HandleTerraforming(){
			tileOverlay.isVisible = currentTerraform == TerraFormType.BUILD;

			//Make sure that start and stop are actually in correct corners
			MapTile startTile = dragStartTile;
			MapTile stopTile = dragStartTile;
			MapTile.SetStartStop(ref startTile, ref stopTile);

			switch (currentTerraform) {
			case TerraFormType.BULLDOZE:
				transform.position = world.GetWorldPositionFromTile(currentTile);

				gridOverlay.isVisible = true;
				gridOverlay.start = transform.position - HalfTileSize();
				gridOverlay.stop = transform.position + HalfTileSize();
				
				if(Input.GetMouseButtonDown(0)){
					if(doDebug)
						Debug.Log ("Bulldoze " + transform.position + " at "+ currentTile);

					WorldBehaviour.TerraformCostCalculation cost = world.GetBullDozeTileCost(currentTile);
                       

					if(CanAfford(cost, true)){
						world.BullDozeTile(currentTile);
                        buildingManager.Bulldoze(currentTile.x, currentTile.y);

                       PayCost(cost);
						GetComponent<AudioSource>().PlayOneShot(explosionClip);
					}
				}
				break;

			case TerraFormType.BULLDOZE_AREA:

				transform.position = GetHeightMapCoords(currentPointerPos);
				
				
				if(isDragging){
					gridOverlay.isVisible = true;
					gridOverlay.start = startPos;
					gridOverlay.stop = currentPointerPos;
				}else{
					gridOverlay.isVisible = false;
				}

				if(didRelease){
					if(doDebug)
						Debug.Log ("BULLDOZE AREA between " + startPos + " and " + currentPointerPos);

					WorldBehaviour.TerraformCostCalculation cost = world.GetBulldozeAreaCost(dragStartTile, dragStopTile);

					if(CanAfford(cost, true)){
						PayCost(cost);
						world.BulldozeArea(dragStartTile, dragStopTile);
						buildingManager.BulldozeArea(dragStartTile, dragStopTile);
						GetComponent<AudioSource>().PlayOneShot(explosionClip);
					}

					gridOverlay.isVisible = false;
				}
				break;

			case TerraFormType.RAISE_TERRAIN:
				transform.position = GetHeightMapCoords(currentPointerPos) + world.terrain.transform.position;
				
				gridOverlay.isVisible = true;
				gridOverlay.start = currentPointerPos - HalfTileSize();
				gridOverlay.stop = currentPointerPos + HalfTileSize();
				
				if(didPress){
					if(doDebug)
						Debug.Log ("RAISE tile" + currentTile);
					
					WorldBehaviour.TerraformCostCalculation cost = world.GetRaiseTerrainCost(currentTile);

					if(buildingManager.DoTileHaveBuilding(cost.bulldozedTiles)){
						FloatingTextManager.Instance.AddText(transform.position, "Can't raise terrain, building in the way", 2 * Vector3.up, 3f, Color.red);
						break;
					}else{
						if(CanAfford(cost, true)){
							PayCost(cost);
							world.RaiseTerrain(currentTile);
							GetComponent<AudioSource>().PlayOneShot(explosionClip);
						}
					}
				}
				break;
			
			case TerraFormType.LOWER_TERRAIN:
				transform.position = GetHeightMapCoords(currentPointerPos) + world.terrain.transform.position;
				
				gridOverlay.isVisible = true;
				gridOverlay.start = currentPointerPos - HalfTileSize();
				gridOverlay.stop = currentPointerPos + HalfTileSize();
				
				if(didPress){
					if(doDebug)
						Debug.Log ("RAISE tile" + currentTile);
					
					WorldBehaviour.TerraformCostCalculation cost = world.GetLowerTerrainCost(currentTile);
					
					if(buildingManager.DoTileHaveBuilding(cost.bulldozedTiles)){
						FloatingTextManager.Instance.AddText(transform.position, "Can't lower terrain, building in the way", 2 * Vector3.up, 3f, Color.red);
						break;
					}else{
						if(CanAfford(cost, true)){
							PayCost(cost);
							world.LowerTerrain(currentTile);
							GetComponent<AudioSource>().PlayOneShot(explosionClip);
						}
					}
				}
				break;

			case TerraFormType.LEVEL_TERRAIN:
				transform.position = GetHeightMapCoords(currentPointerPos);

				if(isDragging){
					gridOverlay.isVisible = true;
					gridOverlay.start = startPos;
					gridOverlay.stop = currentPointerPos;
				}else{
					gridOverlay.isVisible = false;
				}
				
				if(didRelease){
					if(doDebug)	
						Debug.Log ("LEVEL between " + dragStart + " and "+ dragEnd);
						
					WorldBehaviour.TerraformCostCalculation cost = world.GetLevelTerrainCost(dragStartTile, dragStopTile);

					//First check so that no buildings would be affected (just so that user does not unintentionally destroy buildings)
					if(buildingManager.DoTileHaveBuilding(cost.bulldozedTiles)){
						FloatingTextManager.Instance.AddText(transform.position, "Can't level terrain, building in the way", 2 * Vector3.up, 3f, Color.red);
					}else{

						if(CanAfford(cost, true)){
							PayCost(cost);
							world.LevelTerrain(dragStartTile, dragStopTile);
							GetComponent<AudioSource>().PlayOneShot(explosionClip);
						}
					}

					gridOverlay.isVisible = false;
				}
				break;

			case TerraFormType.BUILD:
				SimpleBuildingType buildingType = AvailableBuildingTypes[currentBuildingIndex];
				gridOverlay.isVisible = true;
				
				int width = buildingType.buildArea.width;
				int length = buildingType.buildArea.length;
				
				if(direction%2 == 1){
					width = buildingType.buildArea.length;
					length = buildingType.buildArea.width;
				}
				
				//If we have an even number as width or length then we want the center to be moved by one step
				Vector3 pos = currentPointerPos + new Vector3(width % 2 - 1, 0, length % 2 - 1) * world.worldData.tileWidth / 2f;
				//Make sure we are never below water surface
				pos.y = Mathf.Max(pos.y, world.GetWaterLevelHeight());

				//Calc tile coords
				int startX = (int)((pos.x - width * 0.5f)/world.worldData.tileWidth) ;//((int)pos.x - width);
				int startY =(int)((pos.z - length * 0.5f)/world.worldData.tileWidth) ; //(int)pos.z - length;
				int stopX = startX + width;
				int stopY = startY + length;
				
				//Mult by tile width
				gridOverlay.start = new Vector3(world.worldData.tileWidth * startX, pos.y, world.worldData.tileWidth * startY);
				gridOverlay.stop = new Vector3(world.worldData.tileWidth * stopX, pos.y, world.worldData.tileWidth * stopY);// pos + new Vector3(width,0,length);
				
				tileOverlay.xCoord = startX;
				tileOverlay.yCoord = startY;
				
				transform.position = (gridOverlay.start + gridOverlay.stop)/2 + world.terrain.transform.position;
				transform.rotation = Quaternion.Euler(new Vector3(0,direction * 90,0));
				
				buildingManager.IsBuildingPositionAllowed(startX, startY, direction, buildingType, out tileOverlay.tiles);
				
				if(didPress){
					if(doDebug)
						Debug.Log ("BUILD " + pos + "Coords x:"+ (pos.x-1)/2 + " y:" + (pos.z-1)/2);
					
					if(!buildingManager.IsBuildingPositionAllowed(startX, startY, direction, buildingType)){
						FloatingTextManager.Instance.AddText(transform.position, "Can't build there", 2 * Vector3.up, 3f, Color.red);
						break;
					}

					//If we can afford then build
					if(CanAfford(buildingType, true)){
                        PayConstructionCost( buildingType);
						buildingManager.ConstructBuildingInTile(startX ,startY, currentBuildingIndex, direction);
						
						GetComponent<AudioSource>().PlayOneShot(explosionClip);
					}
				}
				break;

			default:
				break;
			}
		}
		


		#region Game state management
		public virtual void SaveGame(){
			world.Save(worldSaveName);
			buildingManager.Save(worldSaveName + BUILDINGS_KEY);
            if (resourceManager)
                resourceManager.Save(worldSaveName + RESOURCES_KEY);
			SaveDataManagement.SaveData<TransformData>(TransformData.TransformToSerializable(Camera.main.transform), worldSaveName + CAMERA_KEY);
		}
		
		public virtual void LoadGame(){
			buildingManager.Restart();
			world.Load(worldSaveName);
			buildingManager.Load(worldSaveName + BUILDINGS_KEY);
            if (resourceManager)
                resourceManager.Load(worldSaveName + RESOURCES_KEY);
            if (SaveDataManagement.HasSavedData(worldSaveName + CAMERA_KEY))
                TransformData.SetTransformFromData(Camera.main.transform, SaveDataManagement.LoadData<TransformData>(worldSaveName + CAMERA_KEY));
            else
                ResetCamera();
		}
		
		public virtual void ClearSave(){
			SaveDataManagement.DeleteData(worldSaveName + BUILDINGS_KEY);
			SaveDataManagement.DeleteData(worldSaveName + CAMERA_KEY);
		}
		
		public virtual void ResetGame(){
			buildingManager.Restart();
            if(resourceManager)
                resourceManager.Reset();
			world.Generate(true);
            
            ResetCamera();
        }

        public void ResetCamera() {
            if (Camera.main.orthographic) {
                Camera.main.transform.position = world.GetCenterOfWorld() - Camera.main.transform.forward * Camera.main.farClipPlane / 100f;
            } else {
                Camera.main.transform.position = world.GetCenterOfWorld() + world.tileMapSize / 4 * Vector3.up;
            }
        }
        #endregion Game State Management

        #region Cost and resource calculations
        private bool CanAfford(SimpleBuildingType building, bool doShow = false) {
            if (resourceManager != null)
                if(!resourceManager.CanAfford(building.constructionCost)) {
                    if (doShow) {
                        FloatingTextManager.Instance.AddText(transform.position, "Can't afford\n" + resourceManager.GetCostString(building.constructionCost), 2 * Vector3.up, 3f, Color.yellow);
                    }
                    return false;

                } else {
                    return true;
                }
            else
                return true;
        }
        protected virtual bool CanAfford(WorldBehaviour.TerraformCostCalculation cost, bool doShow = false) {
            if (resourceManager != null) {
                bool canAfford = resourceManager.CanAfford(cost);
                if (doShow && !canAfford) {
                    FloatingTextManager.Instance.AddText(transform.position, "Can't afford\n" + resourceManager.GetCostString(cost), 2 * Vector3.up, 3f, Color.yellow);
                }
                return canAfford;
            } else
                return true;
        }


        protected virtual void PayConstructionCost(SimpleBuildingType building, bool doShow = true) {
            PayCost(building.constructionCost, doShow);
        }
        
        protected virtual void PayCost(WorldBehaviour.TerraformCostCalculation cost, bool doShow = false) {
            if (resourceManager) {
                resourceManager.Pay(cost);
                if (doShow) {
                    FloatingTextManager.Instance.AddText(transform.position, "-" + resourceManager.GetCostString(cost), 2 * Vector3.up, 3f, Color.yellow);
                }
            }
        }
        
        protected virtual void PayCost(IEnumerable<ResourceInstance> cost, bool doShow = false) {
            if (resourceManager) {
                resourceManager.Pay(cost);
                if (doShow) {
                    FloatingTextManager.Instance.AddText(transform.position, "-" + resourceManager.GetCostString(cost), 2 * Vector3.up, 3f, Color.yellow);
                }
            }
        }

       

        
        #endregion

        #region Building and Terraforming
        public void SetBuildingType(int newType){
			if(AvailableBuildingTypes.Count <= newType)
				return;
			currentBuildingIndex = newType;
			SimpleBuildingType buildingType = AvailableBuildingTypes[currentBuildingIndex];
			SetBuildingType(buildingType);
		}
		
		public void SetTerraformType(int newType){
			direction = 0;
			gridOverlay.isVisible = false;
             
			currentTerraform = (TerraFormType)newType;
            playerDisplay.SetTerraformType(currentTerraform);

            switch (currentTerraform) {
			case TerraFormType.BULLDOZE:
				gridOverlay.overlayType = GridOverlay.OverlayType.SQUARE;
				break;
			case TerraFormType.BULLDOZE_AREA:
				gridOverlay.overlayType = GridOverlay.OverlayType.SQUARE;
				break;
			case TerraFormType.RAISE_TERRAIN:
				gridOverlay.overlayType = GridOverlay.OverlayType.CROSS;
				break;
			case TerraFormType.LOWER_TERRAIN:
				gridOverlay.overlayType = GridOverlay.OverlayType.CROSS;
				break;
			case TerraFormType.LEVEL_TERRAIN:
				gridOverlay.overlayType = GridOverlay.OverlayType.SQUARE;
				break;
			case TerraFormType.BUILD:
				gridOverlay.overlayType = GridOverlay.OverlayType.SQUARE;
				
				break;
			default:
				break;
			}
		}
		
		/// <summary>
		/// Gets a world space position of a point on terrain's tile coordinates
		/// </summary>
		/// <returns>The height map coords.</returns>
		/// <param name="pointOnTerrain">Point on terrain.</param>
		protected Vector3 GetHeightMapCoords(Vector3 pointOnTerrain){
			Vector3 pos = pointOnTerrain - world.terrain.transform.position;
			pos.x = world.worldData.tileWidth * Mathf.Round(pos.x / world.worldData.tileWidth);
			pos.z = world.worldData.tileWidth * Mathf.Round(pos.z / world.worldData.tileWidth);
			pos.y = world.GetHeight(pos.x, pos.z);
			
			return pos;
		}
		
		/// <summary>
		/// Returns true if the current terraform operation is performed on a tile. This has an effect of where player should be positioned
		/// if it is a tile then the arrow should mark the center of it otherwise a grid point (tile corner)
		/// </summary>
		protected bool IsTileOperation(TerraFormType terraform){
			return 	terraform == TerraFormType.BUILD ||
				terraform == TerraFormType.BULLDOZE;
		}
		
		protected Vector3 HalfTileSize(){
			return new Vector3(world.worldData.tileWidth * 0.5f, 0, world.worldData.tileWidth * 0.5f);
		}

        
		#endregion Building and Terraforming

		#region IPlayerController implementation

		public void SetBuildingType(SimpleBuildingType buildingType){
			if(AvailableBuildingTypes.Contains(buildingType))
				currentBuildingIndex = AvailableBuildingTypes.IndexOf(buildingType);
			else 
				return;
			
			SetTerraformType((int)TerraFormType.BUILD);
			currentTerraform = TerraFormType.BUILD;
			playerDisplay.tmpBuildObject = (GameObject)GameObject.Instantiate(buildingType.prefab, transform.position, transform.rotation);
            playerDisplay.tmpBuildObject.GetComponent<SimpleBuildingBehaviour>().enabled = false;
            playerDisplay.tmpBuildObject.transform.SetParent(transform);
            playerDisplay.tmpBuildObject.transform.localScale = buildingType.scale;
			
		}

		public System.Collections.Generic.List<SimpleBuildingType> AvailableBuildingTypes {
			get {
				return buildingManager.buildingTypes;
			}
		}

		#endregion

		
	}
}