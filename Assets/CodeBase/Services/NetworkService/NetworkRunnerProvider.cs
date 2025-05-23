using System;
using Fusion;
using UnityEngine;

namespace CodeBase.Services.NetworkService
{
    public class NetworkRunnerProvider : IDisposable
    {
        private NetworkRunner _runner;
        private bool _isDisposed;
        private bool _isInitializing;

        public event Action<NetworkRunner> OnRunnerInitialized;

        public NetworkRunner Runner => _runner;

        public void Initialize(NetworkRunner runner)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_isInitializing)
            {
                return;
            }

            if (_runner != null)
            {
                Dispose();
            }

            try
            {
                _isInitializing = true;
                _runner = runner;

                if (_runner == null)
                {
                    Debug.LogError("[NetworkRunnerProvider] Failed to initialize: runner is null");
                    return;
                }

                Debug.Log($"[NetworkRunnerProvider] Runner initialized: {runner.name}, " +
                          $"IsServer: {runner.IsServer}, " +
                          $"IsClient: {runner.IsClient}, " +
                          $"IsRunning: {runner.IsRunning}, " +
                          $"Scene: {runner.SceneManager?.MainRunnerScene.name ?? "None"}");

                OnRunnerInitialized?.Invoke(_runner);
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkRunnerProvider] Failed to initialize runner: {e}");
                _runner = null;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_runner != null)
            {
                Debug.Log($"[NetworkRunnerProvider] Disposing runner: {_runner.name}, " +
                          $"IsServer: {_runner.IsServer}, " +
                          $"IsClient: {_runner.IsClient}, " +
                          $"IsRunning: {_runner.IsRunning}, " +
                          $"Scene: {_runner.SceneManager?.MainRunnerScene.name ?? "None"}");

                _runner = null;
            }

            _isDisposed = true;
        }
    }
}