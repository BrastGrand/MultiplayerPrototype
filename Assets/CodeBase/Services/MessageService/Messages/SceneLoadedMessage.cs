namespace CodeBase.Services.MessageService.Messages
{
    public class SceneLoadedMessage : IMessage
    {
        public string SceneName { get; }
        public SceneLoadedMessage(string sceneName) => SceneName = sceneName;
    }
}