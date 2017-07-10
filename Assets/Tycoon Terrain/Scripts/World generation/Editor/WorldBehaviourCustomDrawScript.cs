using UnityEngine;
using System.Collections;
using UnityEditor;

namespace TycoonTerrain {
    [CustomEditor(typeof(WorldBehaviour))]
    public class WorldBehaviourCustomDrawScript : Editor {

        public override void OnInspectorGUI() {
            GUILayout.Label(@"
The world behaviour stores all data related to the world or things that are in it. 
Trees, oceans, buildings etc all will be children of this object.
You can tweak values below to adjust what type of world you want. 
If you are uncertain what a value does hover over it to see its tooltip.
");
            DrawDefaultInspector();
        }
    }
}