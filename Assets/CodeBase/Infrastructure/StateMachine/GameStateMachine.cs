using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameStateMachine : IStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _registeredStates = new Dictionary<Type, IExitableState>();
        private readonly Dictionary<Type, Func<IExitableState>> _stateFactories = new Dictionary<Type, Func<IExitableState>>();
        private IExitableState _currentState;

        public async UniTask Enter<TState>() where TState : class, IState
        {
            TState newState = await ChangeState<TState>();
            await newState.Enter();
        }

        public void RegisterState<TState>(TState state) where TState : IExitableState => _registeredStates.Add(typeof(TState), state);

        public void RegisterFactory<TState>(Func<TState> factory) where TState : IExitableState =>
            _stateFactories[typeof(TState)] = () => factory();

        private TState GetOrCreateState<TState>() where TState : class, IExitableState
        {
            var type = typeof(TState);

            if (_registeredStates.TryGetValue(type, out var instance))
                return instance as TState;

            if (_stateFactories.TryGetValue(type, out var factory))
            {
                var created = factory();
                _registeredStates[type] = created;
                return created as TState;
            }

            throw new InvalidOperationException($"State of type {type.Name} is not registered and has no factory.");
        }

        private async UniTask<TState> ChangeState<TState>() where TState : class, IExitableState
        {
            if (_currentState != null)
                await _currentState.Exit();

            var newState = GetOrCreateState<TState>();
            _currentState = newState;

            return newState;
        }
    }
}