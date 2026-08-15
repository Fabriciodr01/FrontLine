using UnityEngine;
using UnityEngine.UI;

namespace FrontLine.UI
{
    public class WorldSpaceHPBar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Canvas _canvas;
        private Vector3 _offset = new Vector3(-0.8f, 1.06f, 0.082f);

        private static readonly Color FullHP = new Color(0.3f, 1f, 0.3f, 0.9f);
        private static readonly Color MidHP = new Color(1f, 0.8f, 0f, 0.9f);
        private static readonly Color LowHP = new Color(1f, 0.2f, 0.2f, 0.9f);

        public void Initialize()
        {
            if (_canvas != null)
                _canvas.worldCamera = Camera.main;
            transform.localPosition = _offset;
            transform.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
        public void UpdateHP(int current, int max)
        {
            if (_slider == null) return;

            float ratio = (float)current / max;
            _slider.value = ratio;

            if (_fillImage != null)
            {
                _fillImage.color = ratio > 0.6f ? FullHP
                                 : ratio > 0.3f ? MidHP
                                 : LowHP;
            }
        }

        public void SetActive(bool active)
        {
            if (_fillImage == null) return;
            var c = _fillImage.color;
            _fillImage.color = new Color(c.r, c.g, c.b, active ? 0.9f : 0.25f);
        }
    }
}
