using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Views;

namespace FrontLine.Editor
{
    [CustomEditor(typeof(MapData))]
    public class MapDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space(8);

            var mapData = (MapData)target;

            EditorGUILayout.HelpBox(
                "Bake to scan all GridObject components in the open scene (including inactive) and write their positions into this asset.",
                MessageType.Info);

            if (GUILayout.Button("Bake Map From Scene", GUILayout.Height(32)))
                BakeFromScene(mapData);

            EditorGUILayout.LabelField("Baked entries:", mapData.Entries.Count.ToString(), EditorStyles.boldLabel);
        }

        private static void BakeFromScene(MapData mapData)
        {
            // true = include inactive GameObjects so disabled props aren't silently skipped
            var gridObjects = Object.FindObjectsOfType<GridObject>(true);

            if (gridObjects.Length == 0)
            {
                Debug.LogWarning("[MapDataEditor] No GridObject components found in scene.");
                return;
            }

            var entries = new List<GridObjectEntry>(gridObjects.Length);
            foreach (var go in gridObjects)
            {
                Vector3 pos = go.transform.position;
                int gx = Mathf.RoundToInt(pos.x / go.CellSize);
                int gy = Mathf.RoundToInt(pos.z / go.CellSize);

                entries.Add(new GridObjectEntry { X = gx, Y = gy, ObjectType = go.ObjectType });
            }

            mapData.SetEntries(entries);
            EditorUtility.SetDirty(mapData);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MapDataEditor] Baked {entries.Count} entries into {mapData.name}.");
        }
    }
}
