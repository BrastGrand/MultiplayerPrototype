using CodeBase.Infrastructure.AssetManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Services.UIService
{
    public class UIFactory : IUIFactory
    {
        private readonly IAssetProvider _assetProvider;
        private readonly DiContainer _container;
        private readonly Transform _uiRoot;

        public UIFactory(IAssetProvider assetProvider, DiContainer container)
        {
            _assetProvider = assetProvider;
            _container = container;
            _uiRoot = CreateUIRoot();
        }

        public async UniTask<T> CreateScreen<T>(string assetKey) where T : Component, IScreen
        {
            var prefab = await _assetProvider.Load<GameObject>(assetKey);
            var instance = _container.InstantiatePrefab(prefab);
            instance.transform.SetParent(_uiRoot);
            return instance.GetComponent<T>();
        }

        public void Dispose(string assetKey)
        {
            _assetProvider.ReleaseAssetsByLabel(assetKey);
        }

        private Transform CreateUIRoot()
        {
            var root = new GameObject("UIRoot").transform;
            Object.DontDestroyOnLoad(root.gameObject);
            return root;
        }
    }
}