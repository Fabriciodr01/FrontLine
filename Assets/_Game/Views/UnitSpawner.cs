using System.Collections.Generic;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Services;
using FrontLine.Controllers;

namespace FrontLine.Views
{
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Unit Settings")]
        [SerializeField] private GameObject _unitPrefab;
        [SerializeField] private Color _player1Color = Color.blue;
        [SerializeField] private Color _player2Color = Color.red;

        private GameState _gameState;
        private TurnController _turnController;
        private readonly Dictionary<string, UnitView> _unitViews = new();

        public void Initialize()
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();

            SpawnUnits();
            _turnController.StartGame();
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
        }

        public UnitView GetUnitView(string unitId)
        {
            _unitViews.TryGetValue(unitId, out var view);
            return view;
        }
    }
}
