using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FrontLine.Models;
using FrontLine.Controllers;
using FrontLine.Commands;
using FrontLine.Services;
using FrontLine.Input;

namespace FrontLine.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Subcomponents")]
        [SerializeField] private ActionBarController _actionBar;
        [SerializeField] private ConfirmationPopup _confirmationPopup;

        [Header("Turn Banner")]
        [SerializeField] private GameObject _turnBanner;
        [SerializeField] private TextMeshProUGUI _turnText;

        [Header("Unit List")]
        [SerializeField] private Transform _unitCardContainer;
        [SerializeField] private GameObject _unitCardPrefab;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI _feedbackText;

        [Header("HP Bar")]
        [SerializeField] private GameObject _hpBarPrefab;

        public event Action<ActionType> OnActionPressed;
        public event Action OnPopupExecute;
        public event Action OnPopupCancel;

        private GameState _gameState;
        private TurnController _turnController;
        private CommandProcessor _commandProcessor;
        private InputManager _inputManager;

        private readonly Dictionary<string, UnitCard> _unitCards = new();
        private readonly Dictionary<string, WorldSpaceHPBar> _hpBars = new();

        private Coroutine _feedbackCoroutine;
        private Coroutine _bannerCoroutine;
        private bool _subcomponentEventsSubscribed;

        public void Initialize(InputManager inputManager)
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();
            _commandProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            _inputManager = inputManager;

            _actionBar.Initialize();
            _confirmationPopup.Initialize();
            SubscribeGameEvents();
            SubscribeSubcomponentEvents();
            SetupEndTurnButton();

            _turnBanner.SetActive(false);
            _actionBar.Hide();
            _confirmationPopup.Hide();
        }

        private void SubscribeGameEvents()
        {
            _turnController.OnTurnStarted += HandleTurnStarted;
            _turnController.OnTurnEnded += HandleTurnEnded;
            _turnController.OnGameOver += HandleGameOver;
            _commandProcessor.OnCommandExecuted += HandleCommandExecuted;
            _commandProcessor.OnUnitDamaged += HandleUnitDamaged;
            _commandProcessor.OnUnitKilled += HandleUnitKilled;
        }

        private void SubscribeSubcomponentEvents()
        {
            // Forward subcomponent events upward — SelectionManager never touches subcomponents
            _actionBar.OnActionPressed += HandleActionBarActionPressed;
            _actionBar.OnEndTurnPressed += HandleEndTurnPressed;
            _confirmationPopup.OnExecute += HandlePopupExecuteForwarded;
            _confirmationPopup.OnCancel += HandlePopupCancelForwarded;
            _subcomponentEventsSubscribed = true;
        }

        private void SetupEndTurnButton()
        {
            _inputManager.OnEndTurn += HandleEndTurnPressed;
        }

        private void OnDestroy()
        {
            if (_turnController != null)
            {
                _turnController.OnTurnStarted -= HandleTurnStarted;
                _turnController.OnTurnEnded -= HandleTurnEnded;
                _turnController.OnGameOver -= HandleGameOver;
            }

            if (_commandProcessor != null)
            {
                _commandProcessor.OnCommandExecuted -= HandleCommandExecuted;
                _commandProcessor.OnUnitDamaged -= HandleUnitDamaged;
                _commandProcessor.OnUnitKilled -= HandleUnitKilled;
            }

            if (_subcomponentEventsSubscribed)
            {
                _actionBar.OnActionPressed -= HandleActionBarActionPressed;
                _actionBar.OnEndTurnPressed -= HandleEndTurnPressed;
                _confirmationPopup.OnExecute -= HandlePopupExecuteForwarded;
                _confirmationPopup.OnCancel -= HandlePopupCancelForwarded;
            }

            if (_inputManager != null)
                _inputManager.OnEndTurn -= HandleEndTurnPressed;
        }

        public void RegisterUnit(UnitData unit, Color ownerColor, Transform unitTransform)
        {
            if (unit.OwnerId == "Player1")
            {
                var cardObj = Instantiate(_unitCardPrefab, _unitCardContainer);
                var unitCard = cardObj.GetComponent<UnitCard>();
                unitCard.Initialize(unit, ownerColor);
                _unitCards[unit.UnitId] = unitCard;
            }

            var hpBarObj = Instantiate(_hpBarPrefab, unitTransform);
            var hpBar = hpBarObj.GetComponent<WorldSpaceHPBar>();
            hpBar.Initialize();
            hpBar.UpdateHP(unit.Health.Current, unit.Health.Max);
            hpBar.SetActive(false);
            _hpBars[unit.UnitId] = hpBar;
        }

        public void OnUnitSelected(string unitId)
        {
            if (!_gameState.Units.TryGetValue(unitId, out var unit)) return;

            bool hasAP = _turnController.HasActionPoints(unitId);
            _actionBar.RefreshForUnit(hasAP, hasAP);
            _actionBar.Show();

            foreach (var kvp in _unitCards)
                kvp.Value.SetSelected(kvp.Key == unitId);

            foreach (var kvp in _hpBars)
                kvp.Value.SetActive(kvp.Key == unitId);
        }

        public void OnSelectionCleared()
        {
            _actionBar.Hide();
            _confirmationPopup.Hide();

            foreach (var kvp in _unitCards)
                kvp.Value.SetSelected(false);

            foreach (var kvp in _hpBars)
                kvp.Value.SetActive(false);
        }

        public void SetActionSelected(ActionType actionType, bool selected)
            => _actionBar.SetActionSelected(actionType, selected);

        public void ShowMoveConfirmation(int fromX, int fromY, int toX, int toY)
            => _confirmationPopup.ShowMove(fromX, fromY, toX, toY);

        public void ShowShootConfirmation(string targetId, int hitChance, int damage)
            => _confirmationPopup.ShowShoot(targetId, hitChance, damage);

        public void HideConfirmation()
            => _confirmationPopup.Hide();

        private void HandleActionBarActionPressed(ActionType action)
            => OnActionPressed?.Invoke(action);

        private void HandlePopupExecuteForwarded()
            => OnPopupExecute?.Invoke();

        private void HandlePopupCancelForwarded()
            => OnPopupCancel?.Invoke();

        private void HandleEndTurnPressed()
        {
            var cmd = new EndTurnCommand(_turnController.CurrentPlayerId);
            _commandProcessor.Process(cmd);
        }

        private void HandleTurnStarted(string playerId)
        {
            string label = playerId == "Player1" ? "SUA VEZ" : "VEZ DO INIMIGO";

            if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = StartCoroutine(ShowTurnBanner(label));

            foreach (var kvp in _unitCards)
                if (_gameState.Units.TryGetValue(kvp.Key, out var unit))
                    kvp.Value.Refresh(unit);

            _actionBar.SetEndTurnInteractable(playerId == "Player1");
        }

        private void HandleTurnEnded(string playerId)
        {
            OnSelectionCleared();
        }

        private void HandleGameOver(string winnerId)
        {
            string msg = winnerId == "Player1" ? "VITÓRIA!" : "DERROTA!";
            ShowFeedback(msg, 999f);
            OnSelectionCleared();
        }

        private void HandleCommandExecuted(ICommand command, CommandResult result)
        {
            if (!result.Success) return;

            if (command.UnitId != null &&
                _unitCards.TryGetValue(command.UnitId, out var card) &&
                _gameState.Units.TryGetValue(command.UnitId, out var unit))
            {
                card.Refresh(unit);
            }

            ShowFeedback(result.Message, 2f);
        }

        private void HandleUnitDamaged(UnitData unit)
        {
            if (_hpBars.TryGetValue(unit.UnitId, out var bar))
                bar.UpdateHP(unit.Health.Current, unit.Health.Max);

            if (_unitCards.TryGetValue(unit.UnitId, out var card))
                card.Refresh(unit);
        }

        private void HandleUnitKilled(string unitId)
        {
            if (_hpBars.TryGetValue(unitId, out var bar))
            {
                Destroy(bar.gameObject);
                _hpBars.Remove(unitId);
            }

            if (_unitCards.TryGetValue(unitId, out var card))
            {
                Destroy(card.gameObject);
                _unitCards.Remove(unitId);
            }
        }

        private IEnumerator ShowTurnBanner(string message)
        {
            _turnText.text = message;
            _turnBanner.SetActive(true);

            var img = _turnBanner.GetComponent<Image>();
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                SetBannerAlpha(img, _turnText, Mathf.Lerp(0f, 1f, t / 0.3f));
                yield return null;
            }

            yield return new WaitForSeconds(1.2f);

            t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                SetBannerAlpha(img, _turnText, Mathf.Lerp(1f, 0f, t / 0.4f));
                yield return null;
            }

            _turnBanner.SetActive(false);
        }

        private void SetBannerAlpha(Image img, TextMeshProUGUI text, float alpha)
        {
            if (img != null)
            {
                var c = img.color;
                img.color = new Color(c.r, c.g, c.b, alpha * 0.6f);
            }
            var tc = text.color;
            text.color = new Color(tc.r, tc.g, tc.b, alpha);
        }

        private void ShowFeedback(string message, float duration)
        {
            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(FeedbackFade(message, duration));
        }

        private IEnumerator FeedbackFade(string message, float duration)
        {
            _feedbackText.text = message;
            _feedbackText.alpha = 1f;

            yield return new WaitForSeconds(Mathf.Max(0f, duration - 0.5f));

            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                _feedbackText.alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
                yield return null;
            }

            _feedbackText.alpha = 0f;
        }
    }
}
