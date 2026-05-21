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
                "Bake to scan all GridObject components in the open scene (including inactive) and write their positions into this asset. Multi-tile objects are covered cell-by-cell based on renderer bounds.",
                MessageType.Info);

            if (GUILayout.Button("Bake Map From Scene", GUILayout.Height(32)))
                BakeFromScene(mapData);

            EditorGUILayout.LabelField("Baked entries:", mapData.Entries.Count.ToString(), EditorStyles.boldLabel);
        }

        private static void BakeFromScene(MapData mapData)
        {
            var gridObjects = Object.FindObjectsByType<GridObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var grenadeBoxes = Object.FindObjectsByType<GrenadeBoxView>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (gridObjects.Length == 0 && grenadeBoxes.Length == 0)
            {
                Debug.LogWarning("[MapDataEditor] No GridObject or GrenadeBoxView components found in scene.");
                return;
            }

            // Dictionary keyed by (x,y) so overlapping objects produce one entry — last processed wins.
            var entryMap = new Dictionary<(int, int), GridObjectEntry>();

            foreach (var go in gridObjects)
            {
                foreach (var (gx, gy) in go.GetCoveredCells())
                    entryMap[(gx, gy)] = new GridObjectEntry { X = gx, Y = gy, ObjectType = go.ObjectType };
            }

            foreach (var box in grenadeBoxes)
            {
                if (box.TryGetBakedEntry(out var entry))
                    entryMap[(entry.X, entry.Y)] = entry;
            }

            mapData.SetEntries(new List<GridObjectEntry>(entryMap.Values));
            EditorUtility.SetDirty(mapData);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MapDataEditor] Baked {entryMap.Count} tile entries from {gridObjects.Length} GridObjects and {grenadeBoxes.Length} grenade boxes into {mapData.name}.");
        }
    }
}
