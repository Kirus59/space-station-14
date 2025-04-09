using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInvite>? OnOutgoingInviteAdded;
    public event Action<PartyInvite>? OnOutgoingInviteRemoved;
    public event Action<PartyInvite>? OnIncomingInviteAdded;
    public event Action<PartyInvite>? OnIncomingInviteRemoved;

    public event Action<PartyInvite>? OnPartyInviteUpdated;

    public Dictionary<uint, PartyInvite> OutgoingInvites => _outgoingInvites;
    private Dictionary<uint, PartyInvite> _outgoingInvites = new();

    public Dictionary<uint, PartyInvite> IncomingInvites => _incomingInvites;
    private Dictionary<uint, PartyInvite> _incomingInvites = new();

    public void SendInvite(string username)
    {
        _partySystem?.SendInvite(username);
    }

    public void AcceptInvite(uint inviteId)
    {
        _partySystem?.AcceptInvite(inviteId);
    }

    public void DenyInvite(uint inviteId)
    {
        _partySystem?.DenyInvite(inviteId);
    }

    public void DeleteInvite(uint inviteId)
    {
        _partySystem?.DeleteInvite(inviteId);
    }

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
        CheckInviteStatus(invite);
        OnPartyInviteUpdated?.Invoke(invite);
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
        _incomingInvites.Remove(invite.Id);
        OnIncomingInviteRemoved?.Invoke(invite);
    }

    private void CheckInviteStatus(PartyInvite invite)
    {
        switch (invite.Status)
        {
            case InviteStatus.Deleted:
            case InviteStatus.Accepted:
            case InviteStatus.Denied:
                RemoveIncomingInvite(invite);
                RemoveOutgoingInvite(invite);
                break;
        }
    }
}
