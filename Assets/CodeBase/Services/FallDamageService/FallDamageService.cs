using System.Collections.Generic;
using System.Linq;
using CodeBase.Services.Message;
using Fusion;
using UnityEngine;

namespace CodeBase.Services.FallDamageService
{
    public class FallDamageService
    {
        private class FallEntry
        {
            public float FallStartY;
            public bool IsFalling;
            public NetworkCharacterController NetworkController;
            public CharacterController Controller;
        }

        private readonly Dictionary<NetworkObject, FallEntry> _entries = new Dictionary<NetworkObject, FallEntry>();
        private readonly float _fallThreshold;
        private readonly IMessageService _messageService;
        private float _fallDamage;

        public FallDamageService(float fallThreshold, IMessageService messageService)
        {
            _fallThreshold = fallThreshold;
            _messageService = messageService;
        }

        public void Track(NetworkObject player, float fallDamage)
        {
            _fallDamage = fallDamage;

            var networkController = player.GetComponent<NetworkCharacterController>();
            var characterController = player.GetComponent<CharacterController>();

            _entries[player] = new FallEntry
            {
                FallStartY = player.transform.position.y,
                IsFalling = false,
                NetworkController = networkController,
                Controller = characterController
            };
        }

        public void Tick()
        {
            foreach (var entry in _entries.ToList())
            {
                var player = entry.Key;
                var data = entry.Value;
                var positionY = player.transform.position.y;

                bool isGrounded = false;
                if (data.NetworkController != null)
                {
                    isGrounded = data.NetworkController.Grounded;
                }
                else if (data.Controller != null)
                {
                    isGrounded = data.Controller.isGrounded;
                }
                else
                {
                    continue;
                }

                if (!isGrounded)
                {
                    if (!data.IsFalling)
                    {
                        data.FallStartY = positionY;
                        data.IsFalling = true;
                        Debug.Log($"[FallDamageService] Player {player.Id} started falling from Y={data.FallStartY}");
                    }
                }
                else
                {
                    if (!data.IsFalling) continue;
                    float delta = data.FallStartY - positionY;

                    if (delta >= _fallThreshold)
                    {
                        if (player.HasStateAuthority)
                        {
                            _messageService.Publish(new PlayerTakeDamageMessage
                            {
                                Damage = _fallDamage,
                                PlayerObject = player
                            });

                            Debug.Log($"[FallDamageService] Fall damage sent for player {player.Id}. " +
                                      $"Damage: {_fallDamage}");
                        }
                    }

                    data.IsFalling = false;
                }
            }
        }
    }
}