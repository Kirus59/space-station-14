
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetManager _net = default!;

    public event Action<Party>? PartyDataUpdated;
    public event Action<Party>? PartyDisbanding;

    public IReadOnlyList<Party> Parties => _parties;
    private readonly List<Party> _parties = [];

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

        InviteInitialize();
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
        TryCreateParty(sender, out var _, settings: message.SettingsState);
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
        UpdateClientPartyData(party);
    }

    private void UpdateClientInfo(ICommonSession session)
    {
        var curParty = GetPartyByMember(session);
        UpdateClientPartyData(curParty, session);
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
    public Party? CreateParty(ICommonSession leader, PartySettingsState? settings = null, bool force = false)
    {
        var party = new Party(GetFreePartyId());
        _parties.Add(party);

        settings ??= new PartySettingsState();
        SetSettings(party, settings.Value);

        if (!TryAddUserToParty(leader, party, out _, PartyMemberRole.Leader, force))
        {
            DisbandParty(party);
            return null;
        }

        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(Party party)
    {
        var usersToRemove = party.Members.Keys;
        foreach (var user in usersToRemove)
            RemoveUserFromParty(user, party);

        party.Disbanded = true;
        UpdateClientPartyData(party);

        PartyDisbanding?.Invoke(party);
        _parties.Remove(party);
    }

    public bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyById(id);
        return party != null;
    }

    public bool TryGetPartyByLeader(ICommonSession leader, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByLeader(leader);
        return party != null;
    }

    public bool TryGetPartyByLeader(NetUserId leader, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByLeader(leader);
        return party != null;
    }

    public bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByMember(member);
        return party != null;
    }

    public bool TryGetPartyByMember(NetUserId member, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByMember(member);
        return party != null;
    }

    public Party? GetPartyById(uint id)
    {
        return _parties.Find(p => p.Id == id);
    }

    public Party? GetPartyByLeader(ICommonSession leader)
    {
        return GetPartyByLeader(leader.UserId);
    }

    public Party? GetPartyByLeader(NetUserId leader)
    {
        return _parties.Find(p => p.Leader == leader);
    }

    public Party? GetPartyByMember(ICommonSession member)
    {
        return GetPartyByMember(member.UserId);
    }

    public Party? GetPartyByMember(NetUserId member)
    {
        return _parties.Find(p => p.ContainsUser(member));
    }

    public bool TryAddUserToParty(NetUserId user,
        Party party,
        [NotNullWhen(false)] out string? reason,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false)
    {
        if (!_playerManager.TryGetSessionById(user, out var session))
        {
            reason = Loc.GetString("party-manager-add-user-to-party-fail-reason-session-not-found", ("userId", user));
            return false;
        }

        return TryAddUserToParty(session, party, out reason, role, force);
    }

    public bool TryAddUserToParty(ICommonSession session,
        Party party,
        [NotNullWhen(false)] out string? reason,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false)
    {
        switch (IsAnyPartyMember(session))
        {
            case false when force:
                EnsureNotPartyMember(session);
                break;

            case false:
                reason = Loc.GetString("party-manager-add-user-to-party-fail-reason-user-not-available", ("userName", session.Name));
                return false;
        }

        if (!party.AddMember(session, role, out reason, force))
            return false;

        UpdateClientPartyData(party);

        var chatMesage = Loc.GetString("partymanager-user-join-party-mesage", ("user", session.Name));
        ChatMessageToParty(chatMesage, party, PartyChatMessageType.Info);

        PartyDataUpdated?.Invoke(party);
        return true;
    }

    public void RemoveUserFromParty(ICommonSession session, Party party)
    {
        RemoveUserFromParty(session.UserId, party);
    }

    public void RemoveUserFromParty(NetUserId user, Party party)
    {
        var userInfo = party.GetUserInfo(user);

        if (!party.RemoveMember(user))
            return;

        if (_playerManager.TryGetSessionById(user, out var session))
            UpdateClientPartyData(null, session);

        UpdateClientPartyData(party);

        if (userInfo != null)
        {
            var chatMessage = Loc.GetString("partymanager-user-left-party-message", ("user", userInfo.Name));
            ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);
        }

        PartyDataUpdated?.Invoke(party);
    }

    private void EnsureNotPartyMember(ICommonSession session)
    {
        EnsureNotPartyMember(session.UserId);
    }

    private void EnsureNotPartyMember(NetUserId user)
    {
        if (!TryGetPartyByMember(user, out var party))
            return;

        if (party.Leader == user)
            DisbandParty(party);
        else
            RemoveUserFromParty(user, party);
    }

    private bool IsAnyPartyMember(ICommonSession session)
    {
        return IsAnyPartyMember(session.UserId);
    }

    private bool IsAnyPartyMember(NetUserId user)
    {
        return !TryGetPartyByMember(user, out _);
    }

    private void UpdateClientPartyData(Party party)
    {
        foreach (var userId in party.Members.Keys)
            UpdateClientPartyData(party, userId);
    }

    private void UpdateClientPartyData(Party? party, NetUserId userId)
    {
        ICommonSession? session = null;
        try
        {
            session = _playerManager.GetSessionById(userId);
        }
        catch (Exception e)
        {
            DebugTools.Assert(e.Message);
        }

        if (session is null)
            return;

        UpdateClientPartyData(party, session);
    }

    private void UpdateClientPartyData(Party? party, ICommonSession session)
    {
        if (party is null || !party.TryGetClientState(session, out var state))
            state = null;

        var ev = new UpdatePartyDataMessage(state);
        SendNetMessage(ev, session);
    }

    private uint GetFreePartyId()
    {
        uint id = 1;
        while (GetPartyById(id) != null)
            id++;

        return id;
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
    private int _membersLimit = int.MaxValue;

    private void OnMembersLimitChanged(int value)
    {
        _membersLimit = value;
        ResetSettings();
    }

    private void ResetSettings()
    {
        foreach (var party in _parties)
            ResetSettings(party);
    }

    private void ResetSettings(Party party)
    {
        var state = party.Settings.GetState();
        SetSettings(party, state);
    }

    public void SetSettings(Party party, PartySettingsState state)
    {
        var settings = party.Settings;
        settings.MaxMembers = Math.Clamp(state.MaxMembers, party.Members.Count, _membersLimit);
        UpdateClientPartyData(party);

        PartyDataUpdated?.Invoke(party);
    }
    #endregion
}
