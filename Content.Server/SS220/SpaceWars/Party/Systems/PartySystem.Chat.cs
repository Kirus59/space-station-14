
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem
{
    public void SendPartyChatMessage(string message, ICommonSession session)
    {
        var ev = new ReceivePartyChatMessage(message);
        RaiseNetworkEvent(ev, session);
    }
}
