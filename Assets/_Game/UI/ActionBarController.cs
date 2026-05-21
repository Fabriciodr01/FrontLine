using System;
using System.Collections.Generic;
using UnityEngine;
using FrontLine.Commands;
using FrontLine.UI.BaseComponents;

namespace FrontLine.UI
{
    public class ActionBarController : MonoBehaviour
    {
        [Header("Action Buttons")]
        [SerializeField] private List<ActionButton> _actionButtons;

        [Header("End Turn")]
        [SerializeField] private BaseButton _endTurnButton;

        public event Action<ActionType> OnActionPressed;
        public event Action OnEndTurnPressed;
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized) return;

            foreach (var btn in _actionButtons)
                btn.OnActionClicked += HandleActionClicked;

            _endTurnButton.OnClicked += HandleEndTurnClicked;
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            if (!_isInitialized) return;

            foreach (var btn in _actionButtons)
                btn.OnActionClicked -= HandleActionClicked;

            _endTurnButton.OnClicked -= HandleEndTurnClicked;
        }

        private void HandleActionClicked(ActionType actionType)
            => OnActionPressed?.Invoke(actionType);

        private void HandleEndTurnClicked()
            => OnEndTurnPressed?.Invoke();

        // TODO-POST-ALPHA: replace bool params with ActionDefinition ScriptableObjects
        // so each action type carries its own label, icon and availability logic
        public void RefreshForUnit(bool hasMoveAP, bool hasShootAP, bool hasFragGrenade, bool hasSmokeGrenade)
        {
            foreach (var btn in _actionButtons)
            {
                switch (btn.ActionType)
                {
                    case ActionType.Move:
                        btn.SetInteractable(hasMoveAP);
                        break;
                    case ActionType.Shoot:
                        btn.SetInteractable(hasShootAP);
                        break;
                    case ActionType.Throw:
                        btn.SetInteractable(hasFragGrenade);
                        break;
                    case ActionType.UseConsumable:
                        btn.SetInteractable(hasSmokeGrenade);
                        break;
                }

                btn.SetSelected(false);
            }
        }

        public void SetActionSelected(ActionType actionType, bool selected)
        {
            foreach (var btn in _actionButtons)
                if (btn.ActionType == actionType)
                    btn.SetSelected(selected);
        }

        public void SetEndTurnInteractable(bool interactable)
            => _endTurnButton.SetInteractable(interactable);

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
