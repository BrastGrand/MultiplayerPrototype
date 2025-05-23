using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface IAssetProvider
    {
        UniTask InitializeAsync();
        Task<TAsset> Load<TAsset>(string key) where TAsset : class;
        Task<TAsset> Load<TAsset>(AssetReference assetReference) where TAsset : class;
        UniTask WarmupAssetsByLabel(string label);
        UniTask ReleaseAssetsByLabel(string label);
        void Cleanup();
        Task<GameObject> Instantiate(string address);
        Task<GameObject> Instantiate(string address, Vector3 at);
        Task<GameObject> Instantiate(string address, Transform under);
        Task<GameObject> Instantiate(string address, Vector3 at, Transform under);
        Task<SceneInstance> LoadScene(string sceneName);
        Task UnloadScene(string sceneName);
    }
}