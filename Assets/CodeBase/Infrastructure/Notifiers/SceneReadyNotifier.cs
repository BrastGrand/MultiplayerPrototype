using CodeBase.Services.Log;
using CodeBase.Services.Message;
using CodeBase.Services.PlayerSpawnerService;
using CodeBase.Services.NetworkService;
using CodeBase.Gameplay.Player;
using CodeBase.Services.InputService;
using CodeBase.Infrastructure.AssetManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay
{
    public class SceneReadyNotifier : MonoBehaviour
    {
        private IGameplayReadyNotifier _readyNotifier;
        private IMessageService _messageService;
        private ISpawnPointsProvider _spawnPointsProvider;
        private NetworkRunnerProvider _runnerProvider;
        private ILogService _log;
        private INetworkInputService _inputService;
        private IAssetProvider _assetProvider;

        [Inject]
        private void Construct(
            IGameplayReadyNotifier readyNotifier,
            IMessageService messageService,
            ISpawnPointsProvider spawnPointsProvider,
            NetworkRunnerProvider runnerProvider,
            ILogService log,
            INetworkInputService inputService,
            IAssetProvider assetProvider)
        {
            _readyNotifier = readyNotifier;
            _messageService = messageService;
            _spawnPointsProvider = spawnPointsProvider;
            _runnerProvider = runnerProvider;
            _log = log;
            _inputService = inputService;
            _assetProvider = assetProvider;

            // КРИТИЧЕСКИ ВАЖНО: Инициализируем статические сервисы для NetworkPlayer СРАЗУ
            // Это позволит игрокам на клиентах автоматически инициализироваться при спавне
            NetworkPlayer.InitializeStaticServices(_inputService, _messageService, _assetProvider);
            _log?.Log("[SceneReadyNotifier] NetworkPlayer static services initialized in constructor");
        }

        private async void Start()
        {
            try
            {
                _log?.Log("[SceneReadyNotifier] Starting scene initialization");

                // Проверяем, что все зависимости инжектированы
                if (_runnerProvider == null || _spawnPointsProvider == null || _messageService == null || _readyNotifier == null)
                {
                    _log?.LogError("[SceneReadyNotifier] Some dependencies are not injected!");
                    return;
                }

                await UniTask.WaitUntil(() => _runnerProvider?.Runner != null && _runnerProvider.Runner.IsRunning);

                // Ждем, пока точки спавна будут готовы - добавляем тайм-аут
                var spawnPointsReady = false;
                var timeoutMs = 10000;
                var startTime = System.DateTime.Now;
                
                while (!spawnPointsReady && (System.DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    spawnPointsReady = _spawnPointsProvider is { SpawnPoints: { Count: > 0 } };
                    
                    if (!spawnPointsReady)
                    {
                        await UniTask.Delay(500);
                    }
                }

                if (!spawnPointsReady)
                {
                    _log?.LogError($"[SceneReadyNotifier] Timeout waiting for spawn points! Provider={_spawnPointsProvider != null}, SpawnPoints={_spawnPointsProvider?.SpawnPoints?.Count ?? -1}");
                }
                else
                {
                    _log?.Log("[SceneReadyNotifier] Spawn points ready");
                }

                _messageService.Publish(new SpawnPointsReadyMessage(_spawnPointsProvider));

                // Уведомляем о готовности игрового процесса
                _readyNotifier.NotifyReady();
                _messageService.Publish(new NetworkRunnerReadyMessage(_runnerProvider.Runner));
                _messageService.Publish(new SceneReadyMessage());
                _log?.Log("[SceneReadyNotifier] Scene fully ready");
            }
            catch (System.Exception e)
            {
                _log?.LogError($"[SceneReadyNotifier] Error during scene initialization: {e.Message}\n{e.StackTrace}");
            }
        }
    }
}