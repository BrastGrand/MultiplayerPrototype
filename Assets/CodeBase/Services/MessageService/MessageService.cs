using System;
using System.Collections.Generic;

namespace CodeBase.Services.MessageService
{
    public class MessageService : IMessageService, IDisposable
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> handler) where T : IMessage
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;
        }

        public void Unsubscribe<T>(Action<T> handler) where T : IMessage
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Remove(existing, handler);
        }

        public void Publish<T>(T message) where T : IMessage
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var handlers))
                (handlers as Action<T>)?.Invoke(message);
        }

        public void Dispose() => _handlers.Clear();
    }
}