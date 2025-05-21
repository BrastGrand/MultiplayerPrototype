using System.Collections.Generic;
using System.Linq;
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
        private readonly List<IScreen> _screens = new List<IScreen>();
        private readonly Dictionary<string, GameObject> _assetInstances = new Dictionary<string, GameObject>();

        public UIFactory(IAssetProvider assetProvider, DiContainer container)
        {
            _assetProvider = assetProvider;
            _container = container;
            _uiRoot = CreateUIRoot();
        }

        public async UniTask<T> CreateScreen<T>(string assetKey) where T : Component, IScreen
        {
            if (_screens.OfType<T>().Any())
                return _screens.OfType<T>().First() as T;

            var prefab = await _assetProvider.Load<GameObject>(assetKey);
            var instance = _container.InstantiatePrefab(prefab, _uiRoot);
            var screen = instance.GetComponent<T>();

            _screens.Add(screen);
            _assetInstances[assetKey] = instance;

            return screen;
        }

        public void CloseScreen<T>() where T : IScreen
        {
            var screen = _screens.OfType<T>().FirstOrDefault();
            if (screen == null) return;

            var instance = (screen as Component)?.gameObject;
            if (instance != null)
            {
                Object.Destroy(instance);
                _screens.Remove(screen);

                string key = _assetInstances.FirstOrDefault(x => x.Value == instance).Key;
                if (!string.IsNullOrEmpty(key))
                {
                    _assetProvider.ReleaseAssetsByLabel(key);
                    _assetInstances.Remove(key);
                }
            }
        }

        public T GetScreen<T>() where T : IScreen
        {
            return _screens.OfType<T>().FirstOrDefault();
        }

        private Transform CreateUIRoot()
        {
            var root = new GameObject("UIRoot").transform;
            Object.DontDestroyOnLoad(root.gameObject);
            return root;
        }
    }
}