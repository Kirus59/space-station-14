
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Enums;
using System.Diagnostics.CodeAnalysis;
using Content.Server.SS220.SpaceWars.Party.Systems;
using Robust.Shared.Utility;

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
        while (party.Members.Count > 0)
        {
            var user = party.Members[0].Id;
            RemoveUserFromParty(user, party);
        }

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
    public bool TryAddPlayerToParty(NetUserId user, PartyData party, [NotNullWhen(false)] out string? reason)
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
    public void AddUserToParty(NetUserId user, PartyData party)
    {
        CheckAvaliableMember(user);

        var partyUser = GetPartyUser(user);
        party.AddMember(partyUser);
        SetPartyUserRole(partyUser, PartyRole.Member);

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
