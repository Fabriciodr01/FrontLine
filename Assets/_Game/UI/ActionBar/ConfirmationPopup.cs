using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FrontLine.Models;

namespace FrontLine.UI
{
    public class ConfirmationPopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _detailText;
        [SerializeField] private Button _executeButton;
        [SerializeField] private TextMeshProUGUI _executeLabel;
        [SerializeField] private Button _cancelButton;

        public event Action OnExecute;
        public event Action OnCancel;
        private bool _isInitialized;

        public void Initialize()
        {
            if (_isInitialized) return;

            _executeButton.onClick.AddListener(HandleExecute);
            _cancelButton.onClick.AddListener(HandleCancel);
            _isInitialized = true;
            Hide();
        }

        private void OnDestroy()
        {
            if (!_isInitialized) return;

            _executeButton.onClick.RemoveListener(HandleExecute);
            _cancelButton.onClick.RemoveListener(HandleCancel);
        }

        public void ShowMove(int fromX, int fromY, int toX, int toY)
        {
            _titleText.text = "MOVER";
            _detailText.text = $"({fromX},{fromY}) → ({toX},{toY})";
            _executeLabel.text = "EXECUTAR";
            _root.SetActive(true);
        }

        public void ShowShoot(string targetId, int hitChance, int damage)
        {
            _titleText.text = "ATIRAR";
            _detailText.text = $"Alvo: {targetId}  |  Dano: {damage}";
            _executeLabel.text = $"EXECUTAR ({hitChance}%)";
            _root.SetActive(true);
        }

        public void ShowThrow(int targetX, int targetY, GrenadeType type)
        {
            string typeName = type == GrenadeType.Frag ? "FRAGMENTAÇÃO" : "FUMAÇA";
            _titleText.text = "LANÇAR GRANADA";
            _detailText.text = $"{typeName}  |  Alvo: ({targetX},{targetY})";
            _executeLabel.text = "EXECUTAR";
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void HandleExecute()
        {
            Hide();
            OnExecute?.Invoke();
        }

        private void HandleCancel()
        {
            Hide();
            OnCancel?.Invoke();
        }

    }
}
