using System;
using UnityEngine;
using FrontLine.Commands;
using FrontLine.UI.BaseComponents;

namespace FrontLine.UI
{
    public class ActionButton : BaseButton
    {
        [Header("Action")]
        [SerializeField] private ActionType _actionType;

        public ActionType ActionType => _actionType;

        public event Action<ActionType> OnActionClicked;

        protected override void HandleClick()
        {
            base.HandleClick();
            OnActionClicked?.Invoke(_actionType);
        }
    }
}
