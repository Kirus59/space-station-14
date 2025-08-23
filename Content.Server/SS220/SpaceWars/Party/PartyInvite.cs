
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class PartyInvite(uint id, ICommonSession sender, ICommonSession target, InviteStatus status = InviteStatus.None) : SharedPartyInvite(id, status)
{
    public readonly ICommonSession Sender = sender;
    public readonly ICommonSession Target = target;

    public IncomingInviteState GetIncomingInviteState()
    {
        return new IncomingInviteState(Id, Sender.Name, Status);
    }

    public SendedInviteState GetSendedInviteState()
    {
        return new SendedInviteState(Id, Target.Name, Status);
    }
}
