using System;
using System.Threading.Tasks;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using CodeBase.Services.PlayerSpawnerService;
using Cysharp.Threading.Tasks;
using Fusion;
using Random = UnityEngine.Random;

namespace CodeBase.Gameplay.Player
{
    public class RespawnService : IRespawnService
    {
        private readonly IMessageService _messageService;

        private readonly TaskCompletionSource<ISpawnPointsProvider> _spawnPointsTaskCompletionSource = new TaskCompletionSource<ISpawnPointsProvider>();

        public RespawnService(IMessageService messageService)
        {
            _messageService = messageService;

            _messageService.Subscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
            _messageService.Subscribe<PlayerDiedMessage>(OnPlayerDied);
        }

        private void OnSpawnPointsReady(SpawnPointsReadyMessage message)
        {
            if (!_spawnPointsTaskCompletionSource.Task.IsCompleted)
                _spawnPointsTaskCompletionSource.SetResult(message.Provider);
        }

        private void OnPlayerDied(PlayerDiedMessage message)
        {
            Respawn(message.PlayerObject);
        }

        public async void Respawn(NetworkObject player)
        {
            var provider = await _spawnPointsTaskCompletionSource.Task;

            await UniTask.Delay(1000); //задержка перед респавном, нужно вынести

            var point = provider.SpawnPoints[Random.Range(0, provider.SpawnPoints.Count)];
            player.transform.SetPositionAndRotation(point.position, point.rotation);

            var networkPlayer = player.GetComponent<NetworkPlayer>();
            networkPlayer?.Respawn();
        }

        public void Dispose()
        {
            _messageService.Unsubscribe<SpawnPointsReadyMessage>(OnSpawnPointsReady);
            _messageService.Unsubscribe<PlayerDiedMessage>(OnPlayerDied);
        }
    }
}