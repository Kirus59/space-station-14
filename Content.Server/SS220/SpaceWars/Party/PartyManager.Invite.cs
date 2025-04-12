
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    private Dictionary<uint, ServerPartyInvite> _invites = new();

    public void AcceptInvite(uint inviteId, ICommonSession target)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Target != target)
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

        if (invite.Target != target)
            return;

        invite.Status = InviteStatus.Denied;
        DirtyInvite(invite);
        _invites.Remove(invite.Id);
    }

    public void DeleteInvite(uint inviteId, ICommonSession session)
    {
        if (!_invites.TryGetValue(inviteId, out var invite))
            return;

        if (invite.Sender != session)
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
            failReason = $"Not found session for {username}";
            return false;
        }

        return TrySendInvite(sender, target, out failReason);
    }

    public bool TrySendInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(false)] out string? failReason)
    {
        failReason = null;
        if (!TryCreateNewInvite(sender, target, out var invite))
        {
            failReason = $"{sender.Name} has already sent an invite to {target.Name}";
            return false;
        }

        SendInvite(invite);
        return true;
    }

    public void SendInvite(ServerPartyInvite invite)
    {
        invite.Status = InviteStatus.Sended;
        _partySystem?.SendInvite(invite);
    }

    private bool TryGetInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(true)] out ServerPartyInvite? invite)
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

    private bool TryCreateNewInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(true)] out ServerPartyInvite? invite, InviteStatus status = InviteStatus.None)
    {
        invite = null;
        if (TryGetInvite(sender, target, out _))
            return false;

        invite = CreateNewInvite(sender, target, status);
        return true;
    }

    private ServerPartyInvite CreateNewInvite(ICommonSession sender, ICommonSession target, InviteStatus status = InviteStatus.None)
    {
        var id = GenerateInviteId();
        var invite = new ServerPartyInvite(id, sender, target, status);
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

    private void DirtyInvite(ServerPartyInvite invite)
    {
        var state = GetClientInviteState(invite);

        _partySystem?.DirtyInvite(state, invite.Sender);
        _partySystem?.DirtyInvite(state, invite.Target);
    }

    private ClientPartyInviteState GetClientInviteState(ServerPartyInvite invite)
    {
        return new ClientPartyInviteState(invite.Id, invite.Status);
    }
}

public sealed class ServerPartyInvite : SharedPartyInvite
{
    public readonly ICommonSession Sender;
    public readonly ICommonSession Target;

    public ServerPartyInvite(uint id, ICommonSession sender, ICommonSession target, InviteStatus status = InviteStatus.None) : base(id, status)
    {
        Sender = sender;
        Target = target;
    }
}
