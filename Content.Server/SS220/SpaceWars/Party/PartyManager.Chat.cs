using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public void ChatMessageToParty(string message, ServerPartyData partyData, PartyChatMessageType messageType = PartyChatMessageType.None)
    {
        message = SanitizePartyChatMessage(message, messageType);
        foreach (var (user, _) in partyData.Members)
        {
            if (!_playerManager.TryGetSessionById(user, out var session))
                continue;

            _partySystem?.SendPartyChatMessage(message, session);
        }
    }

    public void ChatMessageToUser(string message, ICommonSession session, PartyChatMessageType messageType = PartyChatMessageType.None)
    {
        message = SanitizePartyChatMessage(message, messageType);
        _partySystem?.SendPartyChatMessage(message, session);
    }

    private string SanitizePartyChatMessage(string message, PartyChatMessageType messageType)
    {
        var msg = new FormattedMessage();

        Color? color = messageType switch
        {
            PartyChatMessageType.Info => Color.Yellow,
            PartyChatMessageType.Alert => Color.Red,
            _ => null,
        };

        if (color != null)
            msg.PushColor(color.Value);

        msg.AddText(message);
        msg.Pop();

        return msg.ToMarkup();
    }
}

public enum PartyChatMessageType
{
    None,
    Info,
    Alert
}
