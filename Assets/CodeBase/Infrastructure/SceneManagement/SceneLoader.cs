using CodeBase.Services.LogService;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
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

            AsyncOperationHandle<SceneInstance> handler = Addressables.LoadSceneAsync(nextScene, LoadSceneMode.Single, false);

            await handler.ToUniTask();
            await handler.Result.ActivateAsync().ToUniTask();
            _logService.Log($"Completed loading scene: {nextScene}");

            _messageService.Publish(new SceneLoadedMessage(nextScene));
        }
    }
}