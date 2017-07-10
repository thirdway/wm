using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VikingCrewDevelopment{
	public class ReplaceMissingMaterials : EditorWindow {
        public GameObject obj;
        public Material material;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Replace missing materials")]
        static void Init() {
            // Get existing open window or if none, make a new one:
            ReplaceMissingMaterials window = (ReplaceMissingMaterials)EditorWindow.GetWindow(typeof(ReplaceMissingMaterials));
            window.Show();
        }
        

        void OnGUI() {
            obj = EditorGUILayout.ObjectField("Parent: ", obj, typeof(GameObject), true) as GameObject;
            material = EditorGUILayout.ObjectField("Material to replace with: ", material, typeof(Material), true) as Material;
            if (obj != null && material != null && GUILayout.Button("Replace missing materials")) {
                var renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers) {
                    if (renderer.sharedMaterial == null)
                        renderer.sharedMaterial = material;
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
                        
                    }
                }
            }
        }
	}
}