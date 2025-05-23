using CodeBase.Infrastructure.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CodeBase.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        private GameStateMachine _gameStateMachine;
        private StatesFactory _statesFactory;

        [Inject]
        private void Construct(GameStateMachine gameStateMachine, StatesFactory statesFactory)
        {
            _gameStateMachine = gameStateMachine;
            _statesFactory = statesFactory;
        }

        private void Start()
        {
            _gameStateMachine.RegisterState(_statesFactory.Create<GameBootstrapState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<GameMenuState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<GameLoadingState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<MenuLoadingState>());
            _gameStateMachine.RegisterState(_statesFactory.Create<GameLoopState>());

            _gameStateMachine.Enter<GameBootstrapState>().Forget();
            DontDestroyOnLoad(this);
        }
    }
}