using Content.Server.EUI;
using Content.Server.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Enums;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<PartyData>? OnPartyDataUpdated;
    public event Action<PartyData>? OnPartyDisbanding;
    public event Action<PartyUser>? OnPartyUserUpdated;

    public List<PartyData> Parties => _parties;
    private List<PartyData> _parties = new();

    public Dictionary<ICommonSession, PartyMenuEui> OpenedMenu => _openedMenu;
    private Dictionary<ICommonSession, PartyMenuEui> _openedMenu = new();

    private Dictionary<NetUserId, PartyUser> _partyUsers = new();

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

        SetRole(partyUser, PartyRole.Leader);
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
        SetRole(partyUser, PartyRole.Member);

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

    public void SetRole(PartyUser user, PartyRole role)
    {
        user.Role = role;
        OnPartyUserUpdated?.Invoke(user);
    }

    public void SetConnected(PartyUser user, bool connected)
    {
        user.Connected = connected;
        OnPartyUserUpdated?.Invoke(user);
    }

    #region PartyMenuUI
    public void OpenPartyMenu(ICommonSession session)
    {
        if (_openedMenu.ContainsKey(session))
            return;

        var ui = new PartyMenuEui();
        _openedMenu.Add(session, ui);
        _euiManager.OpenEui(ui, session);
    }

    public void ClosePartyMenu(ICommonSession session)
    {
        if (!_openedMenu.TryGetValue(session, out var ui))
            return;

        _openedMenu.Remove(session);
        _euiManager.CloseEui(ui);
    }
    #endregion
}
