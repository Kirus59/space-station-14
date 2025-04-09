
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    private Dictionary<uint, PartyInvite> _invites = new();

    public void AcceptInvite(uint inviteId, ICommonSession target)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Target.Id != target.UserId)
            return;

        invite.Status = InviteStatus.Accepted;
        DirtyInvite(invite);
        TryAddUserToParty(invite.Target, invite.Party, out _);
        _invites.Remove(invite.Id);
    }

    public void DenyInvite(uint inviteId, ICommonSession target)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Target.Id != target.UserId)
            return;

        invite.Status = InviteStatus.Denied;
        DirtyInvite(invite);
        _invites.Remove(invite.Id);
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
            failReason = $"Not found session for {username}";
            return false;
        }

        return TrySendInvite(sender, target, out failReason);
    }

    public bool TrySendInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(false)] out string? failReason)
    {
        var party = GetPartyByLeader(sender.UserId);
        if (party == null)
        {
            failReason = $"{sender.Name} is not a party leader";
            return false;
        }

        return TrySendInvite(party, sender, target, out failReason);
    }

    public bool TrySendInvite(PartyData party, ICommonSession sender, ICommonSession target, [NotNullWhen(false)] out string? failReason)
    {
        failReason = null;
        var senderUser = GetPartyUser(sender.UserId);
        var targetUser = GetPartyUser(target.UserId);
        if (!TryCreateNewInvite(senderUser, targetUser, party, out var invite))
        {
            failReason = $"{sender.Name} has already sent an invite to {target.Name}";
            return false;
        }

        SendInvite(invite, sender, target);
        return true;
    }

    public void SendInvite(PartyInvite invite, ICommonSession sender, ICommonSession target)
    {
        invite.Status = InviteStatus.Sended;
        _partySystem?.SendInvite(invite, sender, target);
    }

    private bool TryGetInvite(PartyUser sender, PartyUser target, [NotNullWhen(true)] out PartyInvite? invite)
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

    private bool TryCreateNewInvite(PartyUser sender, PartyUser target, PartyData party, [NotNullWhen(true)] out PartyInvite? invite, InviteStatus status = InviteStatus.None)
    {
        invite = null;
        if (TryGetInvite(sender, target, out _))
            return false;

        invite = CreateNewInvite(sender, target, party, status);
        return true;
    }

    private PartyInvite CreateNewInvite(PartyUser sender, PartyUser target, PartyData party, InviteStatus status = InviteStatus.None)
    {
        var id = GenerateInviteId();
        var invite = new PartyInvite(id, sender, target, party, status);
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

    private void DirtyInvite(PartyInvite invite)
    {
        var state = GetInviteState(invite);
        if (_playerManager.TryGetSessionById(invite.Sender.Id, out var senderSession))
            _partySystem?.DirtyInvite(state, senderSession);

        if (_playerManager.TryGetSessionById(invite.Target.Id, out var targetSession))
            _partySystem?.DirtyInvite(state, targetSession);
    }

    private PartyInviteState GetInviteState(PartyInvite invite)
    {
        return new PartyInviteState(invite.Id, invite.Status);
    }
}
