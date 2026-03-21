using System.Collections.Generic;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Services;
using FrontLine.Controllers;
using FrontLine.Commands;
using FrontLine.UI;

namespace FrontLine.Views
{
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Unit Settings")]
        [SerializeField] private GameObject _unitPrefab;
        [SerializeField] private Color _player1Color = Color.blue;
        [SerializeField] private Color _player2Color = Color.red;

        private HUDController _hudController;
        private GameState _gameState;
        private TurnController _turnController;
        private readonly Dictionary<string, UnitView> _unitViews = new();

        public void Initialize()
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();
            _hudController = ServiceLocator.Instance.Get<HUDController>();
            var cmdProcessor = ServiceLocator.Instance.Get<CommandProcessor>();

            SpawnUnits();
            cmdProcessor.OnUnitKilled += HandleUnitKilled;
            cmdProcessor.OnUnitDamaged += HandleUnitDamaged;
            cmdProcessor.OnCommandExecuted += HandleCommandExecuted;
            _turnController.OnTurnStarted += OnTurnStarted;
            _turnController.StartGame();
        }

        private void OnDestroy()
        {
            if (ServiceLocator.Instance.TryGet(out CommandProcessor cmdProcessor))
            {
                cmdProcessor.OnUnitKilled -= HandleUnitKilled;
                cmdProcessor.OnUnitDamaged -= HandleUnitDamaged;
                cmdProcessor.OnCommandExecuted -= HandleCommandExecuted;
            }

            if (_turnController != null)
                _turnController.OnTurnStarted -= OnTurnStarted;
        }

        private void SpawnUnits()
        {
            // Player 1 units — bottom of grid
            SpawnUnit("P1_Unit1", "Player1", 1, 1);
            SpawnUnit("P1_Unit2", "Player1", 3, 1);
            SpawnUnit("P1_Unit3", "Player1", 5, 1);

            // Player 2 units — top of grid
            SpawnUnit("P2_Unit1", "Player2", 1, 8);
            SpawnUnit("P2_Unit2", "Player2", 3, 8);
            SpawnUnit("P2_Unit3", "Player2", 5, 8);
        }

        private void SpawnUnit(string unitId, string ownerId, int tileX, int tileY)
        {
            // Register in GameState
            var unitData = new UnitData(unitId, ownerId, tileX, tileY);
            _gameState.AddUnit(unitData);

            // Spawn View
            var unitObj = Instantiate(_unitPrefab, transform);
            var unitView = unitObj.GetComponent<UnitView>();

            Color color = ownerId == "Player1" ? _player1Color : _player2Color;
            unitView.Initialize(unitId, color);

            unitObj.name = unitId;
            _unitViews[unitId] = unitView;
            _hudController.RegisterUnit(unitData, color, unitObj.transform);
        }

        private void HandleCommandExecuted(ICommand command, CommandResult result)
        {
            if (!result.Success) return;

            if (command is MoveCommand move)
            {
                if (_unitViews.TryGetValue(move.UnitId, out var view))
                {
                    view.OnMoved(move.TargetX, move.TargetY);
                    bool exhausted = !_turnController.HasActionPoints(move.UnitId);
                    view.SetExhausted(exhausted);
                }
            }
        }

        private void HandleUnitDamaged(UnitData unit)
        {
            if (_unitViews.TryGetValue(unit.UnitId, out var view))
                view.OnDamaged(unit.Health, unit.MaxHealth);
        }

        private void HandleUnitKilled(string unitId)
        {
            if (_unitViews.TryGetValue(unitId, out var view))
                view.OnKilled();

            _unitViews.Remove(unitId);
        }
        private void OnTurnStarted(string playerId)
        {
            foreach (var kvp in _unitViews)
            {
                var unitData = _gameState.Units[kvp.Key];
                if (unitData.OwnerId == playerId)
                    kvp.Value.SetExhausted(false);
            }
        }
        public UnitView GetUnitView(string unitId)
        {
            _unitViews.TryGetValue(unitId, out var view);
            return view;
        }
    }
}
