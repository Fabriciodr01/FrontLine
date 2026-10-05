using System.Collections.Generic;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Commands;
using FrontLine.Map.Grid;

namespace FrontLine.Views
{
    /// <summary>
    /// Manages translucent smoke overlay planes on the grid.
    /// Refreshes on every turn start by reading TileData.HasSmoke from GameState.
    /// </summary>
    public class SmokeViewController : MonoBehaviour
    {
        [Header("Smoke Visual")]
        [SerializeField] private Color _smokeColor = new Color(0.7f, 0.7f, 0.7f, 0.55f);

        private GameState _gameState;
        private GridManager _gridManager;
        private TurnController _turnController;
        private CommandProcessor _commandProcessor;

        private readonly List<GameObject> _smokeOverlays = new List<GameObject>();

        public void Initialize(
            GameState gameState,
            GridManager gridManager,
            TurnController turnController,
            CommandProcessor commandProcessor)
        {
            _gameState = gameState;
            _gridManager = gridManager;
            _turnController = turnController;
            _commandProcessor = commandProcessor;

            _turnController.OnTurnStarted += HandleTurnStarted;
            _commandProcessor.OnCommandExecuted += HandleCommandExecuted;
        }

        private void OnDestroy()
        {
            if (_turnController != null)
                _turnController.OnTurnStarted -= HandleTurnStarted;
            if (_commandProcessor != null)
                _commandProcessor.OnCommandExecuted -= HandleCommandExecuted;
        }

        private void HandleTurnStarted(string playerId) => RefreshSmoke();

        private void HandleCommandExecuted(ICommand command, CommandResult result)
        {
            if (result.Success && command is ThrowGrenadeCommand throwCmd &&
                throwCmd.GrenadeType == GrenadeType.Smoke)
            {
                RefreshSmoke();
            }
        }

        private void RefreshSmoke()
        {
            // Destroy all existing overlays
            foreach (var go in _smokeOverlays)
                if (go != null) Destroy(go);
            _smokeOverlays.Clear();

            // Create new overlays for every tile that currently has smoke
            for (int x = 0; x < _gameState.GridWidth; x++)
            {
                for (int y = 0; y < _gameState.GridHeight; y++)
                {
                    var tile = _gameState.Grid[x, y];
                    if (!tile.HasSmoke) continue;

                    var worldPos = _gridManager.GetWorldPosition(x, y);
                    worldPos.y += 0.5f; // lift smoke cube origin above ground

                    var overlay = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    overlay.name = $"Smoke_{x}_{y}";
                    overlay.transform.position = worldPos;
                    overlay.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                    overlay.transform.localScale = new Vector3(_gridManager.TileStep, _gridManager.TileStep, 1f);

                    // Remove collider so it doesn't interfere with raycasts
                    Destroy(overlay.GetComponent<MeshCollider>());

                    var rend = overlay.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        rend.material.color = _smokeColor;
                        rend.material.SetFloat("_Mode", 3f); // Transparent mode
                        rend.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        rend.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        rend.material.SetInt("_ZWrite", 0);
                        rend.material.DisableKeyword("_ALPHATEST_ON");
                        rend.material.EnableKeyword("_ALPHABLEND_ON");
                        rend.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        rend.material.renderQueue = 3000;
                    }

                    _smokeOverlays.Add(overlay);
                }
            }
        }
    }
}
