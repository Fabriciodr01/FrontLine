using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrontLine.Models;
using FrontLine.Services;

namespace FrontLine.Views
{
    public class UnitView : MonoBehaviour
    {
        public string UnitId { get; private set; }

        private GameState _gameState;
        private GridManager _gridManager;

        private Renderer _renderer;
        private Color _ownerColor;
        private Coroutine _moveCoroutine;
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

        public void OnMoved(List<(int x, int y)> path)
        {
            if (_moveCoroutine != null) StopCoroutine(_moveCoroutine);
            if (path != null && path.Count > 0)
                _moveCoroutine = StartCoroutine(FollowPath(path));
        }

        private IEnumerator FollowPath(List<(int x, int y)> path)
        {
            foreach (var (tileX, tileY) in path)
            {
                var target = _gridManager.GetWorldPosition(tileX, tileY) + Vector3.up * 0.6f;
                while (Vector3.Distance(transform.position, target) >= 0.01f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position, target, _moveSpeed * Time.deltaTime);
                    yield return null;
                }
                transform.position = target;
            }
            _moveCoroutine = null;
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
