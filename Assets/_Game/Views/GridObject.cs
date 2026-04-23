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
#endif

        private void OnDrawGizmos()
        {
            Vector3 pos = transform.position;
            int gx = Mathf.RoundToInt(pos.x / _cellSize);
            int gy = Mathf.RoundToInt(pos.z / _cellSize);

            Gizmos.color = _objectType switch
            {
                GridObjectType.Wall => Color.red,
                GridObjectType.HalfCover => Color.yellow,
                GridObjectType.FullCover => Color.yellow,
                GridObjectType.Decoration => Color.green,
                _ => Color.white
            };

            Vector3 center = new Vector3(gx * _cellSize, pos.y, gy * _cellSize);
            Gizmos.DrawWireCube(center, new Vector3(_cellSize, transform.localScale.magnitude, _cellSize));
#if UNITY_EDITOR
            Handles.Label(center + Vector3.up * 0.6f, $"({gx},{gy})");
#endif
        }
    }
}
