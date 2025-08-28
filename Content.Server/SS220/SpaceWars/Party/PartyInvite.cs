
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class PartyInvite(uint id,
    uint partyId,
    ICommonSession sender,
    ICommonSession target,
    PartyInviteStatus status = PartyInviteStatus.None) : SharedPartyInvite(id, partyId, status)
{
    public readonly ICommonSession Sender = sender;
    public readonly ICommonSession Target = target;

    public InviteState GetState()
    {
        return new InviteState(Id, Sender.UserId, Target.UserId, Status, SenderName: Sender.Name);
    }
}
