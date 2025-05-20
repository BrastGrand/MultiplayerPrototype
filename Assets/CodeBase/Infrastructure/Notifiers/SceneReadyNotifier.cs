using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using CodeBase.Services.PlayerSpawnerService;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay
{
    public class SceneReadyNotifier : MonoBehaviour
    {
        private IGameplayReadyNotifier _readyNotifier;
        private GameStateMachine _stateMachine;
        private GameLoopStateFactory _loopStateFactory;
        private IMessageService _messageService;
        private ISpawnPointsProvider _spawnPointsProvider;

        [Inject]
        private void Construct(
            IGameplayReadyNotifier readyNotifier,
            GameStateMachine stateMachine,
            GameLoopStateFactory loopStateFactory,
            IMessageService messageService,
            ISpawnPointsProvider spawnPointsProvider)
        {
            _readyNotifier = readyNotifier;
            _stateMachine = stateMachine;
            _loopStateFactory = loopStateFactory;
            _messageService = messageService;
            _spawnPointsProvider = spawnPointsProvider;
        }

        private async void Start()
        {
            await UniTask.DelayFrame(1);

            _stateMachine.RegisterFactory(_loopStateFactory.Create);
            _messageService.Publish(new SpawnPointsReadyMessage(_spawnPointsProvider));
            _readyNotifier.NotifyReady();
        }
    }
}