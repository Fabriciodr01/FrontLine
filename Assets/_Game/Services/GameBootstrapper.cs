using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Input;
using FrontLine.Views;
using FrontLine.UI;

namespace FrontLine.Services
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _gridWidth = 10;
        [SerializeField] private int _gridHeight = 10;

        [Header("Scene References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private UnitSpawner _unitSpawner;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private SelectionManager _selectionManager;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private HUDController _hudController;

        private void Awake()
        {
            InitializeServices();
            InitializeScene();
        }

        private void InitializeServices()
        {
            ServiceLocator.Instance.Clear();

            var gameState = new GameState(_gridWidth, _gridHeight);
            var stateMachine = new GameStateMachine();
            var turnController = new TurnController(gameState, stateMachine);
            var combatResolver = new CombatResolver(gameState);
            var cmdProcessor = new CommandProcessor(gameState, turnController);

            ServiceLocator.Instance.Register(gameState);
            ServiceLocator.Instance.Register(stateMachine);
            ServiceLocator.Instance.Register(turnController);
            ServiceLocator.Instance.Register(combatResolver);
            ServiceLocator.Instance.Register(cmdProcessor);
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
            _unitSpawner.Initialize();

            Debug.Log("[GameBootstrapper] Scene initialized.");
        }
    }
}
