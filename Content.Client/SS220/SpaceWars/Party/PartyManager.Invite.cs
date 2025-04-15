using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<SendedPartyInvite>? OnSendedInviteAdded;
    public event Action<SendedPartyInvite>? OnSendedInviteRemoved;
    public event Action<SendedPartyInvite>? OnSendedInviteUpdated;

    public event Action<IncomingPartyInvite>? OnIncomingInviteAdded;
    public event Action<IncomingPartyInvite>? OnIncomingInviteRemoved;
    public event Action<IncomingPartyInvite>? OnIncomingInviteUpdated;

    public event Action<string>? OnSendInviteFail;

    public Dictionary<uint, SendedPartyInvite> SendedInvites => _sendedInvites;
    private Dictionary<uint, SendedPartyInvite> _sendedInvites = new();

    public Dictionary<uint, IncomingPartyInvite> IncomingInvites => _incomingInvites;
    private Dictionary<uint, IncomingPartyInvite> _incomingInvites = new();

    public void SendInvite(string username)
    {
        _partySystem?.SendInvite(username);
    }

    public void SendInviteFail(string reason)
    {
        OnSendInviteFail?.Invoke(reason);
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

    public void HandleInviteState(ClientPartyInviteState state)
    {
        if (_sendedInvites.TryGetValue(state.Id, out var sendedInvite))
            UpdateSendedInvite(state, sendedInvite);

        if (_incomingInvites.TryGetValue(state.Id, out var incomingInvite))
            UpdateIncomingInvite(state, incomingInvite);
    }

    private void UpdateSendedInvite(ClientPartyInviteState state, SendedPartyInvite invite)
    {
        if (state.Id != invite.Id)
            return;

        invite.Status = state.Status;
        CheckInviteStatus(invite);
        OnSendedInviteUpdated?.Invoke(invite);
    }

    private void UpdateIncomingInvite(ClientPartyInviteState state, IncomingPartyInvite invite)
    {
        if (state.Id != invite.Id)
            return;

        invite.Status = state.Status;
        CheckInviteStatus(invite);
        OnIncomingInviteUpdated?.Invoke(invite);
    }

    public void AddSendedInvite(SendedPartyInvite invite)
    {
        if (_sendedInvites.ContainsKey(invite.Id))
            return;

        _sendedInvites.Add(invite.Id, invite);
        OnSendedInviteAdded?.Invoke(invite);
    }

    public void RemoveSendedInvite(SendedPartyInvite invite)
    {
        _sendedInvites.Remove(invite.Id);
        OnSendedInviteRemoved?.Invoke(invite);
    }

    public void AddIncomingInvite(IncomingPartyInvite invite)
    {
        if (_incomingInvites.ContainsKey(invite.Id))
            return;

        _incomingInvites.Add(invite.Id, invite);
        OnIncomingInviteAdded?.Invoke(invite);
    }

    public void RemoveIncomingInvite(IncomingPartyInvite invite)
    {
        _incomingInvites.Remove(invite.Id);
        OnIncomingInviteRemoved?.Invoke(invite);
    }

    private void CheckInviteStatus(SharedPartyInvite invite)
    {
        switch (invite.Status)
        {
            case InviteStatus.Deleted:
            case InviteStatus.Accepted:
            case InviteStatus.Denied:
                if (invite is SendedPartyInvite sendedInvite)
                    RemoveSendedInvite(sendedInvite);
                else if (invite is IncomingPartyInvite incomingInvite)
                    RemoveIncomingInvite(incomingInvite);
                break;
        }
    }
}

public sealed class SendedPartyInvite : SharedPartyInvite
{
    public readonly string TargetName;

    public SendedPartyInvite(uint id, string targetName, InviteStatus status = InviteStatus.None) : base(id, status)
    {
        TargetName = targetName;
    }
}

public sealed class IncomingPartyInvite : SharedPartyInvite
{
    public readonly string SenderName;

    public IncomingPartyInvite(uint id, string senderName, InviteStatus status = InviteStatus.None) : base(id, status)
    {
        SenderName = senderName;
    }
}
