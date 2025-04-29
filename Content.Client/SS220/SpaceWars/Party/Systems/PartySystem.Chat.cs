using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem
{
    public void ChatInitialize()
    {
        SubscribeNetworkEvent<ReceivePartyChatMessage>(OnReceiveChatMessage);
    }

    private void OnReceiveChatMessage(ReceivePartyChatMessage message)
    {
        _partyManager.ReceiveChatMessage(message.Message);
    }
}
