

using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<string>? OnChatMessageReceived;

    public void ChatInitialize()
    {
        SubscribeNetMessage<PartyChatMessage>(OnReceiveChatMessage);
    }

    private void OnReceiveChatMessage(PartyChatMessage message)
    {
        OnChatMessageReceived?.Invoke(message.Message);
    }
}
