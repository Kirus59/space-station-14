using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public void ChatMessageToParty(string message, Party partyData, PartyChatMessageType messageType = PartyChatMessageType.None)
    {
        message = SanitizePartyChatMessage(message, messageType);
        foreach (var (user, _) in partyData.Members)
        {
            if (!_playerManager.TryGetSessionById(user, out var session))
                continue;

            SendPartyChatMessage(message, session);
        }
    }

    public void ChatMessageToUser(string message, ICommonSession session, PartyChatMessageType messageType = PartyChatMessageType.None)
    {
        message = SanitizePartyChatMessage(message, messageType);
        SendPartyChatMessage(message, session);
    }

    public void SendPartyChatMessage(string message, ICommonSession session)
    {
        var msg = new PartyChatMessage(message);
        SendNetMessage(msg, session);
    }

    private static string SanitizePartyChatMessage(string message, PartyChatMessageType messageType)
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
