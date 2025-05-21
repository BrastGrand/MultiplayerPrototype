namespace CodeBase.Services.Log
{
    public interface ILogService
    {
        void Log(string msg);
        void LogError(string msg);
        void LogWarning(string msg);
    }
}