using System.Collections.Generic;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Services;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FrontLine.Views
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public static float TileSize = 1f;
        [SerializeField] private float _tileSpacing = 0.00f;

        [Header("Editor Preview")]
        [SerializeField] private int _editorGridWidth = 16;
        [SerializeField] private int _editorGridHeight = 14;

        [Header("Highlight Pool")]
        [SerializeField] private int _highlightPoolSize = 150;
        [SerializeField] private float _pathLineWidth = 0.08f;
        [SerializeField] private Material _lineMaterial;

        public float TileStep => TileSize + _tileSpacing;

        private static readonly Color GizmoBlocked = new Color(1f, 0.2f, 0.2f, 0.5f);
        private static readonly Color GizmoNormal  = new Color(1f, 1f, 1f, 0.08f);

        private GameState _gameState;
        private Material _resolvedLineMaterial;
        private Queue<LineRenderer> _highlightPool;
        private Dictionary<(int, int), LineRenderer> _activeHighlights;
        private LineRenderer _pathLine;

        public void Initialize()
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            BuildHighlightPool();
            CenterCameraOnGrid();
        }

        private void BuildHighlightPool()
        {
            _resolvedLineMaterial = _lineMaterial != null
                ? _lineMaterial
                : new Material(Shader.Find("Sprites/Default"));

            // Dedicated path line (not pooled — only one path shown at a time)
            var pathGo = new GameObject("MovePath");
            pathGo.transform.SetParent(transform);
            _pathLine = pathGo.AddComponent<LineRenderer>();
            _pathLine.startWidth = _pathLine.endWidth = _pathLineWidth;
            _pathLine.material = _resolvedLineMaterial;
            _pathLine.useWorldSpace = true;
            _pathLine.loop = false;
            _pathLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _pathLine.receiveShadows = false;
            pathGo.SetActive(false);

            _highlightPool = new Queue<LineRenderer>(_highlightPoolSize);
            _activeHighlights = new Dictionary<(int, int), LineRenderer>();

            for (int i = 0; i < _highlightPoolSize; i++)
            {
                var go = new GameObject($"Highlight_{i}");
                go.transform.SetParent(transform);
                var lr = go.AddComponent<LineRenderer>();
                ConfigureLineRenderer(lr);
                go.SetActive(false);
                _highlightPool.Enqueue(lr);
            }
        }

        private void ConfigureLineRenderer(LineRenderer lr)
        {
            lr.positionCount = 5;
            lr.useWorldSpace = true;
            lr.startWidth = lr.endWidth = 0.04f;
            lr.loop = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = _resolvedLineMaterial;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            float step = TileStep;
            var tile = _gameState?.GetTile(x, y);
            float height = tile != null ? tile.Elevation * TileSize : 0f;
            return new Vector3(x * step, height, y * step);
        }

        public void HighlightTile(int x, int y, Color color)
        {
            if (_activeHighlights == null) return;

            if (_activeHighlights.TryGetValue((x, y), out var existing))
            {
                existing.startColor = existing.endColor = color;
                return;
            }

            if (_highlightPool.Count == 0) return;

            var lr = _highlightPool.Dequeue();
            lr.startColor = lr.endColor = color;
            SetOutlinePositions(lr, x, y);
            lr.gameObject.SetActive(true);
            _activeHighlights[(x, y)] = lr;
        }

        private void SetOutlinePositions(LineRenderer lr, int x, int y)
        {
            Vector3 c = GetWorldPosition(x, y);
            float half = TileSize * 0.5f;
            float raise = c.y + 0.02f;

            lr.SetPositions(new[]
            {
                new Vector3(c.x - half, raise, c.z - half),
                new Vector3(c.x + half, raise, c.z - half),
                new Vector3(c.x + half, raise, c.z + half),
                new Vector3(c.x - half, raise, c.z + half),
                new Vector3(c.x - half, raise, c.z - half),
            });
        }

        public void ResetTileColor(int x, int y)
        {
            if (_activeHighlights == null) return;
            if (!_activeHighlights.TryGetValue((x, y), out var lr)) return;

            lr.gameObject.SetActive(false);
            _activeHighlights.Remove((x, y));
            _highlightPool.Enqueue(lr);
        }

        public void ResetAllTileColors()
        {
            if (_activeHighlights == null) return;

            foreach (var lr in _activeHighlights.Values)
            {
                lr.gameObject.SetActive(false);
                _highlightPool.Enqueue(lr);
            }
            _activeHighlights.Clear();
        }

        // path is the A* result (excludes the start tile, includes destination).
        // fromX/fromY is the unit's current tile — prepended as the line's first point.
        public void ShowMovePath(List<(int x, int y)> path, int fromX, int fromY, Color color)
        {
            if (_pathLine == null || path == null || path.Count == 0)
            {
                HideMovePath();
                return;
            }

            int count = path.Count + 1;
            var positions = new Vector3[count];
            positions[0] = GetWorldPosition(fromX, fromY) + Vector3.up * 0.15f;
            for (int i = 0; i < path.Count; i++)
            {
                var (px, py) = path[i];
                positions[i + 1] = GetWorldPosition(px, py) + Vector3.up * 0.15f;
            }

            _pathLine.positionCount = count;
            _pathLine.SetPositions(positions);
            _pathLine.startColor = _pathLine.endColor = color;
            _pathLine.gameObject.SetActive(true);
        }

        public void HideMovePath()
        {
            if (_pathLine != null)
                _pathLine.gameObject.SetActive(false);
        }

        private void CenterCameraOnGrid()
        {
            float step = TileStep;
            int width = _gameState?.GridWidth ?? _editorGridWidth;
            int height = _gameState?.GridHeight ?? _editorGridHeight;
            float centerX = (width - 1) * step / 2f;
            float centerZ = (height - 1) * step / 2f;

            Camera.main.transform.position = new Vector3(
                centerX,
                Mathf.Max(width, height) * 1.2f,
                centerZ - 2f);

            Camera.main.transform.LookAt(new Vector3(centerX, 0f, centerZ));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            int width = _gameState?.GridWidth ?? _editorGridWidth;
            int height = _gameState?.GridHeight ?? _editorGridHeight;
            float step = TileStep;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tile = _gameState?.GetTile(x, y);
                    Gizmos.color = tile?.Type == TileType.Blocked ? GizmoBlocked : GizmoNormal;
                    Gizmos.DrawWireCube(
                        new Vector3(x * step, 0.01f, y * step),
                        new Vector3(TileSize * 0.98f, 0.01f, TileSize * 0.98f));
                }
            }
        }
#endif
    }
}
