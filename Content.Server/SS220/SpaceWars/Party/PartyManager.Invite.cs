
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Pidgin;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    private HashSet<PartyInvite> _invites = new();
    private HashSet<ICommonSession> _doesNotReceiveInvites = new();

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
        var success = TryInvite(out var responceMessage);

        var responce = new InviteInPartyResponceMessage
        {
            Success = success,
            Text = responceMessage
        };

        SendNetMessage(responce, sender);

        bool TryInvite(out string responceMessage)
        {
            if (!TryGetPartyByHost(sender, out var party))
            {
                responceMessage = Loc.GetString($"");
                return false;
            }

            if (!_playerManager.TryGetSessionByUsername(message.Username, out var receiver))
            {
                responceMessage = Loc.GetString($"");
                return false;
            }

            if (!TryCreateAndSendInvite(party, receiver, out var checkoutResult, out _))
            {
                responceMessage = checkoutResult switch
                {
                    InviteCheckoutResult.LimitReached => Loc.GetString(""),
                    InviteCheckoutResult.AlreadyExist => Loc.GetString(""),
                    InviteCheckoutResult.DoesNotReseive => Loc.GetString(""),
                    _ => Loc.GetString("")
                };
                return false;
            }

            responceMessage = string.Empty;
            return true;
        }
    }

    private void OnAcceptInvite(AcceptInviteMessage message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Receiver)
            return;

        AcceptInvite(invite, force: true);
    }

    private void OnDenyInvite(DenyInviteMessage message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Receiver)
            return;

        DenyInvite(invite);
    }

    private void OnDeleteInvite(DeleteInviteRequestMessage message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (invite.Party.Host.Session != sender)
            return;

        DeleteInvite(invite);
    }

    private void OnSetReceiveInvitesStatus(SetReceiveInvitesStatusMessage message, ICommonSession sender)
    {
        if (message.ReceiveInvites)
            _doesNotReceiveInvites.Remove(sender);
        else
            _doesNotReceiveInvites.Add(sender);
    }

    public bool AcceptInvite(PartyInvite invite, bool force = false, bool throwException = false, bool ignoreLimit = false)
    {
        var party = invite.Party;
        if (!Exist(party))
        {
            if (throwException)
                throw new Exception($"Party \"{invite.Party.Id}\" doesn't exist!");

            return false;
        }

        SetInviteStatus(invite, PartyInviteStatus.Accepted, updates: false);
        if (!AddMember(party, invite.Receiver, PartyMemberRole.Member, force: force, ignoreLimit: ignoreLimit, throwException: throwException))
            return false;

        DeleteInvite(invite);
        return true;
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

    public bool TryCreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite)
    {
        return TryCreateInvite(party, target, out _, out invite);
    }

    public bool TryCreateInvite(Party party, ICommonSession target, out InviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = null;

        if (!InviteCheckout(party, target, out result))
            return false;

        invite = CreateInvite(party, target, checkout: false);
        return invite != null;
    }

    public bool TryCreateAndSendInvite(Party party, ICommonSession target, out InviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite)
    {
        if (!TryCreateInvite(party, target, out result, out invite))
            return false;

        SendInvite(invite);
        return true;
    }

    public PartyInvite CreateInvite(Party party, ICommonSession target, bool checkout = true)
    {
        if (checkout && !InviteCheckout(party, target, out var result))
        {
            var exception = result switch
            {
                InviteCheckoutResult.LimitReached => $"Reached the limit of invites from party \"{party.Id}\"",
                InviteCheckoutResult.AlreadyExist => $"Already exist an invite from party \"{party.Id}\" to {target.Name}",
                InviteCheckoutResult.DoesNotReseive => $"{target.Name} doesn't receive new party invites",
                _ => null
            };

            throw new Exception(exception);
        }

        var invite = new PartyInvite(GenerateInviteId(), party, target);
        _invites.Add(invite);

        SetInviteStatus(invite, PartyInviteStatus.Created, updates: false);
        return invite;
    }

    public PartyInvite CreateAndSendInvite(Party party, ICommonSession target, bool checkout = true)
    {
        var invite = CreateInvite(party, target, checkout);
        SendInvite(invite);

        return invite;
    }

    public void SendInvite(PartyInvite invite)
    {
        SetInviteStatus(invite, PartyInviteStatus.Sended, false);
        UpdateClientInvite(invite);
    }

    public bool InviteAvailable(Party party, ICommonSession target)
    {
        return InviteCheckout(party, target, out _);
    }

    public bool InviteCheckout(Party party, ICommonSession target, out InviteCheckoutResult result)
    {
        var existedInvites = GetInvitesByParty(party);
        if (existedInvites.Any(i => i.Receiver == target))
        {
            result = InviteCheckoutResult.AlreadyExist;
            return false;
        }

        var invitesLimit = _cfg.GetCVar(CCVars220.PartyInvitesLimit);
        if (existedInvites.Count() > invitesLimit)
        {
            result = InviteCheckoutResult.LimitReached;
            return false;
        }

        if (_doesNotReceiveInvites.Contains(target))
        {
            result = InviteCheckoutResult.DoesNotReseive;
            return false;
        }

        result = InviteCheckoutResult.Available;
        return true;
    }

    public bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(inviteId);
        return invite != null;
    }

    public bool TryGetInvite(Party party, ICommonSession receiver, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(party, receiver);
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

    public PartyInvite? GetInvite(Party party, ICommonSession receiver)
    {
        var result = GetInvitesByParty(party).Where(i => i.Receiver == receiver);
        var count = result.Count();

        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        return result.First();
    }

    public IEnumerable<PartyInvite> GetInvitesByParty(Party party)
    {
        return _invites.Where(i => i.Party == party);
    }

    public IEnumerable<PartyInvite> GetInvitesByReceiver(ICommonSession receiver)
    {
        return _invites.Where(i => i.Receiver == receiver);
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
        var states = GetInvitesByReceiver(session).Select(i => i.GetState()).ToList();

        if (TryGetPartyByHost(session, out var party))
            states.AddRange(GetInvitesByParty(party).Select(i => i.GetState()));

        var msg = new UpdateClientPartyInvitesMessage(states);
        SendNetMessage(msg, session);
    }

    private void UpdateClientInvite(PartyInvite invite)
    {
        UpdateClientInvite(invite.Party.Host.Session, invite);
        UpdateClientInvite(invite.Receiver, invite);
    }

    private void UpdateClientInvite(ICommonSession session, PartyInvite invite)
    {
        DebugTools.Assert(invite.Party.Host.Session == session || invite.Receiver == session);

        var msg = new UpdateClientPartyInviteMessage(invite.GetState());
        SendNetMessage(msg, session);
    }

    public enum InviteCheckoutResult
    {
        Available,

        LimitReached,
        AlreadyExist,
        DoesNotReseive
    }
}

public record struct PartyInviteStatusChangedActionArgs(uint PartyId, PartyInviteStatus OldStatus, PartyInviteStatus NewStatus);
