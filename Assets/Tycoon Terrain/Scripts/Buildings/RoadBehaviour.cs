using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
    [RequireComponent(typeof(SimpleBuildingBehaviour))]
	public class RoadBehaviour : MonoBehaviour {

        SimpleBuildingBehaviour building;
		// Use this for initialization
		void Start () {
            building = GetComponent<SimpleBuildingBehaviour>();
            
            if(building != null && building.isActiveAndEnabled  && building.world.GetTileType(transform.position) == MapTile.MapTileType.LEANING) {
                MapTile tile = building.world.GetTileFromWorldPosition(transform.position);


                //Next we rotate the road to "lean with terrain". If it DOES lean then we stretch it in case it goes up a hill so that it reaches all the way
                if (building.world.isTileLeaningDownY(tile)) {
                   
                    Vector3 scale = transform.localScale;
                    if (transform.rotation.eulerAngles.y % 180 < 1)
                        scale.z *= 1.5f;
                    transform.localScale = scale;

                    transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                }
                if (building.world.isTileLeaningUpY(tile)) {
                   
                    Vector3 scale = transform.localScale;
                    if (transform.rotation.eulerAngles.y % 180 < 1)
                        scale.z *= 1.5f;
                    transform.localScale = scale;

                    transform.Rotate(new Vector3(45, 0, 0), Space.World);
                }
                if (building.world.isTileLeaningDownX(tile)) {
                    
                    Vector3 scale = transform.localScale;
                    if (transform.rotation.eulerAngles.y % 180 > 1)
                        scale.z *= 1.5f;
                    transform.localScale = scale;

                    transform.Rotate(new Vector3(0, 0, 45), Space.World);
                }
                if (building.world.isTileLeaningUpX(tile)) {
                    
                    Vector3 scale = transform.localScale;
                    if (transform.rotation.eulerAngles.y % 180 > 1)
                        scale.z *= 1.5f;
                    transform.localScale = scale;

                    transform.Rotate(new Vector3(0, 0, -45), Space.World);
                    
                }
                
            }
           

            
		}
	}
}