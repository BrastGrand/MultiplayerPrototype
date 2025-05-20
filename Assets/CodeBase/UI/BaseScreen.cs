using CodeBase.Services.UIService;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.UI
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour, IScreen
    {
        [SerializeField] protected Canvas _canvas;
        [SerializeField] protected CanvasGroup _canvasGroup;

        public Canvas Canvas => _canvas;
        public bool IsVisible { get; private set; }

        public virtual async void Show(float fadeDuration = 0.5f)
        {
            _canvas.enabled = true;
            await Fade(0f, 1f, fadeDuration);
            IsVisible = true;
        }

        public virtual async void Hide(float fadeDuration = 0.5f)
        {
            await Fade(1f, 0f, fadeDuration);
            _canvas.enabled = false;
            IsVisible = false;
        }

        private async UniTask Fade(float from, float to, float duration)
        {
            _canvasGroup.alpha = from;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                await UniTask.Yield();
            }

            _canvasGroup.alpha = to;
        }

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }
    }
}