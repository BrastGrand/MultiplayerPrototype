namespace CodeBase.Services.Message
{
    public class SceneLoadedMessage : IMessage
    {
        public string SceneName { get; }
        public SceneLoadedMessage(string sceneName) => SceneName = sceneName;
    }
}