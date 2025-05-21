using UnityEngine;

namespace CodeBase.Gameplay.Player
{
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "Game/PlayerSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [SerializeField] private int _maxHp = 100;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _fallDamageThreshold = 5f;
        [SerializeField] private float _fallDamage = 20f;

        public int MaxHp => _maxHp;
        public float MoveSpeed => _moveSpeed;
        public float FallDamageThreshold => _fallDamageThreshold;
        public float FallDamage => _fallDamage;
    }
}