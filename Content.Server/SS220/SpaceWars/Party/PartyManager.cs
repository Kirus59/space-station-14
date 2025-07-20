
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Network;
using Robust.Shared.Configuration;
using Content.Shared.SS220.CCVars;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetManager _net = default!;

    public event Action<ServerPartyData>? PartyDataUpdated;
    public event Action<ServerPartyData>? PartyDisbanding;

    public List<ServerPartyData> Parties => _parties;
    private List<ServerPartyData> _parties = new();

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        _cfg.OnValueChanged(CCVars220.PartyMembersLimit, OnMembersLimitChanged, true);

        SubscribeNetMessage<CreatePartyRequestMessage>(OnCreatePartyRequest);
        SubscribeNetMessage<DisbandPartyRequestMessage>(OnDisbandPartyRequest);
        SubscribeNetMessage<LeavePartyRequestMessage>(OnLeavePartyMessage);
        SubscribeNetMessage<KickFromPartyRequestMessage>(OnKickFromPartyRequest);
        SubscribeNetMessage<SetPartySettingsRequestMessage>(OnSetSettingsRequest);
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        var connected = e.NewStatus is SessionStatus.Connected or SessionStatus.InGame;
        UpdateUserConnected(e.Session, connected);

        if (e.NewStatus is SessionStatus.Connected)
            UpdateClientInfo(e.Session);
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, ICommonSession sender)
    {
        TryCreateParty(sender, out var reason, settings: message.SettingsState);
    }

    private void OnDisbandPartyRequest(DisbandPartyRequestMessage message, ICommonSession sender)
    {
        var party = GetPartyByLeader(sender);
        if (party == null)
            return;

        DisbandParty(party);
    }

    private void OnLeavePartyMessage(LeavePartyRequestMessage message, ICommonSession sender)
    {
        var party = GetPartyByMember(sender);
        if (party == null)
            return;

        RemoveUserFromParty(sender, party);
    }

    private void OnKickFromPartyRequest(KickFromPartyRequestMessage message, ICommonSession sender)
    {
        var party = GetPartyByLeader(sender);
        if (party == null)
            return;

        var user = party.GetUserByPartyUserId(message.PartyUserId);
        if (user == null)
            return;

        RemoveUserFromParty(user.Value, party);
    }

    private void OnSetSettingsRequest(SetPartySettingsRequestMessage message, ICommonSession sender)
    {
        var party = GetPartyByLeader(sender);
        if (party == null)
            return;

        SetSettings(party, message.State);
    }

    private void UpdateUserConnected(ICommonSession session, bool connected)
    {
        if (!TryGetPartyByMember(session, out var party))
            return;

        var userInfo = party.GetUserInfo(session);
        if (userInfo == null)
            return;

        userInfo.Connected = connected;
        DirtyParty(party);
    }

    private void UpdateClientInfo(ICommonSession session)
    {
        var curParty = GetPartyByMember(session);
        SetCurrentParty(session, curParty);
        UpdateInvitesInfo(session);
    }

    /// <inheritdoc/>
    public bool TryCreateParty(ICommonSession leader, [NotNullWhen(false)] out string? reason, PartySettingsState? settings = null, bool force = false)
    {
        reason = null;

        try
        {
            CreateParty(leader, settings, force);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    /// <inheritdoc/>
    public ServerPartyData? CreateParty(ICommonSession leader, PartySettingsState? settings = null, bool force = false)
    {
        if (force)
            MakeUserAvaliable(leader.UserId);

        if (!IsUserAvaliable(leader))
            return null;

        var party = new ServerPartyData(GetFreePartyId());
        _parties.Add(party);

        settings ??= new PartySettingsState();
        SetSettings(party, settings.Value);

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

        PartyDisbanding?.Invoke(party);
        _parties.Remove(party);
    }

    public bool TryGetPartyById(uint id, [NotNullWhen(true)] out ServerPartyData? party)
    {
        party = GetPartyById(id);
        return party != null;
    }

    public bool TryGetPartyByLeader(ICommonSession leader, [NotNullWhen(true)] out ServerPartyData? party)
    {
        party = GetPartyByLeader(leader);
        return party != null;
    }

    public bool TryGetPartyByLeader(NetUserId leader, [NotNullWhen(true)] out ServerPartyData? party)
    {
        party = GetPartyByLeader(leader);
        return party != null;
    }

    public bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out ServerPartyData? party)
    {
        party = GetPartyByMember(member);
        return party != null;
    }

    public bool TryGetPartyByMember(NetUserId member, [NotNullWhen(true)] out ServerPartyData? party)
    {
        party = GetPartyByMember(member);
        return party != null;
    }

    public ServerPartyData? GetPartyById(uint id)
    {
        return _parties.Find(p => p.Id == id);
    }

    public ServerPartyData? GetPartyByLeader(ICommonSession leader)
    {
        return GetPartyByLeader(leader.UserId);
    }

    public ServerPartyData? GetPartyByLeader(NetUserId leader)
    {
        return _parties.Find(p => p.Leader == leader);
    }

    public ServerPartyData? GetPartyByMember(ICommonSession member)
    {
        return GetPartyByMember(member.UserId);
    }

    public ServerPartyData? GetPartyByMember(NetUserId member)
    {
        return _parties.Find(p => p.ContainsUser(member));
    }

    public bool TryAddUserToParty(NetUserId user, ServerPartyData party, [NotNullWhen(false)] out string? reason)
    {
        reason = null;
        return _playerManager.TryGetSessionById(user, out var session) &&
            TryAddUserToParty(session, party, out reason);
    }

    public bool TryAddUserToParty(ICommonSession user,
        ServerPartyData party,
        [NotNullWhen(false)] out string? reason,
        PartyRole role = PartyRole.Member,
        bool force = false)
    {
        reason = null;

        try
        {
            AddUserToParty(user, party, role, force);
            return true;
        }
        catch (Exception e)
        {
            reason = e.Message;
            return false;
        }
    }

    public void AddUserToParty(ICommonSession user, ServerPartyData party, PartyRole role = PartyRole.Member, bool force = false)
    {
        if (force)
            MakeUserAvaliable(user.UserId);

        if (!IsUserAvaliable(user))
            return;

        if (!party.AddMember(user, role, out var failReason, force))
            return;

        SetCurrentParty(user, party);
        DirtyParty(party);

        var chatMesage = Loc.GetString("partymanager-user-join-party-mesage", ("user", user.Name));
        ChatMessageToParty(chatMesage, party, PartyChatMessageType.Info);

        PartyDataUpdated?.Invoke(party);
    }

    public void RemoveUserFromParty(ICommonSession session, ServerPartyData party)
    {
        RemoveUserFromParty(session.UserId, party);
    }

    public void RemoveUserFromParty(NetUserId user, ServerPartyData party)
    {
        var userInfo = party.GetUserInfo(user);

        if (!party.RemoveMember(user))
            return;

        if (_playerManager.TryGetSessionById(user, out var session))
            SetCurrentParty(session, null);

        DirtyParty(party);

        if (userInfo != null)
        {
            var chatMessage = Loc.GetString("partymanager-user-left-party-message", ("user", userInfo.Name));
            ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);
        }

        PartyDataUpdated?.Invoke(party);
    }

    private void MakeUserAvaliable(NetUserId user)
    {
        if (!TryGetPartyByMember(user, out var party))
            return;

        if (party.Leader == user)
            DisbandParty(party);
        else
            RemoveUserFromParty(user, party);
    }

    private bool IsUserAvaliable(ICommonSession user, bool throwExeption = true)
    {
        return IsUserAvaliable(user.UserId, throwExeption);
    }

    private bool IsUserAvaliable(NetUserId user, bool throwExeption = true)
    {
        return !TryGetPartyByMember(user, out _);
    }

    private ClientPartyDataState GetClientPartyState(ServerPartyData party, ICommonSession client)
    {
        var localUserInfo = party.GetUserInfo(client);
        if (localUserInfo == null)
            throw new ArgumentException($"{client.Name} is not the member of this party");

        var membersList = party.Members.Select(x => x.Value).ToList();
        return new ClientPartyDataState(party.Id, localUserInfo, membersList, party.Settings.GetState(), party.Disbanded);
    }

    private void SetCurrentParty(ICommonSession session, ServerPartyData? party)
    {
        ClientPartyDataState? state = null;
        if (party != null)
            state = GetClientPartyState(party, session);

        SetCurrentParty(state, session);
    }

    private void DirtyParty(ServerPartyData party)
    {
        foreach (var (user, _) in party.Members)
        {
            if (!_playerManager.TryGetSessionById(user, out var session))
                continue;

            var state = GetClientPartyState(party, session);
            UpdatePartyData(state, session);
        }
    }

    private uint GetFreePartyId()
    {
        uint id = 1;
        while (GetPartyById(id) != null)
            id++;

        return id;
    }

    public void UpdatePartyData(ClientPartyDataState party, ICommonSession session)
    {
        var ev = new UpdateCurrentPartyMessage(party);
        SendNetMessage(ev, session);
    }

    public void SetCurrentParty(ClientPartyDataState? state, ICommonSession session)
    {
        var ev = new SetCurrentPartyMessage(state);
        SendNetMessage(ev, session);
    }

    private void SendNetMessage(PartyMessage message)
    {
        var msg = new PartyNetMessage
        {
            Message = message
        };

        _net.ServerSendToAll(msg);
    }

    private void SendNetMessage(PartyMessage message, ICommonSession session)
    {
        var msg = new PartyNetMessage
        {
            Message = message
        };

        _net.ServerSendMessage(msg, session.Channel);
    }

    #region Settings
    private uint _membersLimit = int.MaxValue;

    private void OnMembersLimitChanged(uint value)
    {
        _membersLimit = value;
        ResetSettings();
    }

    private void ResetSettings()
    {
        foreach (var party in _parties)
            ResetSettings(party);
    }

    private void ResetSettings(ServerPartyData party)
    {
        var state = party.Settings.GetState();
        SetSettings(party, state);
    }

    public void SetSettings(ServerPartyData party, PartySettingsState state)
    {
        var settings = party.Settings;
        settings.MaxMembers = Math.Clamp(state.MaxMembers, (uint)party.Members.Count, _membersLimit);
        DirtyParty(party);

        PartyDataUpdated?.Invoke(party);
    }
    #endregion
}

