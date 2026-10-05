using UnityEngine;
using FrontLine.InputSystem;

namespace FrontLine.Views
{
    public class CameraController : MonoBehaviour
    {
        [Header("Drag Settings")]
        [SerializeField] private float _dragSpeed = 0.02f;

        [Header("Bounds")]
        [SerializeField] private float _minX = -2f;
        [SerializeField] private float _maxX = 12f;
        [SerializeField] private float _minZ = -2f;
        [SerializeField] private float _maxZ = 12f;

        private InputManager _inputManager;

        public void Initialize(InputManager inputManager)
        {
            _inputManager = inputManager;
            _inputManager.OnDragDelta += HandleDrag;
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
                _inputManager.OnDragDelta -= HandleDrag;
        }

        private void HandleDrag(Vector2 delta)
        {
            var pos = transform.position;

            pos.x -= delta.x * _dragSpeed;
            pos.z -= delta.y * _dragSpeed;

            pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
            pos.z = Mathf.Clamp(pos.z, _minZ, _maxZ);

            transform.position = pos;
        }
    }
}
