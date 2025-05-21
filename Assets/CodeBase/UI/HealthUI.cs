using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private Image _healthProgress;

        private IMessageService _messageService;
        private int _maxHp;

        public void Initialize(int maxHp, IMessageService messageService)
        {
            _maxHp = maxHp;
            _messageService = messageService;
            _messageService.Subscribe<HealthChangedMessage>(OnHealthChanged);

            ChangeHealth(_maxHp);
        }

        private void OnHealthChanged(HealthChangedMessage message)
        {
            ChangeHealth(message.CurrentHp);
        }

        private void ChangeHealth(float currentHp)
        {
            _healthProgress.fillAmount = currentHp / _maxHp;
        }

        public void Clear()
        {
            _messageService.Unsubscribe<HealthChangedMessage>(OnHealthChanged);
        }
    }
}