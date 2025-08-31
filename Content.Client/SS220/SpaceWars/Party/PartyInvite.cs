
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed class PartyInvite(PartyInviteState state) : SharedPartyInvite(state.Id, state.Status)
{
    public readonly uint PartyId = state.PartyId;
    public readonly string SenderName = state.SenderName;
    public readonly NetUserId Receiver = state.Receiver;
    public readonly string ReceiverName = state.ReceiverName;

    public void HandleState(PartyInviteState state)
    {
        DebugTools.AssertEqual(Id, state.Id);

        Status = state.Status;
    }
}
