
using Content.Shared.SS220.SpaceWars.Party;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<SendedPartyInvite>? OnSendedInviteAdded;
    public event Action<SendedPartyInvite>? OnSendedInviteRemoved;
    public event Action<SendedPartyInvite>? OnSendedInviteUpdated;

    public event Action<IncomingPartyInvite>? OnIncomingInviteAdded;
    public event Action<IncomingPartyInvite>? OnIncomingInviteRemoved;
    public event Action<IncomingPartyInvite>? OnIncomingInviteUpdated;

    public Dictionary<uint, SendedPartyInvite> SendedInvites => _sendedInvites;
    private Dictionary<uint, SendedPartyInvite> _sendedInvites = new();

    public Dictionary<uint, IncomingPartyInvite> IncomingInvites => _incomingInvites;
    private Dictionary<uint, IncomingPartyInvite> _incomingInvites = new();

    public void InviteInitialize()
    {
        SubscribeNetMessage<CreatedNewInviteMessage>(OnCreatedNewInvite);
        SubscribeNetMessage<InviteReceivedMessage>(OnInviteReceived);
        SubscribeNetMessage<UpdateSendedInviteMessage>(OnUpdateSendedInvite);
        SubscribeNetMessage<UpdateIncomingInviteMessage>(OnUpdateIncomingInvite);
        SubscribeNetMessage<UpdateInvitesInfoMessage>(OnUpdateInvitesInfo);
    }

    private void OnCreatedNewInvite(CreatedNewInviteMessage message)
    {
        var invite = new SendedPartyInvite(message.State);
        AddSendedInvite(invite);
    }

    private void OnInviteReceived(InviteReceivedMessage message)
    {
        var invite = new IncomingPartyInvite(message.State);
        AddIncomingInvite(invite);
    }

    private void OnUpdateSendedInvite(UpdateSendedInviteMessage message)
    {
        UpdateSendedInvite(message.State);
    }

    private void OnUpdateIncomingInvite(UpdateIncomingInviteMessage message)
    {
        UpdateIncomingInvite(message.State);
    }

    private void OnUpdateInvitesInfo(UpdateInvitesInfoMessage message)
    {
        UpdateSendedInvitesInfo(message.SendedInvites);
        UpdateIncomingInvitesInfo(message.IncomingInvites);
    }

    public async Task<InviteInPartyResponceMessage> SendInvite(string username)
    {
        var msg = new InviteInPartyRequestMessage(username);
        SendNetMessage(msg);

        var responce = await WaitResponce<InviteInPartyResponceMessage>();
        return responce;
    }

    public void AcceptInvite(uint inviteId)
    {
        var msg = new AcceptInviteMessage(inviteId);
        SendNetMessage(msg);
    }

    public void DenyInvite(uint inviteId)
    {
        var msg = new AcceptInviteMessage(inviteId);
        SendNetMessage(msg);
    }

    public void DeleteInvite(uint inviteId)
    {
        var msg = new DeleteInviteMessage(inviteId);
        SendNetMessage(msg);
    }

    public void UpdateSendedInvitesInfo(List<SendedInviteState> sendedInvites)
    {
        HashSet<uint> idToRemove = [.. _sendedInvites.Keys];

        foreach (var state in sendedInvites)
        {
            if (_sendedInvites.ContainsKey(state.Id))
            {
                idToRemove.Remove(state.Id);
                UpdateSendedInvite(state);
            }
            else
                AddSendedInvite(state);
        }

        foreach (var id in idToRemove)
            RemoveSendedInvite(id);
    }

    public void UpdateIncomingInvitesInfo(List<IncomingInviteState> incomingInvites)
    {
        HashSet<uint> idToRemove = [.. _incomingInvites.Keys];

        foreach (var state in incomingInvites)
        {
            if (_incomingInvites.ContainsKey(state.Id))
            {
                idToRemove.Remove(state.Id);
                UpdateIncomingInvite(state);
            }
            else
                AddIncomingInvite(state);
        }

        foreach (var id in idToRemove)
            RemoveIncomingInvite(id);
    }

    public void UpdateSendedInvite(SendedInviteState state)
    {
        if (!_sendedInvites.TryGetValue(state.Id, out var invite))
            return;

        if (state.Id != invite.Id)
            return;

        invite.Status = state.Status;
        CheckInviteStatus(invite);
        OnSendedInviteUpdated?.Invoke(invite);
    }

    public void UpdateIncomingInvite(IncomingInviteState state)
    {
        if (!_incomingInvites.TryGetValue(state.Id, out var invite))
            return;

        if (state.Id != invite.Id)
            return;

        invite.Status = state.Status;
        CheckInviteStatus(invite);
        OnIncomingInviteUpdated?.Invoke(invite);
    }

    public void AddSendedInvite(SendedInviteState state)
    {
        var invite = new SendedPartyInvite(state);
        AddSendedInvite(invite);
    }

    public void AddSendedInvite(SendedPartyInvite invite)
    {
        if (_sendedInvites.ContainsKey(invite.Id))
            return;

        _sendedInvites.Add(invite.Id, invite);
        OnSendedInviteAdded?.Invoke(invite);
    }

    public void RemoveSendedInvite(uint id)
    {
        if (!_sendedInvites.TryGetValue(id, out var invite))
            return;

        RemoveSendedInvite(invite);
    }

    public void RemoveSendedInvite(SendedPartyInvite invite)
    {
        _sendedInvites.Remove(invite.Id);
        OnSendedInviteRemoved?.Invoke(invite);
    }

    public void AddIncomingInvite(IncomingInviteState state)
    {
        var invite = new IncomingPartyInvite(state);
        AddIncomingInvite(invite);
    }

    public void AddIncomingInvite(IncomingPartyInvite invite)
    {
        if (_incomingInvites.ContainsKey(invite.Id))
            return;

        _incomingInvites.Add(invite.Id, invite);
        OnIncomingInviteAdded?.Invoke(invite);
    }

    public void RemoveIncomingInvite(uint id)
    {
        if (!_incomingInvites.TryGetValue(id, out var invite))
            return;

        RemoveIncomingInvite(invite);
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

    public SendedPartyInvite(SendedInviteState state) : this(state.Id, state.TargetName, state.Status) { }

    public SendedPartyInvite(uint id, string targetName, InviteStatus status = InviteStatus.None) : base(id, status)
    {
        TargetName = targetName;
    }
}

public sealed class IncomingPartyInvite : SharedPartyInvite
{
    public readonly string SenderName;

    public IncomingPartyInvite(IncomingInviteState state) : this(state.Id, state.SenderName, state.Status) { }

    public IncomingPartyInvite(uint id, string senderName, InviteStatus status = InviteStatus.None) : base(id, status)
    {
        SenderName = senderName;
    }
}
