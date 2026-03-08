using UnityEngine;
using FrontLine.Models;
using FrontLine.Services;
using System.Collections;

namespace FrontLine.Views
{
    public class UnitView : MonoBehaviour
    {
        public string UnitId { get; private set; }

        private GameState _gameState;
        private GridManager _gridManager;

        // Visual state
        private Renderer _renderer;
        private Color _ownerColor;
        private Vector3 _targetPosition;
        private bool _isMoving;
        private Transform _hpBarAnchor;

        [SerializeField] private float _moveSpeed = 5f;

        public void Initialize(string unitId, Color ownerColor)
        {
            UnitId = unitId;
            _ownerColor = ownerColor;

            _gameState = ServiceLocator.Instance.Get<GameState>();
            _gridManager = ServiceLocator.Instance.Get<GridManager>();
            _renderer = GetComponentInChildren<Renderer>();

            if (_renderer != null)
                _renderer.material.color = ownerColor;

            // Snap to initial position
            if (_gameState.Units.TryGetValue(unitId, out var unit))
            {
                transform.position = _gridManager.GetWorldPosition(unit.TileX, unit.TileY)
                    + Vector3.up * 0.6f;
            }
        }

        private void Update()
        {
            if (!_isMoving) return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _targetPosition,
                _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
            {
                transform.position = _targetPosition;
                _isMoving = false;
            }
        }

        public void OnMoved(int targetX, int targetY)
        {
            _targetPosition = _gridManager.GetWorldPosition(targetX, targetY)
                + Vector3.up * 0.6f;
            _isMoving = true;
        }

        public void OnDamaged(int currentHealth, int maxHealth)
        {
            // Flash red briefly
            if (_renderer != null)
                StartCoroutine(DamageFlash());

            Debug.Log($"[UnitView] {UnitId} damaged. HP: {currentHealth}/{maxHealth}");
        }

        public void OnKilled()
        {
            Debug.Log($"[UnitView] {UnitId} killed.");
            Destroy(gameObject);
        }

        private IEnumerator DamageFlash()
        {
            _renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            _renderer.material.color = _ownerColor;
        }

        public void SetExhausted(bool exhausted)
        {
            _renderer.material.color = exhausted ? Color.grey : _ownerColor;
        }
    }
}
