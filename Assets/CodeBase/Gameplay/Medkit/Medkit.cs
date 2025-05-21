using CodeBase.Gameplay.Player;
using Fusion;
using UnityEngine;

namespace CodeBase.Gameplay.Medkit
{
    public class Medkit : NetworkBehaviour
    {
        [SerializeField] private int _healAmount = 25;

        private void OnTriggerEnter(Collider other)
        {
            if(!Runner.IsServer) return;

            if (other.TryGetComponent(out PlayerHealth health))
            {
                health.Heal(_healAmount);
                Runner.Despawn(Object);
            }
        }
    }
}