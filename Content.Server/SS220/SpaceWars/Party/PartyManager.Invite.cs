
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

    private readonly HashSet<PartyInvite> _invites = [];
    private readonly HashSet<ICommonSession> _doesNotReceiveInvites = [];

    private uint _nextInviteId = 0;

    public void InviteInitialize()
    {
        SubscribeNetMessage<MsgInviteUserRequest>(OnInviteInPartyRequest);
        SubscribeNetMessage<MsgAcceptInviteRequest>(OnAcceptInvite);
        SubscribeNetMessage<MsgDenyInviteRequest>(OnDenyInvite);
        SubscribeNetMessage<MsgInviteRequest>(OnDeleteInvite);
        SubscribeNetMessage<MsgSetReceiveInvitesStatus>(OnSetReceiveInvitesStatus);
    }

    private void OnInviteInPartyRequest(MsgInviteUserRequest message, ICommonSession sender)
    {
        var success = TryInvite(out var responceMessage);

        var responce = new MsgInviteUserResponce
        {
            Id = message.Id,
            Success = success,
            Text = responceMessage
        };

        SendNetMessage(responce, sender);

        bool TryInvite(out string responceMessage)
        {
            if (!TryGetPartyByHost(sender, out var party))
            {
                responceMessage = Loc.GetString("party-invite-request-hosted-party-not-found");
                return false;
            }

            if (!_playerManager.TryGetSessionByUsername(message.Username, out var receiver))
            {
                responceMessage = Loc.GetString("party-invite-request-user-not-found", ("username", message.Username));
                return false;
            }

            if (!TryCreateAndSendInvite(party, receiver, out var checkoutResult, out _))
            {
                responceMessage = checkoutResult switch
                {
                    PartyInviteCheckoutResult.PartyNotExist => Loc.GetString("party-invite-request-party-not-exist", ("partyId", party.Id)),
                    PartyInviteCheckoutResult.AlreadyMember => Loc.GetString("party-invite-request-receiver-already-member", ("username", receiver.Name)),
                    PartyInviteCheckoutResult.LimitReached => Loc.GetString("party-invite-request-limit-reached"),
                    PartyInviteCheckoutResult.AlreadyInvited => Loc.GetString("party-invite-request-already-invited", ("username", receiver.Name)),
                    PartyInviteCheckoutResult.DoesNotReseive => Loc.GetString("party-invite-request-does-not-receive", ("username", receiver.Name)),
                    _ => Loc.GetString("party-invite-request-other-reason", ("username", receiver.Name))
                };
                return false;
            }

            responceMessage = string.Empty;
            return true;
        }
    }

    private void OnAcceptInvite(MsgAcceptInviteRequest message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Target)
            return;

        TryAcceptInvite(invite, force: true);
    }

    private void OnDenyInvite(MsgDenyInviteRequest message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (sender != invite.Target)
            return;

        DenyInvite(invite);
    }

    private void OnDeleteInvite(MsgInviteRequest message, ICommonSession sender)
    {
        if (!TryGetInvite(message.InviteId, out var invite))
            return;

        if (invite.Party.Host.Session != sender)
            return;

        DeleteInvite(invite);
    }

    private void OnSetReceiveInvitesStatus(MsgSetReceiveInvitesStatus message, ICommonSession sender)
    {
        if (message.ReceiveInvites)
            _doesNotReceiveInvites.Remove(sender);
        else
            _doesNotReceiveInvites.Add(sender);
    }

    public bool TryAcceptInvite(IPartyInvite invite, bool force = false, bool ignoreLimit = false)
    {
        try
        {
            AcceptInvite(invite, force, ignoreLimit);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void AcceptInvite(IPartyInvite invite, bool force = false, bool ignoreLimit = false)
    {
        var party = invite.Party;
        if (!PartyExist(party))
            throw new ArgumentException($"Party \"{invite.Party.Id}\" doesn't exist!", nameof(invite));

        SetInviteStatus(invite, PartyInviteStatus.Accepted, updates: false);
        AddMember(party, invite.Target, PartyMemberRole.Member, force, ignoreLimit);

        DebugTools.Assert(party.ContainsMember(invite.Target));
        DeleteInvite(invite);
    }

    public void DenyInvite(IPartyInvite invite)
    {
        SetInviteStatus(invite, PartyInviteStatus.Denied);
        DeleteInvite(invite);
    }

    public void DeleteInvite(uint inviteId)
    {
        if (!TryGetInvite(inviteId, out var invite))
            return;

        DeleteInvite(invite);
    }

    public void DeleteInvite(Party party, ICommonSession receiver)
    {
        if (!TryGetInvite(party, receiver, out var invite))
            return;

        DeleteInvite(invite);
    }

    public void DeleteInvite(IPartyInvite invite)
    {
        if (invite is not PartyInvite partyInvite)
            return;

        SetInviteStatus(partyInvite, PartyInviteStatus.Deleted, false);

        var msg = new MsgPartyInviteDeleted(partyInvite.Id);
        SendNetMessage(msg, partyInvite.Party.Host.Session);
        SendNetMessage(msg, partyInvite.Target);

        _invites.Remove(partyInvite);
    }

    public bool TryCreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out IPartyInvite? invite)
    {
        return TryCreateInvite(party, target, out _, out invite);
    }

    public bool TryCreateInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out IPartyInvite? invite)
    {
        invite = default;
        if (!InviteAvailableCheckout(party, target, out result))
            return false;

        try
        {
            invite = CreateInvite(party, target, checkout: false);
            return true;
        }
        catch
        {
            invite = default;
            return false;
        }
    }

    public IPartyInvite CreateInvite(Party party, ICommonSession target, bool checkout = true)
    {
        if (!PartyExist(party))
            throw new ArgumentException($"Party \"{party.Id}\" doesn't exist!", nameof(party));

        if (checkout && !InviteAvailableCheckout(party, target, out var result))
        {
            var exception = result switch
            {
                PartyInviteCheckoutResult.PartyNotExist => $"Party \"{party.Id}\" doesn't exist!",
                PartyInviteCheckoutResult.AlreadyMember => $"{target.Name} is already a member of party \"{party.Id}\".",
                PartyInviteCheckoutResult.LimitReached => $"Reached the limit of invites from party \"{party.Id}\".",
                PartyInviteCheckoutResult.AlreadyInvited => $"Already exist an invite from party \"{party.Id}\" to {target.Name}.",
                PartyInviteCheckoutResult.DoesNotReseive => $"{target.Name} doesn't receive new party invites.",
                _ => null
            };

            throw new Exception(exception);
        }

        var invite = new PartyInvite(GenerateInviteId(), party, target);
        _invites.Add(invite);

        SetInviteStatus(invite, PartyInviteStatus.Created, updates: false);
        return invite;
    }

    public bool TryCreateAndSendInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out IPartyInvite? invite)
    {
        if (!TryCreateInvite(party, target, out result, out invite))
            return false;

        SendInvite(invite);
        return true;
    }

    public IPartyInvite CreateAndSendInvite(Party party, ICommonSession target, bool checkout = true)
    {
        var invite = CreateInvite(party, target, checkout);
        SendInvite(invite);

        return invite;
    }

    public void SendInvite(IPartyInvite invite)
    {
        SetInviteStatus(invite, PartyInviteStatus.Sended, false);

        var state = invite.GetState();
        var msgSended = new MsgPartyInviteSended(state);
        SendNetMessage(msgSended, invite.Party.Host.Session);

        var msgReceived = new MsgPartyInviteReceived(invite.GetState());
        SendNetMessage(msgReceived, invite.Target);
    }

    public bool InviteAvailable(Party party, ICommonSession target)
    {
        return InviteAvailableCheckout(party, target, out _);
    }

    public bool InviteAvailableCheckout(Party party, ICommonSession target, out PartyInviteCheckoutResult result)
    {
        if (!PartyExist(party))
        {
            result = PartyInviteCheckoutResult.PartyNotExist;
            return false;
        }

        if (party.ContainsMember(target))
        {
            result = PartyInviteCheckoutResult.AlreadyMember;
            return false;
        }

        var existedInvites = GetInvitesByParty(party);
        if (existedInvites.Any(i => i.Target == target))
        {
            result = PartyInviteCheckoutResult.AlreadyInvited;
            return false;
        }

        var invitesLimit = _cfg.GetCVar(CCVars220.PartyInvitesLimit);
        if (existedInvites.Count() > invitesLimit)
        {
            result = PartyInviteCheckoutResult.LimitReached;
            return false;
        }

        if (_doesNotReceiveInvites.Contains(target))
        {
            result = PartyInviteCheckoutResult.DoesNotReseive;
            return false;
        }

        result = PartyInviteCheckoutResult.Available;
        return true;
    }

    public bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out IPartyInvite? invite)
    {
        invite = GetInvite(inviteId);
        return invite != null;
    }

    public bool TryGetInvite(Party party, ICommonSession receiver, [NotNullWhen(true)] out IPartyInvite? invite)
    {
        invite = GetInvite(party, receiver);
        return invite != null;
    }

    public IPartyInvite? GetInvite(uint inviteId)
    {
        return _invites.FirstOrDefault(i => i.Id == inviteId);
    }

    public IPartyInvite? GetInvite(Party party, ICommonSession target)
    {
        return _invites.FirstOrDefault(i => i.Party == party && i.Target == target);
    }

    public IEnumerable<IPartyInvite> GetInvitesByParty(Party party)
    {
        return _invites.Where(i => i.Party == party);
    }

    public IEnumerable<IPartyInvite> GetInvitesByTarget(ICommonSession target)
    {
        return _invites.Where(i => i.Target == target);
    }

    public void SetInviteStatus(IPartyInvite invite, PartyInviteStatus status, bool updates = true)
    {
        var oldStatus = invite.Status;

        if (oldStatus == status)
            return;

        invite.SetStatus(status);
        PartyInviteStatusChanged?.Invoke(new PartyInviteStatusChangedActionArgs(invite.Id, oldStatus, status));

        if (updates)
        {
            UpdateClientInviteState(invite);
        }
    }

    private uint GenerateInviteId()
    {
        return _nextInviteId++;
    }

    private void UpdateClientInvites(ICommonSession session)
    {
        var states = GetInvitesByTarget(session).Select(i => i.GetState()).ToList();

        if (TryGetPartyByHost(session, out var party))
            states.AddRange(GetInvitesByParty(party).Select(i => i.GetState()));

        var msg = new MsgUpdateClientPartyInvites(states);
        SendNetMessage(msg, session);
    }

    private void UpdateClientInviteState(IPartyInvite invite)
    {
        UpdateClientInviteState(invite.Party.Host.Session, invite);
        UpdateClientInviteState(invite.Target, invite);
    }

    private void UpdateClientInviteState(ICommonSession session, IPartyInvite invite)
    {
        DebugTools.Assert(invite.Party.Host.Session == session || invite.Target == session);

        var msg = new MsgHandlePartyInviteState(invite.GetState());
        SendNetMessage(msg, session);
    }
}
