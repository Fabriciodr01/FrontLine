using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FrontLine.Input
{
    public class InputManager : MonoBehaviour
    {
        [Header("Drag Detection")]
        [SerializeField] private float _dragThresholdPixels = 10f;

        // Intents — other systems subscribe to these, never to raw input
        public event Action<Vector2> OnTapWorld;
        public event Action<Vector2> OnDragDelta;
        public event Action OnCancel;
        public event Action OnEndTurn;

        private GameInputActions _actions;

        // Internal drag state
        private bool _isHolding;
        private bool _isDragging;
        private Vector2 _holdStartPosition;
        private Vector2 _currentTapPosition;

        public void Initialize()
        {
            _actions = new GameInputActions();
            _actions.Enable();
            SubscribeActions();
        }

        private void OnDestroy()
        {
            if (_actions == null) return;
            UnsubscribeActions();
            _actions.Disable();
            _actions.Dispose();
        }

        private void SubscribeActions()
        {
            _actions.Gameplay.Tap.performed += OnTapPerformed;
            _actions.Gameplay.Hold.started += OnHoldStarted;
            _actions.Gameplay.Hold.canceled += OnHoldCanceled;
            _actions.Gameplay.Drag.performed += OnDragPerformed;
            _actions.Gameplay.Cancel.performed += OnCancelPerformed;
            _actions.UI.EndTurn.performed += OnEndTurnPerformed;
        }

        private void UnsubscribeActions()
        {
            _actions.Gameplay.Tap.performed -= OnTapPerformed;
            _actions.Gameplay.Hold.started -= OnHoldStarted;
            _actions.Gameplay.Hold.canceled -= OnHoldCanceled;
            _actions.Gameplay.Drag.performed -= OnDragPerformed;
            _actions.Gameplay.Cancel.performed -= OnCancelPerformed;
            _actions.UI.EndTurn.performed -= OnEndTurnPerformed;
        }

        // ── Raw input handlers ──────────────────────────────────────────

        private void OnTapPerformed(InputAction.CallbackContext ctx)
        {
            _currentTapPosition = ctx.ReadValue<Vector2>();
        }

        private void OnHoldStarted(InputAction.CallbackContext ctx)
        {
            _isHolding = true;
            _isDragging = false;
            _holdStartPosition = _currentTapPosition;
        }

        private void OnHoldCanceled(InputAction.CallbackContext ctx)
        {
            if (!_isDragging)
            {
                // Finger lifted without dragging — it's a tap intent
                OnTapWorld?.Invoke(_currentTapPosition);
            }

            _isHolding = false;
            _isDragging = false;
        }

        private void OnDragPerformed(InputAction.CallbackContext ctx)
        {
            if (!_isHolding) return;

            var delta = ctx.ReadValue<Vector2>();
            float moved = Vector2.Distance(_currentTapPosition, _holdStartPosition);

            if (!_isDragging && moved < _dragThresholdPixels) return;

            // Threshold crossed — commit to drag
            _isDragging = true;
            OnDragDelta?.Invoke(delta);
        }

        private void OnCancelPerformed(InputAction.CallbackContext ctx)
        {
            OnCancel?.Invoke();
        }

        private void OnEndTurnPerformed(InputAction.CallbackContext ctx)
        {
            OnEndTurn?.Invoke();
        }
    }
}
