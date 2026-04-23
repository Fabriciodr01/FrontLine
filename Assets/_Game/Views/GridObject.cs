using System.Collections.Generic;
using UnityEngine;
using FrontLine.Models;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FrontLine.Views
{
    [DisallowMultipleComponent]
    public class GridObject : MonoBehaviour
    {
        [Header("Grid Authoring")]
        [SerializeField] private GridObjectType _objectType = GridObjectType.Wall;
        private float _cellSize = GridManager.TileSize;

        public GridObjectType ObjectType => _objectType;
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public float CellSize => _cellSize;

#if UNITY_EDITOR
        private void OnValidate() => SnapToGrid();

        private void SnapToGrid()
        {
            Vector3 pos = transform.position;
            GridX = Mathf.RoundToInt(pos.x / _cellSize);
            GridY = Mathf.RoundToInt(pos.z / _cellSize);
            transform.position = new Vector3(GridX * _cellSize, pos.y, GridY * _cellSize);
        }

        public List<(int x, int y)> GetCoveredCells()
        {
            var result = new List<(int, int)>();
            var r = GetComponentInChildren<Renderer>();

            if (r != null)
            {
                // Tile (tx) has its CENTER at world (tx * cellSize) and spans ±half around it.
                // Shifting bounds by +half before flooring aligns Floor with that convention.
                const float eps = 0.001f;
                float half = _cellSize * 0.5f;
                Bounds b = r.bounds;
                int minX = Mathf.FloorToInt((b.min.x + half) / _cellSize);
                int maxX = Mathf.FloorToInt((b.max.x + half - eps) / _cellSize);
                int minZ = Mathf.FloorToInt((b.min.z + half) / _cellSize);
                int maxZ = Mathf.FloorToInt((b.max.z + half - eps) / _cellSize);

                for (int x = minX; x <= maxX; x++)
                    for (int z = minZ; z <= maxZ; z++)
                        result.Add((x, z));
            }
            else
            {
                result.Add((GridX, GridY));
            }

            return result;
        }
#endif

        private void OnDrawGizmos()
        {
            Vector3 pos = transform.position;
            int gx = Mathf.RoundToInt(pos.x / _cellSize);
            int gy = Mathf.RoundToInt(pos.z / _cellSize);
            Vector3 snappedCenter = new Vector3(gx * _cellSize, pos.y, gy * _cellSize);

            Gizmos.color = _objectType switch
            {
                GridObjectType.Wall => Color.red,
                GridObjectType.HalfCover => Color.yellow,
                GridObjectType.FullCover => Color.yellow,
                GridObjectType.Decoration => Color.green,
                _ => Color.white
            };

            var r = GetComponentInChildren<Renderer>();
            if (r != null)
                Gizmos.DrawWireCube(r.bounds.center, r.bounds.size);
            else
                Gizmos.DrawWireCube(snappedCenter, new Vector3(_cellSize, 1f, _cellSize));

#if UNITY_EDITOR
            Handles.Label(snappedCenter + Vector3.up * 0.6f, $"({gx},{gy})");
#endif
        }
    }
}
