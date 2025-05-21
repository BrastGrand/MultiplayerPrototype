using System;
using System.Collections.Generic;
using CodeBase.Services.InputService;

namespace CodeBase.Gameplay.Player
{
    public class PlayerStateMachine
    {
        private readonly Dictionary<PlayerStateType, IPlayerState> _states;
        private IPlayerState _currentState;
        private PlayerStateType _currentType;

        public event Action<PlayerStateType> OnStateChanged;

        public PlayerStateMachine(Dictionary<PlayerStateType, IPlayerState> states)
        {
            _states = states;
        }

        public void Enter(PlayerStateType type)
        {
            _currentState?.Exit();
            _currentType = type;
            _currentState = _states[_currentType];
            _currentState.Enter();

            OnStateChanged?.Invoke(type);
        }

        public void Tick(NetworkInputData input)
        {
            _currentState.Tick(input);

            if (_currentState.ShouldTransition(out var nextState))
            {
                Enter(nextState);
            }
        }
    }
}