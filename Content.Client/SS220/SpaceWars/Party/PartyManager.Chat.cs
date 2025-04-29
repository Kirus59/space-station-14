

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<string>? OnChatMessageReceived;

    public void ReceiveChatMessage(string message)
    {
        OnChatMessageReceived?.Invoke(message);
    }
}
