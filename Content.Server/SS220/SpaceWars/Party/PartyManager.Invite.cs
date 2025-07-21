
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    private Dictionary<uint, ServerPartyInvite> _invites = new();

    public void InviteInitialize()
    {
        SubscribeNetMessage<InviteInPartyRequestMessage>(OnInviteInPartyRequest);
        SubscribeNetMessage<AcceptInviteMessage>(OnAcceptInvite);
        SubscribeNetMessage<DenyInviteMessage>(OnDenyInvite);
        SubscribeNetMessage<DeleteInviteMessage>(OnDeleteInvite);
    }

    private void OnInviteInPartyRequest(InviteInPartyRequestMessage message, ICommonSession sender)
    {
        var success = TrySendInvite(sender, message.Username, out var failReason);

        var responce = new InviteInPartyResponceMessage
        {
            Success = success,
            Text = failReason ?? string.Empty,
        };

        SendNetMessage(responce, sender);
    }

    private void OnAcceptInvite(AcceptInviteMessage message, ICommonSession sender)
    {
        AcceptInvite(message.InviteId, sender);
    }

    private void OnDenyInvite(DenyInviteMessage message, ICommonSession sender)
    {
        DenyInvite(message.InviteId, sender);
    }

    private void OnDeleteInvite(DeleteInviteMessage message, ICommonSession sender)
    {
        DeleteInvite(message.InviteId, sender);
    }

    public void AcceptInvite(uint inviteId, ICommonSession target)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Target != target.UserId)
            return;

        var party = GetPartyByLeader(invite.Sender);
        if (party == null)
            return;

        invite.Status = InviteStatus.Accepted;
        DirtyInvite(invite);
        TryAddUserToParty(invite.Target, party, out _);
        _invites.Remove(invite.Id);
    }

    public void DenyInvite(uint inviteId, ICommonSession target)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Target != target.UserId)
            return;

        invite.Status = InviteStatus.Denied;
        DirtyInvite(invite);
        _invites.Remove(invite.Id);
    }

    public void DeleteInvite(uint inviteId, ICommonSession session)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Sender != session.UserId)
            return;

        DeleteInvite(invite);
    }

    public void DeleteInvite(ServerPartyInvite invite)
    {
        invite.Status = InviteStatus.Deleted;
        DirtyInvite(invite);
        _invites.Remove(invite.Id);
    }

    public bool TrySendInvite(ICommonSession sender, string username, [NotNullWhen(false)] out string? failReason)
    {
        if (!_playerManager.TryGetSessionByUsername(username, out var target))
        {
            failReason = Loc.GetString("partymanager-invite-failreason-user-not-found", ("user", username));
            return false;
        }

        return TrySendInvite(sender, target, out failReason);
    }

    public bool TrySendInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(false)] out string? failReason)
    {
        failReason = null;
        if (!TryCreateNewInvite(sender.UserId, target.UserId, out var invite))
        {
            failReason = Loc.GetString("partymanager-invite-failreason-user-already-invited", ("user", target.Name));
            return false;
        }

        SendInvite(invite);
        return true;
    }

    private bool TryGetInvite(NetUserId sender, NetUserId target, [NotNullWhen(true)] out ServerPartyInvite? invite)
    {
        invite = null;
        var founded = _invites.Values.Where(i => i.Sender == sender && i.Target == target);
        if (founded.Any())
        {
            invite = founded.First();
            return true;
        }

        return false;
    }

    private bool TryCreateNewInvite(NetUserId sender, NetUserId target, [NotNullWhen(true)] out ServerPartyInvite? invite, InviteStatus status = InviteStatus.None)
    {
        invite = null;
        if (TryGetInvite(sender, target, out _))
            return false;

        if (!_playerManager.TryGetSessionById(sender, out var senderSession) ||
            !_playerManager.TryGetSessionById(target, out var targetSession))
            return false;

        invite = CreateNewInvite(senderSession, targetSession, status);
        return true;
    }

    private ServerPartyInvite CreateNewInvite(ICommonSession sender, ICommonSession target, InviteStatus status = InviteStatus.None)
    {
        var id = GenerateInviteId();
        var invite = new ServerPartyInvite(id, sender.UserId, target.UserId, sender.Name, target.Name, status);
        _invites.Add(id, invite);
        return invite;
    }

    private uint GenerateInviteId()
    {
        uint id = 1;
        while (_invites.ContainsKey(id))
            id++;

        return id;
    }

    private void UpdateInvitesInfo(ICommonSession session)
    {
        var sendedInvites = _invites.Values.Where(i => i.Sender == session.UserId).Select(i => i.GetSendedInviteState()).ToList();
        var incomingInvites = _invites.Values.Where(i => i.Target == session.UserId).Select(i => i.GetIncomingInviteState()).ToList();
        UpdateInvitesInfo(sendedInvites, incomingInvites, session);
    }

    public void UpdateInvitesInfo(List<SendedInviteState> sendedInvites, List<IncomingInviteState> incomingInvites, ICommonSession session)
    {
        var msg = new UpdateInvitesInfoMessage(sendedInvites, incomingInvites);
        SendNetMessage(msg, session);
    }

    public void SendInvite(ServerPartyInvite invite)
    {
        invite.Status = InviteStatus.Sended;
        if (_playerManager.TryGetSessionById(invite.Target, out var target))
        {
            var state = invite.GetIncomingInviteState();
            var msg = new InviteReceivedMessage(state);
            SendNetMessage(msg, target);
        }

        if (_playerManager.TryGetSessionById(invite.Sender, out var sender))
        {
            var state = invite.GetSendedInviteState();
            var msg = new CreatedNewInviteMessage(state);
            SendNetMessage(msg, sender);
        }
    }

    public void DirtyInvite(ServerPartyInvite invite)
    {
        if (_playerManager.TryGetSessionById(invite.Sender, out var sender))
        {
            var state = invite.GetSendedInviteState();
            UpdateSendedInvite(state, sender);
        }

        if (_playerManager.TryGetSessionById(invite.Target, out var target))
        {
            var state = invite.GetIncomingInviteState();
            UpdateIncomingInvite(state, target);
        }
    }

    public void UpdateSendedInvite(SendedInviteState state, ICommonSession session)
    {
        var msg = new UpdateSendedInviteMessage(state);
        SendNetMessage(msg, session);
    }

    public void UpdateIncomingInvite(IncomingInviteState state, ICommonSession session)
    {
        var msg = new UpdateIncomingInviteMessage(state);
        SendNetMessage(msg, session);
    }
}

public sealed class ServerPartyInvite : SharedPartyInvite
{
    public readonly NetUserId Sender;
    public readonly NetUserId Target;

    public readonly string SenderName;
    public readonly string TargetName;

    public ServerPartyInvite(uint id,
        NetUserId sender,
        NetUserId target,
        string senderName,
        string targetName,
        InviteStatus status = InviteStatus.None) : base(id, status)
    {
        Sender = sender;
        Target = target;

        SenderName = senderName;
        TargetName = targetName;
    }

    public IncomingInviteState GetIncomingInviteState()
    {
        return new IncomingInviteState(Id, SenderName, Status);
    }

    public SendedInviteState GetSendedInviteState()
    {
        return new SendedInviteState(Id, TargetName, Status);
    }
}
