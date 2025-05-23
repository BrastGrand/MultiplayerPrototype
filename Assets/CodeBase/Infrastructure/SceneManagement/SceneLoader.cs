using System.Threading.Tasks;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.Log;
using CodeBase.Services.Message;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CodeBase.Infrastructure.SceneManagement
{
    public class SceneLoader : INetworkSceneManager, ISceneLoader
    {
        private ILogService _logService;
        private IMessageService _messageService;
        private IAssetProvider _assetProvider;
        private string _currentScene;
        private NetworkRunner _runner;
        private bool _isBusy;

        [Inject]
        public void Construct(
            ILogService log,
            IMessageService messageService,
            IAssetProvider assetProvider)
        {
            _logService = log;
            _messageService = messageService;
            _assetProvider = assetProvider;
        }

        public void Initialize(NetworkRunner runner)
        {
            _runner = runner;
        }

        public void Shutdown()
        {
            _runner = null;
            _currentScene = null;
            _isBusy = false;
        }

        public bool IsBusy => _isBusy;

        public Scene MainRunnerScene => SceneManager.GetActiveScene();

        public bool IsRunnerScene(Scene scene) => true;

        public bool TryGetPhysicsScene2D(out PhysicsScene2D scene2D)
        {
            var mainScene = MainRunnerScene;
            if (mainScene.IsValid())
            {
                scene2D = mainScene.GetPhysicsScene2D();
                return true;
            }
            scene2D = default;
            return false;
        }

        public bool TryGetPhysicsScene3D(out PhysicsScene scene3D)
        {
            var mainScene = MainRunnerScene;
            if (mainScene.IsValid())
            {
                scene3D = mainScene.GetPhysicsScene();
                return true;
            }
            scene3D = default;
            return false;
        }

        public void MakeDontDestroyOnLoad(GameObject obj)
        {
            Object.DontDestroyOnLoad(obj);
        }

        public bool MoveGameObjectToScene(GameObject gameObject, SceneRef sceneRef)
        {
            if (sceneRef == default)
                return true;

            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && GetSceneRef(scene.path) == sceneRef)
                {
                    SceneManager.MoveGameObjectToScene(gameObject, scene);
                    return true;
                }
            }

            return false;
        }

        public NetworkSceneAsyncOp LoadScene(SceneRef sceneRef, NetworkLoadSceneParameters parameters)
        {
            _logService.Log($"[SceneLoader] LoadScene called with sceneRef: {sceneRef}, parameters: {parameters}");
            _isBusy = true;

            var sceneName = GetSceneNameFromRef(sceneRef);
            if (string.IsNullOrEmpty(sceneName))
            {
                _isBusy = false;
                return NetworkSceneAsyncOp.FromError(sceneRef, new System.Exception($"Failed to get scene name from ref: {sceneRef}"));
            }

            var task = LoadSceneAsync(_runner, sceneName, parameters.LoadSceneMode);
            return NetworkSceneAsyncOp.FromTask(sceneRef, task);
        }

        public NetworkSceneAsyncOp UnloadScene(SceneRef sceneRef)
        {
            _logService.Log($"[SceneLoader] UnloadScene called with sceneRef: {sceneRef}");
            _isBusy = true;

            var sceneName = GetSceneNameFromRef(sceneRef);
            if (string.IsNullOrEmpty(sceneName))
            {
                _isBusy = false;
                return NetworkSceneAsyncOp.FromError(sceneRef, new System.Exception($"Failed to get scene name from ref: {sceneRef}"));
            }

            var task = UnloadSceneAsync(_runner, sceneName);
            return NetworkSceneAsyncOp.FromTask(sceneRef, task);
        }

        public SceneRef GetSceneRef(GameObject gameObject)
        {
            var scene = gameObject.scene;
            return GetSceneRef(scene.path);
        }

        public SceneRef GetSceneRef(string sceneNameOrPath)
        {
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneNameOrPath);
            if (buildIndex >= 0)
            {
                return SceneRef.FromIndex(buildIndex);
            }
            return SceneRef.None;
        }

        public bool OnSceneInfoChanged(NetworkSceneInfo sceneInfo, NetworkSceneInfoChangeSource changeSource)
        {
            return false;
        }

        public async UniTask Load(string nextScene)
        {
            _logService.Log($"Start loading scene: {nextScene}");

            try
            {
                var sceneHandle = await _assetProvider.LoadScene(nextScene);
                if (sceneHandle == null)
                {
                    _logService.LogError($"Failed to load scene: {nextScene}");
                    return;
                }

                _currentScene = nextScene;
                _messageService.Publish(new SceneLoadedMessage(nextScene));
                _logService.Log($"Completed loading scene: {nextScene}");
            }
            catch (System.Exception e)
            {
                _logService.LogError($"Error loading scene {nextScene}: {e.Message}");
            }
        }

        private async Task<NetworkSceneInfo> LoadSceneAsync(NetworkRunner runner, string sceneName, LoadSceneMode mode)
        {
            try
            {
                var sceneHandle = await _assetProvider.LoadScene(sceneName);
                if (sceneHandle == null)
                {
                    return new NetworkSceneInfo();
                }

                var scene = sceneHandle.Scene;
                var sceneInfo = new NetworkSceneInfo();
                sceneInfo.AddSceneRef(SceneRef.FromIndex(scene.buildIndex), mode);
                
                _currentScene = sceneName;
                _messageService.Publish(new SceneLoadedMessage(sceneName));
                _isBusy = false;
                
                return sceneInfo;
            }
            catch (System.Exception e)
            {
                _logService.LogError($"[SceneLoader] Error loading scene {sceneName}: {e.Message}");
                _isBusy = false;
                return new NetworkSceneInfo();
            }
        }

        private async Task UnloadSceneAsync(NetworkRunner runner, string sceneName)
        {
            _logService.Log($"[SceneLoader] Unloading scene {sceneName}");

            try
            {
                await _assetProvider.UnloadScene(sceneName);
                if (_currentScene == sceneName)
                {
                    _currentScene = null;
                }
                _isBusy = false;
            }
            catch (System.Exception e)
            {
                _logService.LogError($"[SceneLoader] Error unloading scene {sceneName}: {e.Message}");
                _isBusy = false;
            }
        }

        private string GetSceneNameFromRef(SceneRef sceneRef)
        {
            if (sceneRef.IsIndex)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(sceneRef.AsIndex);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    return System.IO.Path.GetFileNameWithoutExtension(scenePath);
                }
            }
            
            if (!string.IsNullOrEmpty(_currentScene))
            {
                return _currentScene;
            }
            
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                var fallbackName = activeScene.name;
                return fallbackName;
            }
            
            return null;
        }
    }
}