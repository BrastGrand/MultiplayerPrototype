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
        }

        private void OnRunnerInitialized(NetworkRunner runner)
        {
            _runner = runner;
            SpawnMedkits();

            _runnerProvider.OnRunnerInitialized -= OnRunnerInitialized;
        }

        private void SpawnMedkits()
        {
            if (_medkitPrefab == null)
            {
                Debug.LogError("Medkit prefab is not assigned!");
                return;
            }

            foreach (var point in _spawnPoints)
            {
                if (point == null) continue;
                SpawnMedkit(point);
            }
        }

        private async void SpawnMedkit(Transform point)
        {
            if (_runner == null || !_runner.IsServer)
            {
                Debug.LogWarning("SpawnMedkit: Runner not ready or not server");
                return;
            }

            try
            {
                var result = await _runner.SpawnAsync(_medkitPrefab, point.position, Quaternion.identity);
                if (result == null)
                {
                    Debug.LogError($"Failed to spawn medkit at position {point.position}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error spawning medkit: {e.Message}");
            }
        }
    }
}