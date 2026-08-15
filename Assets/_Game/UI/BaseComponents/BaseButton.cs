using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FrontLine.UI.BaseComponents
{
    [RequireComponent(typeof(Button))]
    public class BaseButton : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Image _background;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.8f, 0.1f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        protected Button Button { get; private set; }

        public event Action OnClicked;

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            if (Button != null)
                Button.onClick.RemoveListener(HandleClick);
        }

        protected virtual void HandleClick()
        {
            OnClicked?.Invoke();
        }

        public virtual void SetInteractable(bool interactable)
        {
            Button.interactable = interactable;
            RefreshVisual(interactable, false);
        }

        public virtual void SetSelected(bool selected)
        {
            RefreshVisual(Button.interactable, selected);
        }

        public void SetLabel(string text)
        {
            if (_label != null)
                _label.text = text;
        }

        private void RefreshVisual(bool interactable, bool selected)
        {
            if (_background == null) return;

            _background.color = !interactable ? _disabledColor
                              : selected ? _selectedColor
                              : _normalColor;
        }

        private void Initialize()
        {
            if (Button == null)
            {
                Button = GetComponent<Button>();
                Button.onClick.AddListener(HandleClick);
            }

            if (_background == null)
                _background = GetComponent<Image>();
        }
    }
}
