using UnityEngine;
using FrontLine.Models;
using FrontLine.Services;

namespace FrontLine.Views
{
    public class GridManager : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private float _tileHeight = 0.1f;
        public static float TileSize = 1f;
        [SerializeField] private float _tileSpacing = 0.00f;

        public float TileStep => TileSize + _tileSpacing;
        private static readonly Color ObstacleColor = new Color(0.25f, 0.2f, 0.18f, 1f);

        private GameState _gameState;
        private GameObject[,] _tileObjects;
        private Color[,] _baseTileColors;

        public void Initialize()
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            SpawnGrid();
        }

        private void SpawnGrid()
        {
            _tileObjects = new GameObject[_gameState.GridWidth, _gameState.GridHeight];
            _baseTileColors = new Color[_gameState.GridWidth, _gameState.GridHeight];

            for (int x = 0; x < _gameState.GridWidth; x++)
            {
                for (int y = 0; y < _gameState.GridHeight; y++)
                {
                    SpawnTile(x, y);
                    var tile = _gameState.GetTile(x, y);
                    _baseTileColors[x, y] = tile != null && tile.Type == TileType.Blocked
                        ? ObstacleColor
                        : Color.white;
                }
            }

            // Apply base colors so obstacles are immediately visible
            for (int x = 0; x < _gameState.GridWidth; x++)
                for (int y = 0; y < _gameState.GridHeight; y++)
                    HighlightTile(x, y, _baseTileColors[x, y]);

            CenterCameraOnGrid();
        }

        private void SpawnTile(int x, int y)
        {
            float step = TileSize + _tileSpacing;
            Vector3 worldPos = new Vector3(x * step, 0f, y * step);

            var tileObj = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);

            tileObj.name = $"Tile_{x}_{y}";
            tileObj.transform.localScale = new Vector3(TileSize, _tileHeight, TileSize);

            _tileObjects[x, y] = tileObj;
        }

        private void CenterCameraOnGrid()
        {
            float step = TileSize + _tileSpacing;
            float centerX = (_gameState.GridWidth - 1) * step / 2f;
            float centerZ = (_gameState.GridHeight - 1) * step / 2f;

            Camera.main.transform.position = new Vector3(
                centerX,
                Mathf.Max(_gameState.GridWidth, _gameState.GridHeight) * 1.2f,
                centerZ - 2f);

            Camera.main.transform.LookAt(new Vector3(centerX, 0f, centerZ));
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            float step = TileSize + _tileSpacing;
            var tile = _gameState.GetTile(x, y);
            float height = tile != null ? tile.Elevation * TileSize : 0f;
            return new Vector3(x * step, height, y * step);
        }

        public GameObject GetTileObject(int x, int y)
        {
            if (x < 0 || x >= _gameState.GridWidth) return null;
            if (y < 0 || y >= _gameState.GridHeight) return null;
            return _tileObjects[x, y];
        }

        public void HighlightTile(int x, int y, Color color)
        {
            var tileObj = GetTileObject(x, y);
            if (tileObj == null) return;

            var renderer = tileObj.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }

        public void ResetTileColor(int x, int y)
        {
            if (_baseTileColors == null) return;
            if (x < 0 || x >= _gameState.GridWidth || y < 0 || y >= _gameState.GridHeight) return;
            HighlightTile(x, y, _baseTileColors[x, y]);
        }

        public void ResetAllTileColors()
        {
            for (int x = 0; x < _gameState.GridWidth; x++)
                for (int y = 0; y < _gameState.GridHeight; y++)
                    ResetTileColor(x, y);
        }
    }
}
