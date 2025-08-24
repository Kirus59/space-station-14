
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    private Dictionary<uint, PartyInvite> _invites = new();
    private HashSet<ICommonSession> _doesntReceiveInvites = new();

    public void InviteInitialize()
    {
        SubscribeNetMessage<InviteInPartyRequestMessage>(OnInviteInPartyRequest);
        SubscribeNetMessage<AcceptInviteMessage>(OnAcceptInvite);
        SubscribeNetMessage<DenyInviteMessage>(OnDenyInvite);
        SubscribeNetMessage<DeleteInviteMessage>(OnDeleteInvite);
        SubscribeNetMessage<SetReceiveInvitesStatusMessage>(OnSetReceiveInvitesStatus);
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

    private void OnSetReceiveInvitesStatus(SetReceiveInvitesStatusMessage message, ICommonSession sender)
    {
        if (message.ReceiveInvites)
            _doesntReceiveInvites.Remove(sender);
        else
            _doesntReceiveInvites.Add(sender);
    }

    public void AcceptInvite(uint inviteId, ICommonSession target)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Target != target.UserId)
            return;

        var party = GetPartyByHost(invite.Sender);
        if (party == null)
            return;

        invite.Status = InviteStatus.Accepted;
        DirtyInvite(invite);

        if (!TryAddUserToParty(invite.Target, party, out var reason, PartyMemberRole.Member, force: true))
            Sawmill.Warning($"Failed to accept party invite with id: \"{inviteId}\"; by reason: \"{reason}\"");

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
    }

    public void DeleteInvite(uint inviteId, ICommonSession session)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Sender != session.UserId)
            return;

        DeleteInvite(invite);
    }

    public void DeleteInvite(PartyInvite invite)
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
        if (_doesntReceiveInvites.Contains(target))
        {
            failReason = Loc.GetString("partymanager-invite-failreason-user-doesnt-receive-invites", ("user", target.Name));
            return false;
        }

        var invitesLimit = _cfg.GetCVar(CCVars220.PartyInvitesLimit);
        if (GetSendedInvites(sender.UserId).Count() > invitesLimit)
        {
            failReason = Loc.GetString("partymanager-invite-failreason-reached-invited-limit");
            return false;
        }

        if (TryGetInvite(sender.UserId, target.UserId, out _))
        {
            failReason = Loc.GetString("partymanager-invite-failreason-user-already-invited", ("user", target.Name));
            return false;
        }

        var invite = CreateNewInvite(sender, target);
        invite.Status = InviteStatus.Sended;
        var incomingState = invite.GetIncomingInviteState();
        SendNetMessage(new InviteReceivedMessage(incomingState), target);

        var sendedState = invite.GetSendedInviteState();
        SendNetMessage(new CreatedNewInviteMessage(sendedState), sender);

        return true;
    }

    public void SendInvite(ICommonSession sender, ICommonSession target)
    {
        if (!TrySendInvite(sender, target, out var reason))
            throw new Exception($"Failed to send invite from {sender} to {target} by reason: {reason}");
    }

    private bool TryGetInvite(NetUserId sender, NetUserId target, [NotNullWhen(true)] out PartyInvite? invite)
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

    private IEnumerable<PartyInvite> GetSendedInvites(NetUserId sender)
    {
        return _invites.Values.Where(i => i.Sender == sender);
    }

    private PartyInvite CreateNewInvite(ICommonSession sender, ICommonSession target, InviteStatus status = InviteStatus.None)
    {
        if (TryGetInvite(sender.UserId, target.UserId, out _))
            throw new Exception($"Invite from {sender} to {target} is already exist!");

        var id = GenerateInviteId();
        var invite = new PartyInvite(id, sender, target, status);
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
        var sendedInvites = _invites.Values.Where(i => i.Sender == session).Select(i => i.GetSendedInviteState()).ToList();
        var incomingInvites = _invites.Values.Where(i => i.Target == session).Select(i => i.GetIncomingInviteState()).ToList();
        UpdateInvitesInfo(sendedInvites, incomingInvites, session);
    }

    public void UpdateInvitesInfo(List<SendedInviteState> sendedInvites, List<IncomingInviteState> incomingInvites, ICommonSession session)
    {
        var msg = new UpdateInvitesInfoMessage(sendedInvites, incomingInvites);
        SendNetMessage(msg, session);
    }

    public void DirtyInvite(PartyInvite invite)
    {
        var sendedState = invite.GetSendedInviteState();
        UpdateSendedInvite(sendedState, invite.Target);

        var incomingState = invite.GetIncomingInviteState();
        UpdateIncomingInvite(incomingState, invite.Target);
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
