
namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<string>? OnChatMessageReceived;

    void ReceiveChatMessage(string message);
}
