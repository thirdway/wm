using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TycoonTerrain {
    [CustomEditor(typeof(Resource))]
    [CanEditMultipleObjects]
    public class ResourceDataDrawer : Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();
            base.DrawDefaultInspector();
            if (GUILayout.Button("Add to resource manager")) {
                var resourceManagers = FindObjectsOfType<ResourceManager>();

                if (resourceManagers.Length == 0) {
                    Debug.Log("Could not find a building manager in scene");
                } else if (resourceManagers.Length == 1) {
                    var resourceManager = resourceManagers[0];
                    var resourceType = target as Resource;
                    if (resourceManager.resources.Contains(resourceType))
                        Debug.Log("Resource manager already contains " + resourceType);
                    else
                        resourceManager.resources.Add(target as Resource);
                    Selection.activeGameObject = resourceManager.gameObject;
                } else {
                    Debug.Log("Found several resource managers in scene, can't determine which one to add to. You'll have to add it manually");
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}