using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FrontLine.Models;

namespace FrontLine.UI
{
    public class UnitCard : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _colorIndicator;
        [SerializeField] private TextMeshProUGUI _unitNameText;
        [SerializeField] private TextMeshProUGUI _unitHPText;
        [SerializeField] private TextMeshProUGUI _unitAPText;
        [SerializeField] private Image _background;
        [SerializeField] private Slider _hpSlider;

        private string _unitId;
        private Color _ownerColor;

        private static readonly Color ActiveColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        private static readonly Color ExhaustedColor = new Color(0.05f, 0.05f, 0.05f, 0.5f);
        private static readonly Color SelectedColor = new Color(0.15f, 0.25f, 0.35f, 0.95f);

        public string UnitId => _unitId;

        public void Initialize(UnitData unit, Color ownerColor)
        {
            _unitId = unit.UnitId;
            _ownerColor = ownerColor;

            if (_colorIndicator != null)
                _colorIndicator.color = ownerColor;

            Refresh(unit);
        }

        public void Refresh(UnitData unit)
        {
            if (_unitNameText != null)
                _unitNameText.text = unit.UnitId;

            if (_unitHPText != null)
            {
                _unitHPText.text = $"HP  {unit.Health.Current}/{unit.Health.Max}";
                _unitHPText.color = unit.Health.Current <= 1
                    ? Color.red
                    : new Color(0.4f, 1f, 0.4f);
            }

            if (_hpSlider != null)
                _hpSlider.value = (float)unit.Health.Current / unit.Health.Max;

            if (_unitAPText != null)
            {
                _unitAPText.text = $"AP  {unit.ActionPoints}/{unit.MaxActionPoints}";
                _unitAPText.color = unit.ActionPoints > 0
                    ? new Color(1f, 0.85f, 0.2f)
                    : Color.grey;
            }

            if (_background != null)
                _background.color = unit.ActionPoints <= 0 ? ExhaustedColor : ActiveColor;
        }

        public void SetSelected(bool selected)
        {
            if (_background != null)
                _background.color = selected ? SelectedColor : ActiveColor;
        }
    }
}
