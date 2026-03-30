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
        UnitSelected,
        MovePending,
        ShootPending
    }

    public class SelectionManager : MonoBehaviour
    {
        [Header("Highlight Colors")]
        [SerializeField] private Color _moveRangeColor = new Color(0.4f, 0.8f, 1f, 1f);
        [SerializeField] private Color _attackRangeColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _selectedColor = new Color(1f, 1f, 0f, 1f);
        [SerializeField] private Color _pendingColor = new Color(0.4f, 1f, 0.4f, 1f);

        private GameState _gameState;
        private TurnController _turnController;
        private CommandProcessor _commandProcessor;
        private GridManager _gridManager;
        private CombatResolver _combatResolver;
        private LineOfSightService _losService;
        private InputManager _inputManager;
        private HUDController _hudController;
        private Camera _camera;

        private bool _inputEnabled;

        private SelectionState _state = SelectionState.Idle;
        private string _selectedUnitId;
        private Vector2? _pendingTap;

        private int _pendingMoveX;
        private int _pendingMoveY;
        private string _pendingTargetId;

        public void Initialize(InputManager inputManager, HUDController hudController)
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();
            _commandProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            _gridManager = ServiceLocator.Instance.Get<GridManager>();
            _combatResolver = ServiceLocator.Instance.Get<CombatResolver>();
            _losService = ServiceLocator.Instance.Get<LineOfSightService>();
            _camera = Camera.main;
            _inputManager = inputManager;
            _hudController = hudController;

            _inputEnabled = false;

            _inputManager.OnTapWorld += HandleTap;
            _inputManager.OnCancel += ClearSelection;
            _hudController.OnActionPressed += HandleAction;
            _hudController.OnPopupExecute += HandlePopupExecute;
            _hudController.OnPopupCancel += HandlePopupCancel;
        }

        public void OnTurnStarted(string playerId)
        {
            _inputEnabled = playerId == "Player1";
            if (!_inputEnabled)
                ClearSelection();
        }

        public void OnTurnEnded(string playerId)
        {
            _inputEnabled = false;
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.OnTapWorld -= HandleTap;
                _inputManager.OnCancel -= ClearSelection;
            }

            if (_hudController != null)
            {
                _hudController.OnActionPressed -= HandleAction;
                _hudController.OnPopupExecute -= HandlePopupExecute;
                _hudController.OnPopupCancel -= HandlePopupCancel;
            }
        }

        private void HandleTap(Vector2 screenPosition)
        {
            if (!_inputEnabled) return;
            _pendingTap = screenPosition;
        }

        private void Update()
        {
            if (_pendingTap == null) return;

            var screenPos = _pendingTap.Value;
            _pendingTap = null;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = _camera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                if (_state != SelectionState.Idle)
                    ClearSelection();
                return;
            }

            if (!TryGetTileFromHit(hit, out int tileX, out int tileY))
            {
                if (_state != SelectionState.Idle)
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
                case SelectionState.UnitSelected:
                    TrySelectUnit(tileX, tileY);
                    break;

                case SelectionState.MovePending:
                    TryConfirmMove(tileX, tileY);
                    break;

                case SelectionState.ShootPending:
                    TryConfirmShoot(tileX, tileY);
                    break;
            }
        }

        private void HandleAction(ActionType action)
        {
            if (!_inputEnabled) return;
            switch (action)
            {
                case ActionType.Move:
                    ToggleAction(SelectionState.MovePending, ActionType.Move,
                        () => HighlightMoveRange());
                    break;

                case ActionType.Shoot:
                    ToggleAction(SelectionState.ShootPending, ActionType.Shoot,
                        () => HighlightAttackRange());
                    break;

                case ActionType.Cancel:
                    ClearSelection();
                    break;

                    // TODO-POST-ALPHA: ActionType.Throw, ActionType.UseConsumable
            }
        }

        private void ToggleAction(SelectionState targetState, ActionType actionType, System.Action highlightAction)
        {
            if (_selectedUnitId == null) return;

            if (_state == targetState)
            {
                EnterUnitSelected();
                return;
            }

            _state = targetState;
            ClearActionSelectionVisuals();
            _hudController.SetActionSelected(actionType, true);
            _hudController.HideConfirmation();
            highlightAction();
        }

        private void TrySelectUnit(int tileX, int tileY)
        {
            var tile = _gameState.GetTile(tileX, tileY);
            if (tile == null || !tile.IsOccupied) return;

            var unit = _gameState.Units[tile.OccupyingUnitId];
            if (!_turnController.IsCurrentPlayer(unit.OwnerId)) return;
            if (!_turnController.HasActionPoints(unit.UnitId)) return;

            _selectedUnitId = unit.UnitId;
            EnterUnitSelected();
        }

        private void EnterUnitSelected()
        {
            _state = SelectionState.UnitSelected;
            _hudController.HideConfirmation();

            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var unit)) return;

            ClearActionSelectionVisuals();
            _hudController.OnUnitSelected(_selectedUnitId);

            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);
        }

        private void TryConfirmMove(int tileX, int tileY)
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var unit)) return;

            if (tileX == unit.TileX && tileY == unit.TileY) return;

            var reachable = GridMath.GetReachableTiles(_gameState, unit.TileX, unit.TileY, unit.MoveRange);
            if (!reachable.Contains((tileX, tileY))) return;

            _pendingMoveX = tileX;
            _pendingMoveY = tileY;

            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);
            _gridManager.HighlightTile(tileX, tileY, _pendingColor);

            _hudController.ShowMoveConfirmation(unit.TileX, unit.TileY, tileX, tileY);
        }

        private void TryConfirmShoot(int tileX, int tileY)
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var attacker)) return;

            var tile = _gameState.GetTile(tileX, tileY);
            if (tile == null || !tile.IsOccupied) return;

            var target = _gameState.Units[tile.OccupyingUnitId];
            if (target.OwnerId == attacker.OwnerId) return;

            int dist = GridMath.GetTileDistance(
                attacker.TileX,
                attacker.TileY,
                target.TileX,
                target.TileY);

            if (dist > attacker.AttackRange) return;

            if (!_losService.HasLOS(attacker.TileX, attacker.TileY, target.TileX, target.TileY)) return;

            _pendingTargetId = target.UnitId;

            int hitChance = _combatResolver.CalculateHitChance(attacker, target);
            _hudController.ShowShootConfirmation(target.UnitId, hitChance, attacker.Damage);
        }

        private void HandlePopupExecute()
        {
            switch (_state)
            {
                case SelectionState.MovePending:
                    var moveResult = _commandProcessor.Process(
                        new MoveCommand(
                            _turnController.CurrentPlayerId,
                            _selectedUnitId,
                            _pendingMoveX,
                            _pendingMoveY));
                    Debug.Log($"[SelectionManager] {moveResult.Message}");
                    EnterUnitSelected();
                    break;

                case SelectionState.ShootPending:
                    var shootResult = _commandProcessor.Process(
                        new ShootCommand(
                            _turnController.CurrentPlayerId,
                            _selectedUnitId,
                            _pendingTargetId));
                    Debug.Log($"[SelectionManager] {shootResult.Message}");
                    EnterUnitSelected();
                    break;
            }
        }

        private void HandlePopupCancel()
        {
            switch (_state)
            {
                case SelectionState.MovePending:
                    HighlightMoveRange();
                    break;

                case SelectionState.ShootPending:
                    HighlightAttackRange();
                    break;
            }
        }

        private void HighlightMoveRange()
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var unit)) return;

            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);

            var reachable = GridMath.GetReachableTiles(_gameState, unit.TileX, unit.TileY, unit.MoveRange);
            foreach (var (x, y) in reachable)
                _gridManager.HighlightTile(x, y, _moveRangeColor);
        }

        private void HighlightAttackRange()
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var unit)) return;

            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);

            foreach (var target in _gameState.Units.Values)
            {
                if (target.OwnerId == unit.OwnerId) continue;

                int dist = GridMath.GetTileDistance(
                    unit.TileX,
                    unit.TileY,
                    target.TileX,
                    target.TileY);

                if (dist <= unit.AttackRange &&
                    _losService.HasLOS(unit.TileX, unit.TileY, target.TileX, target.TileY))
                    _gridManager.HighlightTile(target.TileX, target.TileY, _attackRangeColor);
            }
        }

        public void ClearSelection()
        {
            _selectedUnitId = null;
            _pendingMoveX = 0;
            _pendingMoveY = 0;
            _pendingTargetId = null;
            _state = SelectionState.Idle;
            _gridManager.ResetAllTileColors();
            _hudController.OnSelectionCleared();
        }

        private void ClearActionSelectionVisuals()
        {
            _hudController.SetActionSelected(ActionType.Move, false);
            _hudController.SetActionSelected(ActionType.Shoot, false);
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
