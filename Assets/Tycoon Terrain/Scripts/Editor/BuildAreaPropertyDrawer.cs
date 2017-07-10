using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TycoonTerrain {
    [CustomPropertyDrawer(typeof(TycoonTerrain.SimpleBuildingType.TileTypeArea))]
    public class BuildAreaPropertyDrawer : PropertyDrawer {
        bool doFoldout;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            int initialIndent = EditorGUI.indentLevel;
            SerializedProperty columnsData = property.FindPropertyRelative("width");
            SerializedProperty rowsData = property.FindPropertyRelative("length");
            SerializedProperty tiles = property.FindPropertyRelative("tiles");
            position.height = 18;
            doFoldout = EditorGUI.Foldout(position, doFoldout, label);
            //EditorGUI.PrefixLabel (position, label);
            if (doFoldout) {
                EditorGUI.indentLevel++;
                position.y += 18;
                columnsData.intValue = EditorGUI.IntField(position, "Width: ", columnsData.intValue);
                position.y += 18;
                rowsData.intValue = EditorGUI.IntField(position, "Length: ", rowsData.intValue);
                position.y += 18;

                if (tiles.arraySize != columnsData.intValue * rowsData.intValue) {
                    property.FindPropertyRelative("tiles.Array.size").intValue = columnsData.intValue * rowsData.intValue;
                }

                Rect newPos = position;
                newPos.width = 60;
                newPos.height = 32;
                for (int y = 0; y < rowsData.intValue; y++) {
                    for (int x = 0; x < columnsData.intValue; x++) {
                        int index = x + y * columnsData.intValue;
                        //tiles.InsertArrayElementAtIndex(index) = EditorGUI.EnumPopup(newPos, tiles.GetArrayElementAtIndex(index));
                        EditorGUI.PropertyField(newPos, tiles.GetArrayElementAtIndex(index), GUIContent.none);
                        newPos.x += newPos.width;
                    }
                    newPos.x = position.x;
                    newPos.y += newPos.height;
                }
            }
            EditorGUI.indentLevel = initialIndent;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (doFoldout)
                return property.FindPropertyRelative("length").intValue * 32 + 18 * 3;
            else
                return 18;

        }
    }
}