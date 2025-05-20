using Cysharp.Threading.Tasks;

namespace CodeBase.Infrastructure.StateMachine
{
    public interface IExitableState
    {
        UniTask Exit();
    }
}