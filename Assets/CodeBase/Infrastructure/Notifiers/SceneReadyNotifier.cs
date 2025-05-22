using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.Message;
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
        private IMessageService _messageService;
        private ISpawnPointsProvider _spawnPointsProvider;

        [Inject]
        private void Construct(
            IGameplayReadyNotifier readyNotifier,
            GameStateMachine stateMachine,
            IMessageService messageService,
            ISpawnPointsProvider spawnPointsProvider)
        {
            _readyNotifier = readyNotifier;
            _stateMachine = stateMachine;
            _messageService = messageService;
            _spawnPointsProvider = spawnPointsProvider;
        }

        private async void Start()
        {
            await _readyNotifier.WaitUntilReady();
            await UniTask.WaitUntil(() => _spawnPointsProvider != null && _spawnPointsProvider.SpawnPoints.Count > 0);

            _messageService.Publish(new SpawnPointsReadyMessage(_spawnPointsProvider));
            await _stateMachine.Enter<GameLoopState>();

            _messageService.Publish(new SceneReadyMessage());
        }
    }
}