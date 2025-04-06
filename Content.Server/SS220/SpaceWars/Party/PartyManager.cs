
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Enums;
using System.Diagnostics.CodeAnalysis;
using Content.Server.SS220.SpaceWars.Party.Systems;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private PartySystem? _partySystem = default!;

    public event Action<PartyData>? OnPartyDataUpdated;
    public event Action<PartyData>? OnPartyDisbanding;
    public event Action<PartyUser>? OnPartyUserUpdated;
    public event Action<PartyInvite>? OnPartyInviteUpdated;

    public List<PartyData> Parties => _parties;
    private List<PartyData> _parties = new();

    private Dictionary<NetUserId, PartyUser> _partyUsers = new();
    private HashSet<PartyInvite> _sendedInvites = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetPartySystem(PartySystem partySystem)
    {
        _partySystem = partySystem;
    }

    /// <inheritdoc/>
    public bool TryCreateParty(NetUserId leader, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        try
        {
            CreateParty(leader);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    public PartyData CreateParty(NetUserId leader)
    {
        CheckAvaliableMember(leader);

        var partyUser = GetPartyUser(leader);
        var party = new PartyData();
        _parties.Add(party);

        SetPartyUserRole(partyUser, PartyRole.Leader);
        party.AddMember(partyUser);
        OnPartyUserUpdated?.Invoke(partyUser);

        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(PartyData party)
    {
        foreach (var member in party.Members)
            RemovePlayerFromParty(member.Id, party);

        OnPartyDisbanding?.Invoke(party);
        _parties.Remove(party);
    }

    /// <inheritdoc/>
    public PartyData? GetPartyByLeader(NetUserId leader)
    {
        return _parties.Find(x => x.IsLeader(leader));
    }

    /// <inheritdoc/>
    public PartyData? GetPartyByMember(NetUserId member)
    {
        return _parties.Find(x => x.ContainMember(member));
    }

    /// <inheritdoc/>
    public bool TryAddPlayerToParty(NetUserId member, PartyData party, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        try
        {
            AddPlayerToParty(member, party);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    public void AddPlayerToParty(NetUserId member, PartyData party)
    {
        CheckAvaliableMember(member);

        var partyUser = GetPartyUser(member);
        party.AddMember(partyUser);
        SetPartyUserRole(partyUser, PartyRole.Member);

        OnPartyDataUpdated?.Invoke(party);
    }

    /// <inheritdoc/>
    public void RemovePlayerFromParty(NetUserId member, PartyData party)
    {
        if (!party.TryGetMember(member, out var partyUser))
            return;

        party.RemoveMember(partyUser);
        OnPartyDataUpdated?.Invoke(party);
        OnPartyUserUpdated?.Invoke(partyUser);
    }

    private bool CheckAvaliableMember(NetUserId member, bool throwExeption = true)
    {
        if (!_partyUsers.TryGetValue(member, out var partyUser))
            return true;

        if (GetPartyByMember(member) != null)
        {
            if (throwExeption)
                throw new ArgumentException($"{member.UserId} is already the member of the another party");

            return false;
        }

        return true;
    }

    public PartyUser GetPartyUser(NetUserId userId)
    {
        if (_partyUsers.TryGetValue(userId, out var user))
            return user;
        else
        {
            var session = _playerManager.GetSessionById(userId);
            var partyUser = new PartyUser(userId, PartyRole.Member, session.Name, session.Status is SessionStatus.Connected);
            _partyUsers.Add(userId, partyUser);

            return partyUser;
        }
    }

    public void SetPartyUserRole(PartyUser user, PartyRole role)
    {
        user.Role = role;
        OnPartyUserUpdated?.Invoke(user);
    }

    public void SetPartyUserConnected(PartyUser user, bool connected)
    {
        user.Connected = connected;
        OnPartyUserUpdated?.Invoke(user);
    }

    public void SendInviteToUser(ICommonSession sender, string username)
    {
        var invite = new PartyInvite(sender.UserId, sender.Name);
        SendInviteToUser(invite, username);
    }

    public void SendInviteToUser(ICommonSession sender, ICommonSession target)
    {
        var invite = new PartyInvite(sender.UserId, sender.Name);
        SendInviteToUser(invite, target);
    }

    public void SendInviteToUser(PartyInvite invite, string username)
    {
        var inviteState = GetInviteState(invite);
        if (!_playerManager.TryGetSessionByUsername(username, out var target))
        {
            inviteState.InviteStatus = InviteStatus.UserNotFound;
            HandleInviteState(invite, inviteState);
            return;
        }

        SendInviteToUser(invite, target);
    }

    public void SendInviteToUser(PartyInvite invite, ICommonSession target)
    {
        var inviteState = GetInviteState(invite);
        if (!_playerManager.TryGetSessionById(invite.Sender, out var sender))
        {
            inviteState.InviteStatus = InviteStatus.Error;
            HandleInviteState(invite, inviteState);
            return;
        }

        if (target == sender)
        {
            inviteState.InviteStatus = InviteStatus.TargetIsSender;
            HandleInviteState(invite, inviteState);
            return;
        }

        if (_sendedInvites.Contains(invite))
        {
            inviteState.InviteStatus = InviteStatus.AlreadySended;
            HandleInviteState(invite, inviteState);
            return;
        }

        inviteState.Target = target.UserId;
        inviteState.TargetName = target.Name;
        inviteState.InviteStatus = InviteStatus.Sended;
        HandleInviteState(invite, inviteState);
    }

    public PartyInviteState GetInviteState(PartyInvite invite)
    {
        return new PartyInviteState(invite.Target, invite.TargetName, invite.InviteStatus);
    }

    public void HandleInviteState(PartyInvite invite, PartyInviteState state)
    {
        invite.Target = state.Target;
        invite.TargetName = state.TargetName;
        invite.InviteStatus = state.InviteStatus;
        OnPartyInviteUpdated?.Invoke(invite);
    }

    public void AcceptInvite(PartyInvite invite)
    {
        var party = GetPartyByLeader(invite.Sender);
        if (party != null && invite.Target != null)
            AddPlayerToParty(invite.Target.Value, party);

        var inviteState = GetInviteState(invite);
        inviteState.InviteStatus = InviteStatus.Accepted;
        HandleInviteState(invite, inviteState);

        _sendedInvites.Remove(invite);
    }

    public void DenyInvite(PartyInvite invite)
    {
        var inviteState = GetInviteState(invite);
        inviteState.InviteStatus = InviteStatus.Denied;
        HandleInviteState(invite, inviteState);

        _sendedInvites.Remove(invite);
    }

    #region PartyMenuUI
    public void OpenPartyMenu(ICommonSession session)
    {
        _partySystem?.OpenPartyMenu(session);
    }

    public void ClosePartyMenu(ICommonSession session)
    {
        _partySystem?.ClosePartyMenu(session);
    }
    #endregion
}

public record struct PartyInviteState
{
    public NetUserId? Target;
    public string? TargetName;
    public InviteStatus InviteStatus;

    public PartyInviteState(NetUserId? target, string? targetName, InviteStatus inviteStatus)
    {
        Target = target;
        TargetName = targetName;
        InviteStatus = inviteStatus;
    }
}
