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
        [SerializeField] private float _tileSize = 1f;
        [SerializeField] private float _tileSpacing = 0.05f;

        public float TileStep => _tileSize + _tileSpacing;
        private GameState _gameState;
        private GameObject[,] _tileObjects;

        public void Initialize()
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            SpawnGrid();
        }

        private void SpawnGrid()
        {
            _tileObjects = new GameObject[_gameState.GridWidth, _gameState.GridHeight];

            for (int x = 0; x < _gameState.GridWidth; x++)
            {
                for (int y = 0; y < _gameState.GridHeight; y++)
                {
                    SpawnTile(x, y);
                }
            }

            CenterCameraOnGrid();
        }

        private void SpawnTile(int x, int y)
        {
            float step = _tileSize + _tileSpacing;
            Vector3 worldPos = new Vector3(x * step, 0f, y * step);

            var tileObj = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);

            tileObj.name = $"Tile_{x}_{y}";
            tileObj.transform.localScale = new Vector3(_tileSize, _tileHeight, _tileSize);

            _tileObjects[x, y] = tileObj;
        }

        private void CenterCameraOnGrid()
        {
            float step = _tileSize + _tileSpacing;
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
            float step = _tileSize + _tileSpacing;
            var tile = _gameState.GetTile(x, y);
            float height = tile != null ? tile.Elevation * _tileSize : 0f;
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
            HighlightTile(x, y, Color.white);
        }

        public void ResetAllTileColors()
        {
            for (int x = 0; x < _gameState.GridWidth; x++)
                for (int y = 0; y < _gameState.GridHeight; y++)
                    ResetTileColor(x, y);
        }
    }
}
