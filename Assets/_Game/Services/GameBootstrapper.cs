using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Input;
using FrontLine.Views;
using FrontLine.UI;
using FrontLine.AI;

namespace FrontLine.Services
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _gridWidth = 16;
        [SerializeField] private int _gridHeight = 14;

        [Header("Map Data")]
        [SerializeField] private MapData _mapData;

        [Header("Scene References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private UnitSpawner _unitSpawner;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private SelectionManager _selectionManager;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private HUDController _hudController;
        [SerializeField] private AIInputHandler _aiInputHandler;

        private TurnController _turnController;

        private void Awake()
        {
            InitializeServices();
            InitializeScene();
        }

        private void OnDestroy()
        {
            if (_turnController == null) return;
            _turnController.OnTurnStarted -= _aiInputHandler.OnTurnStarted;
            _turnController.OnTurnEnded   -= _aiInputHandler.OnTurnEnded;
            _turnController.OnTurnStarted -= _selectionManager.OnTurnStarted;
            _turnController.OnTurnEnded   -= _selectionManager.OnTurnEnded;
        }

        private void InitializeServices()
        {
            ServiceLocator.Instance.Clear();

            var gameState = new GameState(_gridWidth, _gridHeight);
            var stateMachine = new GameStateMachine();
            _turnController = new TurnController(gameState, stateMachine);
            new MapLoader(gameState, _mapData).Load();

            var losService     = new LineOfSightService(gameState);
            var combatResolver = new CombatResolver(gameState, losService);
            var cmdProcessor   = new CommandProcessor(gameState, _turnController);

            ServiceLocator.Instance.Register(gameState);
            ServiceLocator.Instance.Register(stateMachine);
            ServiceLocator.Instance.Register(_turnController);
            ServiceLocator.Instance.Register(combatResolver);
            ServiceLocator.Instance.Register(cmdProcessor);
            ServiceLocator.Instance.Register(losService);
            ServiceLocator.Instance.Register(_gridManager);
            ServiceLocator.Instance.Register(_hudController);

            Debug.Log("[GameBootstrapper] Services registered.");
        }

        private void InitializeScene()
        {
            _gridManager.Initialize();
            _inputManager.Initialize();
            _hudController.Initialize(_inputManager);
            _selectionManager.Initialize(_inputManager, _hudController);
            _cameraController.Initialize(_inputManager);

            _aiInputHandler.Initialize();
            _turnController.OnTurnStarted += _aiInputHandler.OnTurnStarted;
            _turnController.OnTurnEnded   += _aiInputHandler.OnTurnEnded;
            _turnController.OnTurnStarted += _selectionManager.OnTurnStarted;
            _turnController.OnTurnEnded   += _selectionManager.OnTurnEnded;

            _unitSpawner.Initialize();

            Debug.Log("[GameBootstrapper] Scene initialized.");
        }

    }
}

