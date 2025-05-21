using System.Collections.Generic;
using System.Linq;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
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

            _entries[player] = new FallEntry
            {
                FallStartY = player.transform.position.y,
                IsFalling = false,
                Controller = player.GetComponent<CharacterController>()
            };
        }

        public void Tick()
        {
            foreach (var entry in _entries.ToList())
            {
                var player = entry.Key;
                var data = entry.Value;
                var positionY = player.transform.position.y;

                var controller = entry.Value.Controller;
                if (controller == null) continue;

                if (!controller.isGrounded)
                {
                    // Начало падения
                    if (!data.IsFalling)
                    {
                        data.FallStartY = positionY;
                        data.IsFalling = true;
                    }
                }
                else
                {
                    // Приземление
                    if (data.IsFalling)
                    {
                        float delta = data.FallStartY - positionY;

                        if (delta >= _fallThreshold)
                        {
                            _messageService.Publish(new PlayerTakeDamageMessage
                            {
                                Damage = _fallDamage,
                                PlayerObject = player
                            });
                        }

                        data.IsFalling = false;
                    }
                }
            }
        }
    }
}