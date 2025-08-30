
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    void ChatMessageToParty(string message, Party party, PartyChatMessageType messageType = PartyChatMessageType.None);

    void ChatMessageToUser(string message, ICommonSession session, PartyChatMessageType messageType = PartyChatMessageType.None);
}

public enum PartyChatMessageType
{
    None,
    Info,
    Alert
}
