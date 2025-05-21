using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        private readonly Dictionary<string, AsyncOperationHandle> _assetRequests = new Dictionary<string, AsyncOperationHandle>();

        public async UniTask InitializeAsync() => await Addressables.InitializeAsync().ToUniTask();

        public async Task<TAsset> Load<TAsset>(string key) where TAsset : class
        {
            var validateAddress = Addressables.LoadResourceLocationsAsync(key);
            await validateAddress.Task;

            if (validateAddress.Status != AsyncOperationStatus.Succeeded || validateAddress.Result.Count == 0)
            {
                Debug.LogError($"Ресурсы по имени {key} не найдены или загрузка завершилась ошибкой.");
                Addressables.Release(validateAddress);
                return null;
            }

            var handle = Addressables.LoadAssetAsync<TAsset>(key);
            try
            {
                await handle.Task;
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при загрузке ассета {key}: {e}");
                Addressables.Release(handle);
                return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result as TAsset;
            }
            else
            {
                Debug.LogError($"Загрузка ассета {key} завершилась с ошибкой. Статус: {handle.Status}");
                Addressables.Release(handle);
                return null;
            }
        }

        public async UniTask<TAsset> Load<TAsset>(AssetReference assetReference) where TAsset : class => await Load<TAsset>(assetReference.AssetGUID);

        public async UniTask<List<string>> GetAssetsListByLabel<TAsset>(string label) => await GetAssetsListByLabel(label, typeof(TAsset));

        public async UniTask<List<string>> GetAssetsListByLabel(string label, Type type = null)
        {
            var operationHandle = Addressables.LoadResourceLocationsAsync(label, type);

            var locations = await operationHandle.ToUniTask();

            List<string> assetKeys = new List<string>(locations.Count);

            foreach (var location in locations)
                assetKeys.Add(location.PrimaryKey);

            Addressables.Release(operationHandle);
            return assetKeys;
        }

        public async Task<TAsset[]> LoadAll<TAsset>(List<string> keys) where TAsset : class
        {
            List<Task<TAsset>> tasks = new List<Task<TAsset>>(keys.Count);

            foreach (var key in keys)
                tasks.Add(Load<TAsset>(key));

            return await Task.WhenAll(tasks);
        }

        public async UniTask WarmupAssetsByLabel(string label)
        {
            var assetsList = await GetAssetsListByLabel(label);
            await LoadAll<object>(assetsList);
        }

        public async UniTask ReleaseAssetsByLabel(string label)
        {
            var assetsList = await GetAssetsListByLabel(label);

            foreach (var assetKey in assetsList)
                if (_assetRequests.TryGetValue(assetKey, out var handler))
                {
                    Addressables.Release(handler);
                    _assetRequests.Remove(assetKey);
                }
        }

        public void Cleanup()
        {
            foreach (var assetRequest in _assetRequests)
                Addressables.Release(assetRequest.Value);

            _assetRequests.Clear();
        }
    }
}