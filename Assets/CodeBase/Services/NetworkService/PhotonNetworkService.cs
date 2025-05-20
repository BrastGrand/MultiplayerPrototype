using CodeBase.Services.MessageService;
using Cysharp.Threading.Tasks;
using Fusion;
using FusionDemo;
using Unity.VisualScripting;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkService : INetworkService
    {
        private readonly NetworkRunnerProvider _runnerProvider;
        private readonly IMessageService _messageService;
        private readonly INetworkCallbacksService _networkCallbacksService;

        public PhotonNetworkService(NetworkRunnerProvider runnerProvider,  IMessageService messageService, INetworkCallbacksService callbacksService)
        {
            _runnerProvider = runnerProvider;
            _messageService = messageService;
            _networkCallbacksService = callbacksService;
        }

        public async UniTask StartHost()
        {
            var runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddComponent<NetworkSceneManagerDefault>();
            runner.AddComponent<DemoInputPooling>();
            runner.AddCallbacks(new NetworkCallbacks(_messageService));
            _runnerProvider.Initialize(runner);

            await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = "TestRoom",
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });

            _networkCallbacksService.RegisterPlayer(runner.LocalPlayer);
        }

        public async UniTask StartClient()
        {
            var runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
            runner.AddCallbacks(new NetworkCallbacks(_messageService));
            _runnerProvider.Initialize(runner);

            await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = "TestRoom",
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });
        }

        public void Disconnect()
        {
            _runnerProvider
                .Runner
                .Shutdown()
                .Dispose();
        }
    }
}