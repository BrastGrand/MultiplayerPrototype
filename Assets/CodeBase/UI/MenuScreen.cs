using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class MenuScreen : BaseScreen
    {
        [SerializeField] private Button _hostButton;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        public event Action OnHostClicked;
        public event Action OnJoinClicked;
        public event Action OnQuitClicked;

        protected override void Awake()
        {
            base.Awake();
            _hostButton.onClick.AddListener(() => OnHostClicked?.Invoke());
            _joinButton.onClick.AddListener(() => OnJoinClicked?.Invoke());
            _quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public void SetStatus(string text, bool isError = false)
        {
            _statusText.text = text;
            _statusText.color = isError ? Color.red : Color.white;
        }

        public void SetButtonsInteractable(bool interactable)
        {
            _hostButton.interactable = interactable;
            _joinButton.interactable = interactable;
        }
    }
}