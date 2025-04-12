
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Enums;
using System.Diagnostics.CodeAnalysis;
using Content.Server.SS220.SpaceWars.Party.Systems;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private PartySystem? _partySystem = default!;

    public event Action<ServerPartyData>? OnPartyDataUpdated;
    public event Action<ServerPartyData>? OnPartyDisbanding;

    public List<ServerPartyData> Parties => _parties;
    private List<ServerPartyData> _parties = new();

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetPartySystem(PartySystem partySystem)
    {
        _partySystem = partySystem;
    }

    /// <inheritdoc/>
    public bool TryCreateParty(ICommonSession leader, [NotNullWhen(false)] out string? reason)
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
    public ServerPartyData CreateParty(ICommonSession leader)
    {
        CheckAvaliableUser(leader);

        var party = new ServerPartyData(GetFreePartyId());
        _parties.Add(party);

        AddUserToParty(leader, party, PartyRole.Leader);
        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(ServerPartyData party)
    {
        var usersToRemove = party.Members.Keys;
        foreach (var user in usersToRemove)
            RemoveUserFromParty(user, party);

        party.Disbanded = true;
        DirtyParty(party);

        OnPartyDisbanding?.Invoke(party);
        _parties.Remove(party);
    }

    public ServerPartyData? GetPartyById(uint id)
    {
        return _parties.Find(p => p.Id == id);
    }

    public ServerPartyData? GetPartyByLeader(ICommonSession leader)
    {
        return _parties.Find(p => p.Leader == leader);
    }

    public ServerPartyData? GetPartyByMember(ICommonSession member)
    {
        return _parties.Find(p => p.ContainsUser(member));
    }

    public bool TryAddUserToParty(ICommonSession user, ServerPartyData party, [NotNullWhen(false)] out string? reason)
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

    public void AddUserToParty(ICommonSession user, ServerPartyData party, PartyRole role = PartyRole.Member)
    {
        CheckAvaliableUser(user);

        if (!party.AddMember(user, role))
            return;

        SetCurrentParty(user, party);
        DirtyParty(party);

        OnPartyDataUpdated?.Invoke(party);
    }

    public void RemoveUserFromParty(ICommonSession user, ServerPartyData party)
    {
        if (!party.RemoveMember(user))
            return;

        SetCurrentParty(user, null);
        DirtyParty(party);

        OnPartyDataUpdated?.Invoke(party);
    }

    private bool CheckAvaliableUser(ICommonSession user, bool throwExeption = true)
    {
        if (GetPartyByMember(user) != null)
        {
            if (throwExeption)
                throw new ArgumentException($"{user.Name} is already the member of the another party");

            return false;
        }

        return true;
    }

    private ClientPartyDataState GetClientPartyState(ServerPartyData party, ICommonSession client)
    {
        var localUserInfo = party.GetUserInfo(client);
        if (localUserInfo == null)
            throw new ArgumentException($"{client.Name} is not the member of this party");

        var membersList = party.Members.Select(x => x.Value).ToList();
        return new ClientPartyDataState(party.Id, localUserInfo, membersList, party.Disbanded);
    }

    private void SetCurrentParty(ICommonSession session, ServerPartyData? party)
    {
        ClientPartyDataState? state = null;
        if (party != null)
            state = GetClientPartyState(party, session);

        _partySystem?.SetCurrentParty(state, session);
    }

    private void DirtyParty(ServerPartyData party)
    {
        foreach (var (session, _) in party.Members)
        {
            var state = GetClientPartyState(party, session);
            _partySystem?.UpdatePartyData(state, session);
        }
    }

    private uint GetFreePartyId()
    {
        uint id = 1;
        while (GetPartyById(id) != null)
            id++;

        return id;
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

public sealed class ServerPartyData : SharedPartyData
{
    public ICommonSession? Leader => GetLeader();
    public Dictionary<ICommonSession, PartyUserInfo> Members = new();

    public ServerPartyData(uint id) : base(id)
    {
    }

    public ICommonSession? GetLeader()
    {
        foreach (var (session, info) in Members)
        {
            if (info.Role == PartyRole.Leader)
                return session;
        }

        return null;
    }

    public bool AddMember(ICommonSession session, PartyRole role)
    {
        if (Members.ContainsKey(session) ||
            (role == PartyRole.Leader && Leader != null))
            return false; ;

        var connected = session.Status == SessionStatus.Connected || session.Status == SessionStatus.InGame;
        var userInfo = new PartyUserInfo(role, session.Name, connected);
        Members.Add(session, userInfo);
        return true;
    }

    public bool RemoveMember(ICommonSession session)
    {
        return Members.Remove(session);
    }

    public PartyUserInfo? GetUserInfo(ICommonSession session)
    {
        Members.TryGetValue(session, out var userInfo);
        return userInfo;
    }

    public bool ContainsUser(ICommonSession session)
    {
        return Members.ContainsKey(session);
    }
}
