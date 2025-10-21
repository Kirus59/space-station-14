// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    void ChatMessageToParty(string message, Party party, PartyChatMessageType messageType = PartyChatMessageType.Common);

    void ChatMessageToUser(string message, ICommonSession session, PartyChatMessageType messageType = PartyChatMessageType.Common);
}

public enum PartyChatMessageType
{
    Common,
    Info,
    Alert
}
