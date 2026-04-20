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

        [Header("Scene References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private UnitSpawner _unitSpawner;
        [SerializeField] private InputManager _inputManager;
        [SerializeField] private SelectionManager _selectionManager;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private HUDController _hudController;
        [SerializeField] private AIInputHandler _aiInputHandler;
        [SerializeField] private SmokeViewController _smokeViewController;
        [SerializeField] private GrenadeBoxView[] _grenadeBoxViews;

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
            ConfigureObstacles(gameState);

            var losService     = new LineOfSightService(gameState);
            var combatResolver = new CombatResolver(gameState, losService);
            var cmdProcessor   = new CommandProcessor(gameState, _turnController);

            ConfigureCollectables(gameState);

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

            var cmdProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            if (_smokeViewController != null)
                _smokeViewController.Initialize(
                    ServiceLocator.Instance.Get<GameState>(),
                    _gridManager,
                    ServiceLocator.Instance.Get<TurnController>(),
                    cmdProcessor);

            if (_grenadeBoxViews != null)
                foreach (var view in _grenadeBoxViews)
                    if (view != null) view.Initialize(cmdProcessor);

            Debug.Log("[GameBootstrapper] Scene initialized.");
        }

        private void ConfigureCollectables(GameState gameState)
        {
            // Symmetrical placement near the chokepoints — contested by both players.
            // (5,4)  frag  — P1-side of left chokepoint
            // (10,9) frag  — P2-side of right chokepoint
            // (5,9)  smoke — P2-side of left chokepoint
            // (10,4) smoke — P1-side of right chokepoint
            gameState.AddGrenadeBox(new GrenadeBox(GrenadeType.Frag,  5,  4));
            gameState.AddGrenadeBox(new GrenadeBox(GrenadeType.Frag,  10, 9));
            gameState.AddGrenadeBox(new GrenadeBox(GrenadeType.Smoke, 5,  9));
            gameState.AddGrenadeBox(new GrenadeBox(GrenadeType.Smoke, 10, 4));
        }

        private void SetBlocked(GameState gameState, int x, int y)
        {
            var tile = gameState.GetTile(x, y);
            if (tile == null)
            {
                Debug.LogWarning($"[GameBootstrapper] Obstacle at ({x},{y}) is out of grid bounds ({gameState.GridWidth}x{gameState.GridHeight}) — skipped.");
                return;
            }
            tile.Type = TileType.Blocked;
        }

        private void ConfigureObstacles(GameState gameState)
        {
            // Central wall at y=6 and y=7, x=1..14, with chokepoint gaps at x=5 and x=10
            for (int x = 1; x <= 14; x++)
            {
                if (x == 5 || x == 10) continue;
                SetBlocked(gameState, x, 6);
                SetBlocked(gameState, x, 7);
            }

            // P1-side cover
            SetBlocked(gameState, 2,  3);
            SetBlocked(gameState, 3,  3);
            SetBlocked(gameState, 12, 3);
            SetBlocked(gameState, 13, 3);

            // P2-side cover
            SetBlocked(gameState, 2,  10);
            SetBlocked(gameState, 3,  10);
            SetBlocked(gameState, 12, 10);
            SetBlocked(gameState, 13, 10);
        }
    }
}

