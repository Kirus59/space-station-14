
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class PartyInvite(uint id,
    Party party,
    ICommonSession receiver,
    PartyInviteStatus status = PartyInviteStatus.None) : SharedPartyInvite(id, status)
{
    public readonly Party Party = party;
    public readonly ICommonSession Receiver = receiver;

    public InviteState GetState()
    {
        return new InviteState(Id, Party.Id, Receiver.UserId, Party.Host.Session.Name, Status);
    }
}
