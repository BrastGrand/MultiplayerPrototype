using TMPro;
using UnityEngine;

namespace CodeBase.UI
{
    public class LoadingScreen : BaseScreen
    {
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private float _animationSpeed = 1f;

        private string _baseText = "Loading";
        private int _dotCount;
        private float _timer;

        protected override void Awake()
        {
            base.Awake();
            UpdateText();
        }

        private void Update()
        {
            if (!IsVisible) return;

            _timer += Time.deltaTime * _animationSpeed;
            if (_timer >= 1f)
            {
                _timer = 0f;
                _dotCount = (_dotCount + 1) % 4;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            _loadingText.text = _baseText + new string('.', _dotCount);
        }
    }
}