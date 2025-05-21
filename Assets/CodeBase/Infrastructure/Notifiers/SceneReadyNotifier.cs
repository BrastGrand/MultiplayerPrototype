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
            Debug.Log("[SceneReadyNotifier] ReadyNotifier completed");

            await UniTask.WaitUntil(() => _spawnPointsProvider != null && _spawnPointsProvider.SpawnPoints.Count > 0);
            Debug.Log("[SceneReadyNotifier] Spawn points ready");

            _messageService.Publish<SpawnPointsReadyMessage>(new SpawnPointsReadyMessage(_spawnPointsProvider));
            Debug.Log("[SceneReadyNotifier] Published SpawnPointsReadyMessage");

            await _stateMachine.Enter<GameLoopState>();
            Debug.Log("[SceneReadyNotifier] Entered GameLoopState");

            _messageService.Publish<SceneReadyMessage>(new SceneReadyMessage());
            Debug.Log("[SceneReadyNotifier] Published SceneReadyMessage");
        }
    }
}