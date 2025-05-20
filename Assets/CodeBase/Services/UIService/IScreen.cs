using UnityEngine;

namespace CodeBase.Services.UIService
{
    public interface IScreen
    {
        Canvas Canvas { get; }
        void Show(float fadeDuration = 0.5f);
        void Hide(float fadeDuration = 0.5f);
        bool IsVisible { get; }
    }
}