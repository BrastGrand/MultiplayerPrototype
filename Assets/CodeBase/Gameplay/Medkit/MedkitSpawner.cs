using CodeBase.Services.NetworkService;
using Fusion;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Medkit
{
    public class MedkitSpawner : MonoBehaviour
    {
        [SerializeField] private NetworkPrefabRef _medkitPrefab;
        [SerializeField] private Transform[] _spawnPoints;

        private NetworkRunnerProvider _runnerProvider;
        private NetworkRunner _runner;

        [Inject]
        public void Construct(NetworkRunnerProvider runnerProvider)
        {
            _runnerProvider = runnerProvider;

            if (runnerProvider.Runner != null)
            {
                _runner = runnerProvider.Runner;
                SpawnMedkits();
            }
            else
            {
                runnerProvider.OnRunnerInitialized += OnRunnerInitialized;
            }

            SpawnMedkits();
        }

        private void OnRunnerInitialized(NetworkRunner runner)
        {
            _runner = runner;
            SpawnMedkits();

            _runnerProvider.OnRunnerInitialized -= OnRunnerInitialized;
        }

        private void SpawnMedkits()
        {
            foreach (var point in _spawnPoints)
            {
                if (point == null) continue;
                SpawnMedkit(point);
            }
        }

        private void SpawnMedkit(Transform point)
        {
            if (_runner == null || !_runner.IsServer)
            {
                Debug.LogWarning("SpawnMedkit: Runner not ready or not server");
                return;
            }

            _runner.Spawn(_medkitPrefab, point.position, Quaternion.identity);
        }

    }
}