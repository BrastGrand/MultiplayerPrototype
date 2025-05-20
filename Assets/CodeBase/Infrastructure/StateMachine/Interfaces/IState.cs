using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.StateMachine
{
    public interface IState : IExitableState
    {
        UniTask Enter();
    }
}