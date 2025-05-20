using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Services.UIService
{
    public interface IUIFactory
    {
        UniTask<T> CreateScreen<T>(string assetKey) where T : Component, IScreen;
        void Dispose(string assetKey);
    }
}