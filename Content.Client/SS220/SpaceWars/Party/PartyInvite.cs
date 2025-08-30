
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed class PartyInvite(PartyInviteState state) : SharedPartyInvite(state.Id, state.Status)
{
    public readonly uint PartyId = state.PartyId;
    public readonly NetUserId Receiver = state.Receiver;
    public readonly string SenderName = state.SenderName;

    public void HandleState(PartyInviteState state)
    {
        if (state.Id != Id)
            return;

        Status = state.Status;
    }
}
