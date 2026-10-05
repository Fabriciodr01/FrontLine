using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FrontLine.Map.Grid
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

        public (int x, int y) GetAnchorCell()
        {
            var r = GetComponentInChildren<Renderer>();
            Vector3 pos = transform.position;
            float sizeX = r != null ? r.bounds.size.x : _cellSize;
            float sizeZ = r != null ? r.bounds.size.z : _cellSize;
            float snapX = ComputeSnap(pos.x, sizeX);
            float snapZ = ComputeSnap(pos.z, sizeZ);
            return (Mathf.FloorToInt(snapX / _cellSize), Mathf.FloorToInt(snapZ / _cellSize));
        }

#if UNITY_EDITOR
        private void OnValidate() => SnapToGrid();

        private void SnapToGrid()
        {
            var r = GetComponentInChildren<Renderer>();
            Vector3 pos = transform.position;
            float sizeX = r != null ? r.bounds.size.x : _cellSize;
            float sizeZ = r != null ? r.bounds.size.z : _cellSize;

            float snapX = ComputeSnap(pos.x, sizeX);
            float snapZ = ComputeSnap(pos.z, sizeZ);

            var anchor = GetAnchorCell();
            GridX = anchor.x;
            GridY = anchor.y;
            transform.position = new Vector3(snapX, pos.y, snapZ);
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

        private float ComputeSnap(float worldPos, float boundsSize)
        {
            int tileCount = Mathf.RoundToInt(boundsSize / _cellSize);
            if (tileCount % 2 == 0)
                return (Mathf.Round(worldPos / _cellSize - 0.5f) + 0.5f) * _cellSize;
            return Mathf.RoundToInt(worldPos / _cellSize) * _cellSize;
        }

        private void OnDrawGizmos()
        {
            var r = GetComponentInChildren<Renderer>();
            Vector3 pos = transform.position;
            float sizeX = r != null ? r.bounds.size.x : _cellSize;
            float sizeZ = r != null ? r.bounds.size.z : _cellSize;
            float snapX = ComputeSnap(pos.x, sizeX);
            float snapZ = ComputeSnap(pos.z, sizeZ);
            int gx = Mathf.FloorToInt(snapX / _cellSize);
            int gy = Mathf.FloorToInt(snapZ / _cellSize);
            Vector3 snappedCenter = new Vector3(snapX, pos.y, snapZ);

            Gizmos.color = _objectType switch
            {
                GridObjectType.Wall => Color.red,
                GridObjectType.HalfCover => Color.yellow,
                GridObjectType.FullCover => Color.yellow,
                GridObjectType.Decoration => Color.green,
                GridObjectType.FragGrenadeBox => Color.magenta,
                GridObjectType.SmokeGrenadeBox => Color.cyan,
                _ => Color.white
            };

            if (r != null)
                Gizmos.DrawWireCube(snappedCenter, r.bounds.size);
            else
                Gizmos.DrawWireCube(snappedCenter, new Vector3(_cellSize, 1f, _cellSize));

#if UNITY_EDITOR
            Handles.Label(snappedCenter + Vector3.up * 0.6f, $"({gx},{gy})");
#endif
        }
    }
}
