using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInvite>? OnOutgoingInviteAdded;
    public event Action<PartyInvite>? OnOutgoingInviteRemoved;
    public event Action<PartyInvite>? OnIncomingInviteAdded;
    public event Action<PartyInvite>? OnIncomingInviteRemoved;

    private Dictionary<uint, PartyInvite> _outgoingInvites = new();
    private Dictionary<uint, PartyInvite> _incomingInvites = new();

    public void HandleInviteState(PartyInviteState state)
    {
        if (_outgoingInvites.TryGetValue(state.Id, out var outgoingInvite))
            HandleInviteState(state, outgoingInvite);

        if (_incomingInvites.TryGetValue(state.Id, out var incomingInvite))
            HandleInviteState(state, incomingInvite);
    }

    public void HandleInviteState(PartyInviteState state, PartyInvite invite)
    {
        if (state.Id != invite.Id)
            return;

        invite.Status = state.Status;
    }

    public void AddOutgoingInvite(PartyInvite invite)
    {
        if (_outgoingInvites.ContainsKey(invite.Id))
            return;

        _outgoingInvites.Add(invite.Id, invite);
        OnOutgoingInviteAdded?.Invoke(invite);
    }

    public void RemoveOutgoingInvite(PartyInvite invite)
    {
        _outgoingInvites.Remove(invite.Id);
        OnOutgoingInviteRemoved?.Invoke(invite);
    }

    public void AddIncomingInvite(PartyInvite invite)
    {
        if (_incomingInvites.ContainsKey(invite.Id))
            return;

        _incomingInvites.Add(invite.Id, invite);
        OnIncomingInviteAdded?.Invoke(invite);
    }

    public void RemoveIncomingInvite(PartyInvite invite)
    {
        _outgoingInvites.Remove(invite.Id);
        OnIncomingInviteRemoved?.Invoke(invite);
    }
}
