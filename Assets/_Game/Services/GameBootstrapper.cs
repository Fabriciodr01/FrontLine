using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Views;

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

            Debug.Log("[GameBootstrapper] Services registered.");
        }

        private void InitializeScene()
        {
            _gridManager.Initialize();
            _unitSpawner.Initialize();

            Debug.Log("[GameBootstrapper] Scene initialized.");
        }
    }
}
