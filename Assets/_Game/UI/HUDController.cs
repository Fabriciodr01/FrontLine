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
        [Header("Turn Banner")]
        [SerializeField] private GameObject _turnBanner;
        [SerializeField] private TextMeshProUGUI _turnText;

        [Header("Unit List")]
        [SerializeField] private Transform _unitCardContainer;
        [SerializeField] private GameObject _unitCardPrefab;

        [Header("Action Bar")]
        [SerializeField] private GameObject _actionBar;
        [SerializeField] private Button _endTurnButton;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI _feedbackText;

        [Header("HP Bar")]
        [SerializeField] private GameObject _hpBarPrefab;

        private GameState _gameState;
        private TurnController _turnController;
        private CommandProcessor _commandProcessor;
        private SelectionManager _selectionManager;
        private InputManager _inputManager;

        private readonly Dictionary<string, UnitCard> _unitCards = new();
        private readonly Dictionary<string, WorldSpaceHPBar> _hpBars = new();

        private Coroutine _feedbackCoroutine;
        private Coroutine _bannerCoroutine;

        public void Initialize(SelectionManager selectionManager, InputManager inputManager)
        {
            _gameState = ServiceLocator.Instance.Get<GameState>();
            _turnController = ServiceLocator.Instance.Get<TurnController>();
            _commandProcessor = ServiceLocator.Instance.Get<CommandProcessor>();
            _selectionManager = selectionManager;
            _inputManager = inputManager;

            SubscribeEvents();
            SetupEndTurnButton(inputManager);

            _actionBar.SetActive(false);
            _turnBanner.SetActive(false);
        }

        private void SubscribeEvents()
        {
            _turnController.OnTurnStarted += HandleTurnStarted;
            _turnController.OnTurnEnded += HandleTurnEnded;
            _turnController.OnGameOver += HandleGameOver;
            _commandProcessor.OnCommandExecuted += HandleCommandExecuted;
            _commandProcessor.OnUnitDamaged += HandleUnitDamaged;
            _commandProcessor.OnUnitKilled += HandleUnitKilled;
        }

        private void OnDestroy()
        {
            _turnController.OnTurnStarted -= HandleTurnStarted;
            _turnController.OnTurnEnded -= HandleTurnEnded;
            _turnController.OnGameOver -= HandleGameOver;
            _commandProcessor.OnCommandExecuted -= HandleCommandExecuted;
            _commandProcessor.OnUnitDamaged -= HandleUnitDamaged;
            _commandProcessor.OnUnitKilled -= HandleUnitKilled;
        }

        private void SetupEndTurnButton(InputManager inputManager)
        {
            // Wire button click
            _endTurnButton.onClick.AddListener(() =>
            {
                var cmd = new EndTurnCommand(_turnController.CurrentPlayerId);
                _commandProcessor.Process(cmd);
            });

            // Also wire keyboard spacebar via InputManager
            inputManager.OnEndTurn += () =>
            {
                var cmd = new EndTurnCommand(_turnController.CurrentPlayerId);
                _commandProcessor.Process(cmd);
            };
        }

        // ── Unit Cards ──────────────────────────────────────────────────

        public void RegisterUnit(UnitData unit, Color ownerColor, Transform unitTransform)
        {
            // Unit card — only for Player 1 units for now
            if (unit.OwnerId == "Player1")
            {
                var cardObj = Instantiate(_unitCardPrefab, _unitCardContainer);
                var unitCard = cardObj.GetComponent<UnitCard>();
                unitCard.Initialize(unit, ownerColor);
                _unitCards[unit.UnitId] = unitCard;
            }

            // HP bar for all units
            var hpBarObj = Instantiate(_hpBarPrefab, unitTransform);
            var hpBar = hpBarObj.GetComponent<WorldSpaceHPBar>();
            hpBar.Initialize();
            hpBar.UpdateHP(unit.Health, unit.MaxHealth);
            hpBar.SetActive(false);
            _hpBars[unit.UnitId] = hpBar;
        }

        public void OnUnitSelected(string unitId)
        {
            _actionBar.SetActive(true);

            foreach (var kvp in _unitCards)
                kvp.Value.SetSelected(kvp.Key == unitId);

            foreach (var kvp in _hpBars)
                kvp.Value.SetActive(kvp.Key == unitId);
        }

        public void OnSelectionCleared()
        {
            _actionBar.SetActive(false);

            foreach (var kvp in _unitCards)
                kvp.Value.SetSelected(false);

            foreach (var kvp in _hpBars)
                kvp.Value.SetActive(false);
        }

        // ── Event Handlers ──────────────────────────────────────────────

        private void HandleTurnStarted(string playerId)
        {
            string label = playerId == "Player1" ? "YOUR TURN" : "ENEMY TURN";

            if (_bannerCoroutine != null) StopCoroutine(_bannerCoroutine);
            _bannerCoroutine = StartCoroutine(ShowTurnBanner(label));

            // Refresh all unit cards
            foreach (var kvp in _unitCards)
            {
                if (_gameState.Units.TryGetValue(kvp.Key, out var unit))
                    kvp.Value.Refresh(unit);
            }

            // Show action bar only on player turn
            bool isPlayerTurn = playerId == "Player1";
            _endTurnButton.interactable = isPlayerTurn;
        }

        private void HandleTurnEnded(string playerId)
        {
            _actionBar.SetActive(false);
            OnSelectionCleared();
        }

        private void HandleGameOver(string winnerId)
        {
            string msg = winnerId == "Player1" ? "VICTORY" : "DEFEAT";
            ShowFeedback(msg, 999f);
            _actionBar.SetActive(false);
        }

        private void HandleCommandExecuted(ICommand command, CommandResult result)
        {
            if (!result.Success) return;

            // Refresh card for the acting unit
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
                bar.UpdateHP(unit.Health, unit.MaxHealth);

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

        // ── Coroutines ──────────────────────────────────────────────────

        private IEnumerator ShowTurnBanner(string message)
        {
            _turnText.text = message;
            _turnBanner.SetActive(true);

            // Fade in
            var img = _turnBanner.GetComponent<Image>();
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                SetBannerAlpha(img, _turnText, Mathf.Lerp(0f, 1f, t / 0.3f));
                yield return null;
            }

            yield return new WaitForSeconds(1.2f);

            // Fade out
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

            yield return new WaitForSeconds(duration - 0.5f);

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
