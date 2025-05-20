using Zenject;

namespace CodeBase.Infrastructure.StateMachine
{
    public class GameLoopStateFactory
    {
        private readonly DiContainer _container;

        public GameLoopStateFactory(DiContainer container)
        {
            _container = container;
        }

        public GameLoopState Create()
        {
            return _container.Instantiate<GameLoopState>();
        }
    }
}