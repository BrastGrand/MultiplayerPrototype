using System.Collections.Generic;
using CodeBase.Gameplay.Player.States;
using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Player.Animations
{
    public class PlayerAnimator : NetworkBehaviour, IPlayerAnimator
    {
        [SerializeField] private Animator _animator;

        private readonly Dictionary<PlayerStateType, string> _animationTriggers = new Dictionary<PlayerStateType, string>
        {
            { PlayerStateType.Idle, "Idle" },
            { PlayerStateType.Move, "Move" },
            { PlayerStateType.Dead, "Dead" },
        };
        
        public void Play(PlayerStateType state)
        {
            if (_animator == null) 
            {
                return;
            }
            
            _animationTriggers.TryGetValue(state, out string triggerName);

            if (string.IsNullOrEmpty(triggerName)) 
            {
                return;
            }

            _animator.SetTrigger(triggerName);
        }
    }
}