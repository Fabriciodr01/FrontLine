using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;

namespace FrontLine.Services
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int _gridWidth = 10;
        [SerializeField] private int _gridHeight = 10;

        private void Awake()
        {
            InitializeServices();
        }

        private void InitializeServices()
        {
            // Clear any stale services from previous sessions
            ServiceLocator.Instance.Clear();

            // Build core systems in dependency order
            var gameState = new GameState(_gridWidth, _gridHeight);
            var stateMachine = new GameStateMachine();
            var turnController = new TurnController(gameState, stateMachine);
            var combatResolver = new CombatResolver();
            var cmdProcessor = new CommandProcessor(gameState, turnController);

            // Register everything
            ServiceLocator.Instance.Register(gameState);
            ServiceLocator.Instance.Register(stateMachine);
            ServiceLocator.Instance.Register(turnController);
            ServiceLocator.Instance.Register(combatResolver);
            ServiceLocator.Instance.Register(cmdProcessor);

            Debug.Log("[GameBootstrapper] All services initialized.");
        }
    }
}
