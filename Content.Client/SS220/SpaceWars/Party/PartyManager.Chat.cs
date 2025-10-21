

using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<string>? ChatMessageReceived;

    public void ChatInitialize()
    {
        SubscribeNetMessage<MsgPartyChatMessage>(OnReceiveChatMessage);
    }

    private void OnReceiveChatMessage(MsgPartyChatMessage message)
    {
        ChatMessageReceived?.Invoke(message.Message);
    }
}