public sealed class ServerPartyData : SharedPartyData
{
    public NetUserId? Leader => GetLeader();
    public Dictionary<NetUserId, PartyUserInfo> Members = new();
    public PartySettings Settings;

    public ServerPartyData(uint id, PartySettings? settings = null) : base(id)
    {
        Settings = settings ?? new PartySettings();
    }

    public NetUserId? GetLeader()
    {
        foreach (var (session, info) in Members)
        {
            if (info.Role == PartyRole.Leader)
                return session;
        }

        return null;
    }

    public bool AddMember(ICommonSession session, PartyRole role, [NotNullWhen(false)] out string? failReason, bool force = false)
    {
        var connected = session.Status == SessionStatus.Connected || session.Status == SessionStatus.InGame;
        return AddMember(session.UserId, role, session.Name, connected, out failReason, force);
    }

    public bool AddMember(NetUserId userId, PartyRole role, string name, bool connected, [NotNullWhen(false)] out string? failReason, bool force = false)
    {
        failReason = null;
        if (ContainsUser(userId))
        {
            failReason = "User is already a member of the party";
            return false;
        }

        if (!force)
        {
            if (role == PartyRole.Leader && Leader != null)
            {
                failReason = "The party already has a leader";
                return false;
            }

            if (Members.Count >= Settings.MaxMembers)
            {
                failReason = "The party has reached the limit of members";
                return false;
            }
        }

        var userInfo = new PartyUserInfo(GetFreeUserId(), role, name, connected);
        Members.Add(userId, userInfo);
        return true;
    }

    public bool RemoveMember(ICommonSession session)
    {
        return RemoveMember(session.UserId);
    }

    public bool RemoveMember(NetUserId userId)
    {
        return Members.Remove(userId);
    }

    public PartyUserInfo? GetUserInfo(ICommonSession session)
    {
        return GetUserInfo(session.UserId);
    }

    public PartyUserInfo? GetUserInfo(NetUserId userId)
    {
        Members.TryGetValue(userId, out var userInfo);
        return userInfo;
    }

    public bool ContainsUser(ICommonSession session)
    {
        return ContainsUser(session.UserId);
    }

    public bool ContainsUser(NetUserId userId)
    {
        return Members.ContainsKey(userId);
    }

    public NetUserId? GetUserByPartyUserId(uint id)
    {
        return Members.Where(m => m.Value.Id == id)?.First().Key;
    }

    private uint GetFreeUserId()
    {
        uint id = 1;
        while (Members.Where(m => m.Value.Id == id).Any())
            id++;

        return id;
    }
}

public sealed class PartySettings
{
    public uint MaxMembers = 20;

    public PartySettingsState GetState()
    {
        return new PartySettingsState()
        {
            MaxMembers = MaxMembers
        };
    }
}
