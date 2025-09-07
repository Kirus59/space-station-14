
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed class PartyInvite(PartyInviteState state, PartyInviteType inviteType = PartyInviteType.None) : SharedPartyInvite(state.Id, state.Status)
{
    public readonly uint PartyId = state.PartyId;
    public readonly NetUserId Receiver = state.Receiver;

    public string SenderName = state.SenderName;
    public string ReceiverName = state.ReceiverName;
    public PartyInviteType InviteType = inviteType;

    public void HandleState(PartyInviteState state)
    {
        DebugTools.AssertEqual(Id, state.Id);

        Status = state.Status;
        SenderName = state.SenderName;
        ReceiverName = state.ReceiverName;
    }
}

public enum PartyInviteType
{
    None,
    External,
    Internal
}
