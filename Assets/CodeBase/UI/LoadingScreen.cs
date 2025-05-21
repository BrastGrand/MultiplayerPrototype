using TMPro;
using UnityEngine;
using CodeBase.Services.Message;
using Zenject;
using Cysharp.Threading.Tasks;

namespace CodeBase.UI
{
    public class LoadingScreen : BaseScreen
    {
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private float _animationSpeed = 1f;

        private string _baseText = "Loading";
        private int _dotCount;
        private float _timer;
        private IMessageService _messageService;
        private bool _isDestroyed = false;

        [Inject]
        public void Construct(IMessageService messageService)
        {
            _messageService = messageService;
        }

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

        private void OnEnable()
        {
            _messageService.Subscribe<SceneReadyMessage>(OnSceneReady);
        }

        private void OnDisable()
        {
            _messageService.Unsubscribe<SceneReadyMessage>(OnSceneReady);
        }

        public override async UniTask Show(float fadeDuration = 0.3f)
        {
            if (_isDestroyed) return;

            gameObject.SetActive(true);
            await Fade(0f, 1f, fadeDuration);
        }

        public override async UniTask Hide(float fadeDuration = 0.3f)
        {
            if (_isDestroyed) return;

            await Fade(1f, 0f, fadeDuration);
            gameObject.SetActive(false);
        }

        private async void OnSceneReady(SceneReadyMessage message)
        {
            if (_isDestroyed || gameObject == null)
            {
                Debug.LogWarning("LoadingScreen: Attempted to hide already destroyed loading screen");
                return;
            }

            try
            {
                await Hide();
                Destroy(gameObject);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LoadingScreen: Error while hiding: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }
    }
}