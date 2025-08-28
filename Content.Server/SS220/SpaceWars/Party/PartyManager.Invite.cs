
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    private HashSet<PartyInvite> _invites = new();
    private HashSet<ICommonSession> _doesntReceiveInvites = new();

    private uint _nextInviteId = 0;

    public void InviteInitialize()
    {
        SubscribeNetMessage<InviteInPartyRequestMessage>(OnInviteInPartyRequest);
        SubscribeNetMessage<AcceptInviteMessage>(OnAcceptInvite);
        SubscribeNetMessage<DenyInviteMessage>(OnDenyInvite);
        SubscribeNetMessage<DeleteInviteRequestMessage>(OnDeleteInvite);
        SubscribeNetMessage<SetReceiveInvitesStatusMessage>(OnSetReceiveInvitesStatus);
    }

    private void OnInviteInPartyRequest(InviteInPartyRequestMessage message, ICommonSession sender)
    {
        var responce = new InviteInPartyResponceMessage
        {
            Success = success,
            Text = failReason ?? string.Empty,
        };

        var success = TrySendInvite(sender, message.Username, out var failReason);

        SendNetMessage(responce, sender);

        bool TryInvite(out string responceMessage)
        {
            if (!TryGetPartyByHost(sender, out var party))
            {
                responceMessage = Loc.GetString($"");
                return false;
            }

            if (!_playerManager.TryGetSessionByUsername(message.Username, out var target))
            {
                responceMessage = Loc.GetString($"");
                return false;
            }

            TryCreateAndSendInvite(sender, target);
        }
    }

    private void OnAcceptInvite(AcceptInviteMessage message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Target)
            return;

        AcceptInvite(invite);
    }

    private void OnDenyInvite(DenyInviteMessage message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Target)
            return;

        DenyInvite(invite);
    }

    private void OnDeleteInvite(DeleteInviteRequestMessage message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Sender)
            return;

        DeleteInvite(invite);
    }

    private void OnSetReceiveInvitesStatus(SetReceiveInvitesStatusMessage message, ICommonSession sender)
    {
        if (message.ReceiveInvites)
            _doesntReceiveInvites.Remove(sender);
        else
            _doesntReceiveInvites.Add(sender);
    }

    public void AcceptInvite(PartyInvite invite)
    {
        if (!TryGetPartyById(invite.Id, out var party))
            return;

        if (party.Host.Session != invite.Sender)
            return;

        SetInviteStatus(invite, PartyInviteStatus.Accepted, updates: false);

        var result = AddMember(party, invite.Target, PartyMemberRole.Member, force: true);
        DebugTools.Assert(result);

        DeleteInvite(invite);
    }

    public void DenyInvite(PartyInvite invite)
    {
        SetInviteStatus(invite, PartyInviteStatus.Denied);
        DeleteInvite(invite);
    }

    public void DeleteInvite(PartyInvite invite, bool updates = true)
    {
        SetInviteStatus(invite, PartyInviteStatus.Deleted, updates: updates);
        _invites.Remove(invite);
    }

    public bool TryCreateInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite)
    {
        try
        {
            invite = CreateInvite(sender, target);
        }
        catch
        {
            invite = null;
        }

        return invite != null;
    }

    public bool TryCreateAndSendInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite)
    {
        try
        {
            invite = CreateAndSendInvite(sender, target);
        }
        catch
        {
            invite = null;
        }

        return invite != null;
    }

    public PartyInvite CreateInvite(ICommonSession sender, ICommonSession target)
    {
        if (!TryGetPartyByHost(sender, out var party))
            throw new Exception($"Trying to create invite from {sender.Name} when he isn't any party host");

        var sendedInvites = GetInvitesBySender(sender);
        var invitesLimit = _cfg.GetCVar(CCVars220.PartyInvitesLimit);
        if (sendedInvites.Count() > invitesLimit)
            throw new Exception($"{sender.Name} has reached the limit of party invites");

        if (sendedInvites.Any(i => i.Target == target))
            throw new Exception($"Already exist a party invite from {sender.Name} to {target.Name}");

        if (_doesntReceiveInvites.Contains(target))
            throw new Exception($"{target.Name} doesn't receive new party invites");

        var invite = new PartyInvite(GenerateInviteId(), party.Id, sender, target);
        _invites.Add(invite);

        SetInviteStatus(invite, PartyInviteStatus.Created, updates: false);
        return invite;
    }

    public void SendInvite(PartyInvite invite)
    {
        SetInviteStatus(invite, PartyInviteStatus.Sended, false);
        UpdateClientInvite(invite);
    }

    public PartyInvite CreateAndSendInvite(ICommonSession sender, ICommonSession target)
    {
        var invite = CreateInvite(sender, target);
        SendInvite(invite);

        return invite;
    }

    public void SendInvite(ICommonSession sender, ICommonSession target, bool throwException = false)
    {
        if (GetInvitesBySender(sender).Any(i => i.Target == target))
        {
            if (throwException)
                throw new Exception($"{sender.Name} is already sended party invite to {target.Name}");

            return;
        }


    }

    public bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(inviteId);
        return invite != null;
    }

    public bool TryGetInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(sender, target);
        return invite != null;
    }

    public PartyInvite? GetInvite(uint inviteId)
    {
        var result = _invites.Where(i => i.Id == inviteId);
        var count = result.Count();

        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        return result.First();
    }

    public PartyInvite? GetInvite(ICommonSession sender, ICommonSession target)
    {
        var result = _invites.Where(i => i.Sender == sender && i.Target == target);
        var count = result.Count();

        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        return result.First();
    }

    public IEnumerable<PartyInvite> GetInvitesBySender(ICommonSession sender)
    {
        return _invites.Where(i => i.Sender == sender);
    }

    public IEnumerable<PartyInvite> GetInvitesByTarget(ICommonSession target)
    {
        return _invites.Where(i => i.Target == target);
    }

    public void SetInviteStatus(PartyInvite invite, PartyInviteStatus status, bool updates = true)
    {
        var oldStatus = invite.Status;

        if (oldStatus == status)
            return;

        invite.Status = status;

        PartyInviteStatusChanged?.Invoke(new PartyInviteStatusChangedActionArgs(invite.Id, oldStatus, status));

        if (updates)
        {
            UpdateClientInvite(invite);
        }
    }

    private uint GenerateInviteId()
    {
        return _nextInviteId++;
    }

    private void UpdateClientInvites(ICommonSession session)
    {
        var states = GetInvitesBySender(session).Select(i => i.GetState()).ToList();
        states.AddRange(GetInvitesByTarget(session).Select(i => i.GetState()));

        var msg = new UpdateClientPartyInvitesMessage(states);
        SendNetMessage(msg, session);
    }

    private void UpdateClientInvite(PartyInvite invite)
    {
        UpdateClientInvite(invite.Sender, invite);
        UpdateClientInvite(invite.Target, invite);
    }

    private void UpdateClientInvite(ICommonSession session, PartyInvite invite)
    {
        DebugTools.Assert(invite.Sender == session || invite.Target == session);

        var msg = new UpdateClientPartyInviteMessage(invite.GetState());
        SendNetMessage(msg, session);
    }
}

public record struct PartyInviteStatusChangedActionArgs(uint PartyId, PartyInviteStatus OldStatus, PartyInviteStatus NewStatus);
