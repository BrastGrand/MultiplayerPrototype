using System;

namespace CodeBase.Services.Message
{
    public interface IMessageService
    {
        void Subscribe<T>(Action<T> handler) where T : IMessage;
        void Unsubscribe<T>(Action<T> handler) where T : IMessage;
        void Publish<T>(T message) where T : IMessage;
    }
}