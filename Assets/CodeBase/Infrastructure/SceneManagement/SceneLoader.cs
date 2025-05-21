using CodeBase.Services.Log;
using CodeBase.Services.Message;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.SceneManagement
{
    public class SceneLoader : ISceneLoader
    {
        private readonly ILogService _logService;
        private readonly IMessageService _messageService;

        protected SceneLoader(ILogService log, IMessageService messageService)
        {
            _logService = log;
            _messageService = messageService;
        }


        public async UniTask Load(string nextScene)
        {
            _logService.Log($"Start loading scene: {nextScene}");

            var handler = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single); // Addressables.LoadSceneAsync(nextScene, LoadSceneMode.Additive, false);

            await handler.ToUniTask();
            //await handler.Result.ActivateAsync().ToUniTask();
            _logService.Log($"Completed loading scene: {nextScene}");

            _messageService.Publish(new SceneLoadedMessage(nextScene));
        }
    }
}