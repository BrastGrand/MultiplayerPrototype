using CodeBase.Services.InputService;
using CodeBase.Services.MessageService;
using Cysharp.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public class PhotonNetworkService : INetworkService
    {
        private readonly NetworkRunnerProvider _runnerProvider;
        private readonly IMessageService _messageService;
        private readonly INetworkCallbacksService _networkCallbacksService;
        private readonly INetworkInputService _inputService;

        public PhotonNetworkService(NetworkRunnerProvider runnerProvider,  IMessageService messageService, INetworkCallbacksService callbacksService, INetworkInputService inputService)
        {
            _runnerProvider = runnerProvider;
            _messageService = messageService;
            _networkCallbacksService = callbacksService;
            _inputService = inputService;
        }

        public async UniTask StartHost()
        {
            var runner = new GameObject("NetworkRunner").AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
            runner.AddComponent<NetworkSceneManagerDefault>();
            runner.AddCallbacks(new NetworkCallbacks(_messageService, _inputService));
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
            runner.ProvideInput = true;
            runner.AddCallbacks(new NetworkCallbacks(_messageService, _inputService));
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