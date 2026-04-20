using UnityEngine;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Services;

namespace FrontLine.Views
{
    /// <summary>
    /// Attach to a scene GameObject representing a grenade box.
    /// Configure TileX, TileY, and GrenadeType in the Inspector.
    /// The view destroys itself when the unit collects this box.
    /// </summary>
    public class GrenadeBoxView : MonoBehaviour
    {
        [Header("Collectable Config")]
        [SerializeField] private int _tileX;
        [SerializeField] private int _tileY;
        [SerializeField] private GrenadeType _grenadeType;

        private CommandProcessor _commandProcessor;

        public void Initialize(CommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
            _commandProcessor.OnGrenadeCollected += HandleGrenadeCollected;
        }

        private void OnDestroy()
        {
            if (_commandProcessor != null)
                _commandProcessor.OnGrenadeCollected -= HandleGrenadeCollected;
        }

        private void HandleGrenadeCollected(string unitId, GrenadeType type)
        {
            if (type != _grenadeType) return;

            // Check if this box was the one collected (it was already removed from GameState)
            var gameState = ServiceLocator.Instance.Get<GameState>();
            if (!gameState.GrenadeBoxes.ContainsKey((_tileX, _tileY)))
                Destroy(gameObject);
        }
    }
}
