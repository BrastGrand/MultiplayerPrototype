using CodeBase.Infrastructure.StateMachine;
using CodeBase.Services.MessageService;
using CodeBase.Services.MessageService.Messages;

namespace CodeBase.Services.DisconnectService
{
    public class HostDisconnectHandler : IHostDisconnectHandler
    {
        private readonly IMessageService _messageService;
        private readonly IStateMachine _stateMachine;

        public HostDisconnectHandler(IMessageService messageService, IStateMachine stateMachine)
        {
            _messageService = messageService;
            _stateMachine = stateMachine;

            _messageService.Subscribe<HostDisconnectMessage>(OnHostDisconnected);
        }

        private void OnHostDisconnected(HostDisconnectMessage message)
        {
            _messageService.Unsubscribe<HostDisconnectMessage>(OnHostDisconnected);
            _stateMachine.Enter<GameMenuState>();
        }
    }
}