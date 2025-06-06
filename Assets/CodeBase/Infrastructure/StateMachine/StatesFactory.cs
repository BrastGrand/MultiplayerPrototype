using Zenject;

namespace CodeBase.Infrastructure.StateMachine
{
    public class StatesFactory
    {
        private readonly IInstantiator _instantiator;

        public StatesFactory(IInstantiator instantiator) => _instantiator = instantiator;
        public TState Create<TState>() where TState : IExitableState => _instantiator.Instantiate<TState>();
    }
}