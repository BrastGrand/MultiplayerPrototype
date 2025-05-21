using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Services.UIService
{
    public interface IScreen
    {
        Canvas Canvas { get; }
        UniTask Show(float fadeDuration = 0.5f);
        UniTask Hide(float fadeDuration = 0.5f);
        bool IsVisible { get; }
    }
}