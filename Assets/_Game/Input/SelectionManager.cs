using UnityEngine;
using UnityEngine.EventSystems;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Services;
using FrontLine.Commands;
using FrontLine.Views;
using FrontLine.UI;

namespace FrontLine.Input
{
    public enum SelectionState
    {
        Idle,
        UnitSelected
    }

    public class SelectionManager : MonoBehaviour
    {
        [Header("Highlight Colors")]
        [SerializeField] private Color _moveRangeColor = new Color(0.4f, 0.8f, 1f, 1f);
        [SerializeField] private Color _attackRangeColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _selectedColor = new Color(1f, 1f, 0f, 1f);

        private HUDController _hudController;
        private GameState _gameState;
        private TurnController _turnController;
        private CommandProcessor _commandProcessor;
        private GridManager _gridManager;
        private InputManager _inputManager;
        private Camera _camera;
        private Vector2? _pendingTap;

        private SelectionState _state = SelectionState.Idle;
        private string _selectedUnitId;

        public void Initialize(InputManager inputManager)
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();
            _commandProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            _gridManager = ServiceLocator.Instance.Get<GridManager>();
            _hudController = ServiceLocator.Instance.Get<HUDController>();
            _camera = Camera.main;

            _inputManager = inputManager;
            _inputManager.OnTapWorld += HandleTap;
            _inputManager.OnCancel += ClearSelection;
        }

        private void OnDestroy()
        {
            if (_inputManager == null) return;
            _inputManager.OnTapWorld -= HandleTap;
            _inputManager.OnCancel -= ClearSelection;
        }

        private void HandleTap(Vector2 screenPosition)
        {
            _pendingTap = screenPosition;
        }

        private void Update()
        {
            if (_pendingTap == null) return;

            var screenPosition = _pendingTap.Value;
            _pendingTap = null;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = _camera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                ClearSelection();
                return;
            }

            if (!TryGetTileFromHit(hit, out int tileX, out int tileY))
            {
                ClearSelection();
                return;
            }

            HandleTileTouch(tileX, tileY);
        }

        private void HandleTileTouch(int tileX, int tileY)
        {
            switch (_state)
            {
                case SelectionState.Idle:
                    TrySelectUnit(tileX, tileY);
                    break;

                case SelectionState.UnitSelected:
                    TryActOnTile(tileX, tileY);
                    break;
            }
        }

        private void TrySelectUnit(int tileX, int tileY)
        {
            var tile = _gameState.GetTile(tileX, tileY);

            if (tile == null || !tile.IsOccupied) return;

            var unit = _gameState.Units[tile.OccupyingUnitId];

            if (!_turnController.IsCurrentPlayer(unit.OwnerId)) return;
            if (!_turnController.HasActionPoints(unit.UnitId)) return;

            _selectedUnitId = unit.UnitId;
            _state = SelectionState.UnitSelected;
            HighlightSelection(unit);
            _hudController.OnUnitSelected(_selectedUnitId);
        }

        private void TryActOnTile(int tileX, int tileY)
        {
            if (_selectedUnitId == null) return;

            var tile = _gameState.GetTile(tileX, tileY);
            if (tile == null) return;

            if (tile.IsOccupied)
            {
                var occupant = _gameState.Units[tile.OccupyingUnitId];

                if (occupant.OwnerId != _turnController.CurrentPlayerId)
                {
                    var result = _commandProcessor.Process(
                        new ShootCommand(
                            _turnController.CurrentPlayerId,
                            _selectedUnitId,
                            tile.OccupyingUnitId));

                    Debug.Log($"[SelectionManager] Shoot: {result.Message}");
                    ClearSelection();
                    return;
                }

                // Tapped friendly — switch selection
                ClearSelection();
                TrySelectUnit(tileX, tileY);
                return;
            }

            var moveResult = _commandProcessor.Process(
                new MoveCommand(
                    _turnController.CurrentPlayerId,
                    _selectedUnitId,
                    tileX,
                    tileY));

            Debug.Log($"[SelectionManager] Move: {moveResult.Message}");
            ClearSelection();
        }

        private void HighlightSelection(UnitData unit)
        {
            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);

            for (int x = 0; x < _gameState.GridWidth; x++)
            {
                for (int y = 0; y < _gameState.GridHeight; y++)
                {
                    var tile = _gameState.GetTile(x, y);
                    if (tile == null || !tile.IsWalkable()) continue;

                    int dist = Mathf.Max(
                        Mathf.Abs(x - unit.TileX),
                        Mathf.Abs(y - unit.TileY));

                    if (dist <= unit.MoveRange && dist > 0)
                        _gridManager.HighlightTile(x, y, _moveRangeColor);
                }
            }

            foreach (var target in _gameState.Units.Values)
            {
                if (target.OwnerId == unit.OwnerId) continue;

                int dist = Mathf.Max(
                    Mathf.Abs(target.TileX - unit.TileX),
                    Mathf.Abs(target.TileY - unit.TileY));

                if (dist <= unit.AttackRange)
                    _gridManager.HighlightTile(target.TileX, target.TileY, _attackRangeColor);
            }
        }

        public void ClearSelection()
        {
            _selectedUnitId = null;
            _state = SelectionState.Idle;
            _gridManager.ResetAllTileColors();
            _hudController.OnSelectionCleared();
        }

        private bool TryGetTileFromHit(RaycastHit hit, out int tileX, out int tileY)
        {
            float step = _gridManager.TileStep;
            tileX = Mathf.RoundToInt(hit.point.x / step);
            tileY = Mathf.RoundToInt(hit.point.z / step);

            return _gameState.IsValidPosition(tileX, tileY);
        }
    }
}
