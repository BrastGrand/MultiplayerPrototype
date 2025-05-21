using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface IAssetProvider
    {
        UniTask InitializeAsync();
        Task<TAsset> Load<TAsset>(string key) where TAsset : class;
        UniTask WarmupAssetsByLabel(string label);
        UniTask ReleaseAssetsByLabel(string label);
        void Cleanup();
    }
}