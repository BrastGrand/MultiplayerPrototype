using System;
using System.Collections.Generic;
using CodeBase.Services.InputService;
using CodeBase.Gameplay.Player.States;

namespace CodeBase.Gameplay.Player
{
    public class PlayerStateMachine
    {
        private readonly Dictionary<PlayerStateType, IPlayerState> _states;
        private IPlayerState _currentState;
        private PlayerStateType _currentStateType;

        public event Action<PlayerStateType> OnStateChanged;

        public PlayerStateMachine(Dictionary<PlayerStateType, IPlayerState> states)
        {
            _states = states;
        }

        public void Enter(PlayerStateType stateType)
        {
            if (_states.TryGetValue(stateType, out var state))
            {
                if (_currentState != null)
                {
                    _currentState.Exit();
                }

                _currentState = state;
                _currentStateType = stateType;
                
                _currentState.Enter();
                OnStateChanged?.Invoke(stateType);
            }
        }

        public void Tick(NetworkInputData input)
        {
            if (_currentState == null)
            {
                return;
            }

            PlayerStateType nextStateType = _currentState.Tick(input);
            
            if (nextStateType != _currentStateType)
            {
                Enter(nextStateType);
            }
        }
    }
}