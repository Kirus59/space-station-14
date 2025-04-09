
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

    public List<PartyData> Parties => _parties;
    private List<PartyData> _parties = new();

    private Dictionary<NetUserId, PartyUser> _partyUsers = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetPartySystem(PartySystem partySystem)
    {
        _partySystem = partySystem;
    }

    public bool TryCreateParty(NetUserId leader, [NotNullWhen(false)] out string? reason)
    {
        return TryCreateParty(GetPartyUser(leader), out reason);
    }

    /// <inheritdoc/>
    public bool TryCreateParty(PartyUser leader, [NotNullWhen(false)] out string? reason)
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
    public PartyData CreateParty(PartyUser leader)
    {
        CheckAvaliableUser(leader);

        var party = new PartyData();
        _parties.Add(party);

        SetPartyUserRole(leader, PartyRole.Leader);
        party.AddMember(leader);
        OnPartyUserUpdated?.Invoke(leader);

        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(PartyData party)
    {
        while (party.Members.Count > 0)
        {
            var user = party.Members[0].Id;
            RemoveUserFromParty(user, party);
        }

        OnPartyDisbanding?.Invoke(party);
        _parties.Remove(party);
    }

    public PartyData? GetPartyByLeader(NetUserId leader)
    {
        return GetPartyByLeader(GetPartyUser(leader));
    }

    /// <inheritdoc/>
    public PartyData? GetPartyByLeader(PartyUser leader)
    {
        return _parties.Find(x => x.IsLeader(leader));
    }

    public PartyData? GetPartyByMember(NetUserId member)
    {
        return GetPartyByMember(GetPartyUser(member));
    }

    /// <inheritdoc/>
    public PartyData? GetPartyByMember(PartyUser member)
    {
        return _parties.Find(x => x.ContainUser(member));
    }

    /// <inheritdoc/>
    public bool TryAddUserToParty(PartyUser user, PartyData party, [NotNullWhen(false)] out string? reason)
    {
        reason = null;

        try
        {
            AddUserToParty(user, party);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    public void AddUserToParty(PartyUser user, PartyData party)
    {
        CheckAvaliableUser(user);
        party.AddMember(user);
        SetPartyUserRole(user, PartyRole.Member);

        OnPartyDataUpdated?.Invoke(party);
    }

    /// <inheritdoc/>
    public void RemoveUserFromParty(NetUserId user, PartyData party)
    {
        if (!party.TryGetMember(user, out var partyUser))
            return;

        party.RemoveMember(partyUser);
        OnPartyDataUpdated?.Invoke(party);
        OnPartyUserUpdated?.Invoke(partyUser);
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

    public bool TryGetSessionByPartyUser(PartyUser user, [NotNullWhen(true)] out ICommonSession? session)
    {
        _playerManager.TryGetSessionById(user.Id, out session);
        return session != null;
    }

    public ICommonSession GetSessionByPartyUser(PartyUser user)
    {
        return _playerManager.GetSessionById(user.Id);
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

    private bool CheckAvaliableUser(PartyUser user, bool throwExeption = true)
    {
        if (GetPartyByMember(user) != null)
        {
            if (throwExeption)
                throw new ArgumentException($"{user.Id} is already the member of the another party");

            return false;
        }

        return true;
    }
}
