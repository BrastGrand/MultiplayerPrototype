using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Services.UIService
{
    public interface IUIFactory
    {
        UniTask<T> CreateScreen<T>(string assetKey) where T : Component, IScreen;
        void CloseScreen<T>() where T : IScreen;
        T GetScreen<T>() where T : IScreen;
    }
}