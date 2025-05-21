using System;
using System.Collections.Generic;
using CodeBase.Services.InputService;
using CodeBase.Gameplay.Player.States;
using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    public class PlayerStateMachine
    {
        private readonly Dictionary<PlayerStateType, States.IPlayerState> _states;
        private States.IPlayerState _currentState;
        private PlayerStateType _currentStateType;

        public event Action<PlayerStateType> OnStateChanged;

        public PlayerStateMachine(Dictionary<PlayerStateType, States.IPlayerState> states)
        {
            _states = states;
        }

        public void Enter(PlayerStateType stateType)
        {
            if (_states.TryGetValue(stateType, out States.IPlayerState state))
            {
                if (_currentState != null)
                {
                    Debug.Log($"Exiting state {_currentStateType}");
                    _currentState.Exit();
                }

                _currentState = state;
                _currentStateType = stateType;
                
                Debug.Log($"Entering state {stateType}");
                _currentState.Enter();
                OnStateChanged?.Invoke(stateType);
            }
            else
            {
                Debug.LogError($"State {stateType} not found");
            }
        }

        public void Tick(NetworkInputData input)
        {
            if (_currentState == null)
            {
                Debug.LogError("Current state is null");
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