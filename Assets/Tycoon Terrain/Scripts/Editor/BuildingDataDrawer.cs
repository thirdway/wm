using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TycoonTerrain {
    [CustomEditor(typeof(SimpleBuildingType))]
    [CanEditMultipleObjects]
    public class BuildingDataDrawer : Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();
            base.DrawDefaultInspector();
            if (GUILayout.Button("Add to building manager")) {
                var buildingManagers = FindObjectsOfType<SimpleBuildingManager>();
                
                if (buildingManagers.Length == 0) {
                    Debug.Log("Could not find a building manager in scene");
                } else if (buildingManagers.Length == 1) {
                    var buildingManager = buildingManagers[0];
                    var buildingType = target as SimpleBuildingType;
                    if (buildingManager.buildingTypes.Contains(buildingType))
                        Debug.Log("Building manager already contains " + buildingType);
                    else
                        buildingManager.buildingTypes.Add(target as SimpleBuildingType);
                    Selection.activeGameObject = buildingManager.gameObject;
                } else {
                    Debug.Log("Found several building managers in scene, can't determine which one to add to. You'll have to add it manually");
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}