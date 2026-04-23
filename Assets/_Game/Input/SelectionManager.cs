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
        [SerializeField] private Color _dashRangeColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color _pathPreviewColor = new Color(0.7f, 0.95f, 1f, 1f);
        [SerializeField] private Color _attackRangeColor = new Color(1f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color _selectedColor = new Color(1f, 1f, 0f, 1f);
        [SerializeField] private Color _pendingColor = new Color(0.4f, 1f, 0.4f, 1f);

        private GameState _gameState;
        private TurnController _turnController;
        private CommandProcessor _commandProcessor;
        private GridManager _gridManager;
        private CombatResolver _combatResolver;
        private InputManager _inputManager;
        private HUDController _hudController;
        private Camera _camera;

        private bool _inputEnabled;

        private SelectionState _state = SelectionState.Idle;
        private string _selectedUnitId;
        private Vector2? _pendingTap;

        private int _pendingMoveX;
        private int _pendingMoveY;
        private bool _pendingMoveIsDash;
        private string _pendingTargetId;

        public void Initialize(InputManager inputManager, HUDController hudController)
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();
            _commandProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            _gridManager = ServiceLocator.Instance.Get<GridManager>();
            _combatResolver = ServiceLocator.Instance.Get<CombatResolver>();
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

            _gridManager.HideMovePath();
            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);
        }

        private void TryConfirmMove(int tileX, int tileY)
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var unit)) return;

            if (tileX == unit.TileX && tileY == unit.TileY) return;

            var walkZone = GridMath.GetReachableTiles(_gameState, unit.TileX, unit.TileY, unit.MoveRange);
            if (walkZone.Contains((tileX, tileY)))
            {
                _pendingMoveIsDash = false;
            }
            else if (unit.ActionPoints >= unit.MaxActionPoints)
            {
                var dashZone = GridMath.GetReachableTiles(_gameState, unit.TileX, unit.TileY, unit.DashRange);
                if (!dashZone.Contains((tileX, tileY))) return;
                _pendingMoveIsDash = true;
            }
            else
            {
                return;
            }

            _pendingMoveX = tileX;
            _pendingMoveY = tileY;

            var path = GridMath.GetPath(_gameState, unit.TileX, unit.TileY, tileX, tileY);

            Color lineColor = _pendingMoveIsDash ? _dashRangeColor : _pathPreviewColor;
            Color destColor = _pendingMoveIsDash ? _dashRangeColor : _pendingColor;

            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);
            _gridManager.ShowMovePath(path, unit.TileX, unit.TileY, lineColor);
            _gridManager.HighlightTile(tileX, tileY, destColor);

            _hudController.ShowMoveConfirmation(unit.TileX, unit.TileY, tileX, tileY);
        }

        private void TryConfirmShoot(int tileX, int tileY)
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var attacker)) return;

            var tile = _gameState.GetTile(tileX, tileY);
            if (tile == null || !tile.IsOccupied) return;

            var target = _gameState.Units[tile.OccupyingUnitId];
            if (target.OwnerId == attacker.OwnerId) return;

            var eval = _combatResolver.EvaluateAttack(new AttackContext(attacker, target));
            if (!eval.CanAttack) return;

            _pendingTargetId = target.UnitId;
            _hudController.ShowShootConfirmation(target.UnitId, eval.HitChance, attacker.Damage);
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
                            _pendingMoveY,
                            _pendingMoveIsDash));
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

            _gridManager.HideMovePath();
            _gridManager.ResetAllTileColors();

            var walkZone = GridMath.GetReachableTiles(_gameState, unit.TileX, unit.TileY, unit.MoveRange);

            // Show dash zone (amber-yellow) only when unit still has full AP
            if (unit.ActionPoints >= unit.MaxActionPoints)
            {
                var dashZone = GridMath.GetReachableTiles(_gameState, unit.TileX, unit.TileY, unit.DashRange);
                foreach (var (x, y) in dashZone)
                    if (!walkZone.Contains((x, y)))
                        _gridManager.HighlightTile(x, y, _dashRangeColor);
            }

            foreach (var (x, y) in walkZone)
                _gridManager.HighlightTile(x, y, _moveRangeColor);

            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);
        }

        private void HighlightAttackRange()
        {
            if (!_gameState.Units.TryGetValue(_selectedUnitId, out var unit)) return;

            _gridManager.HideMovePath();
            _gridManager.ResetAllTileColors();
            _gridManager.HighlightTile(unit.TileX, unit.TileY, _selectedColor);

            foreach (var target in _gameState.Units.Values)
            {
                if (target.OwnerId == unit.OwnerId) continue;
                var eval = _combatResolver.EvaluateAttack(new AttackContext(unit, target));
                if (eval.CanAttack)
                    _gridManager.HighlightTile(target.TileX, target.TileY, _attackRangeColor);
            }
        }

        public void ClearSelection()
        {
            _selectedUnitId = null;
            _pendingMoveX = 0;
            _pendingMoveY = 0;
            _pendingMoveIsDash = false;
            _pendingTargetId = null;
            _state = SelectionState.Idle;
            _gridManager.HideMovePath();
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
