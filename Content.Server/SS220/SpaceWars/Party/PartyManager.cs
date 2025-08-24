
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetManager _net = default!;

    public event Action<Party>? PartyStatusChanged;
    public event Action<Party>? PartyDataUpdated;

    public event Action<PartyMember>? UserJoinedParty;
    public event Action<PartyMember>? UserLeavedParty;

    public IReadOnlyList<Party> Parties => _parties;
    private readonly List<Party> _parties = [];

    private uint _nextPartyId = 0;

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
        var oldConnected = e.OldStatus is not SessionStatus.Connected or SessionStatus.InGame;
        var newConnected = e.NewStatus is SessionStatus.Connected or SessionStatus.InGame;

        if (oldConnected == newConnected)
            return;

        if (TryGetPartyByMember(e.Session, out var party))
            UpdateClientPartyData(party);

        if (newConnected)
            UpdateClientInfo(e.Session);
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, ICommonSession sender)
    {
        CreateParty(sender);
    }

    private void OnDisbandPartyRequest(DisbandPartyRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        DisbandParty(party);
    }

    private void OnLeavePartyMessage(LeavePartyRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByMember(sender, out var party))
            return;

        RemoveUserFromParty(sender, party);
    }

    private void OnKickFromPartyRequest(KickFromPartyRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        if (!_playerManager.TryGetSessionById(message.UserId, out var session))
            return;

        RemoveUserFromParty(session, party);
    }

    private void OnSetSettingsRequest(SetPartySettingsRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        SetSettings(party, message.State);
    }

    private void UpdateClientInfo(ICommonSession session)
    {
        var curParty = GetPartyByMember(session);
        UpdateClientPartyData(curParty, session);
        UpdateInvitesInfo(session);
    }

    /// <inheritdoc/>
    public bool TryCreateParty(ICommonSession host, [NotNullWhen(true)] out Party? party, PartySettingsState? settings = null, bool force = false)
    {
        party = CreateParty(host, settings, force);
        return party != null;
    }

    /// <inheritdoc/>
    public Party? CreateParty(ICommonSession host, PartySettingsState? settings = null, bool force = false)
    {
        if (IsAnyPartyMember(host))
        {
            if (force)
                EnsureNotPartyMember(host);
            else
                return null;
        }

        var party = new Party(GeneratePartyId(), host);
        _parties.Add(party);

        if (settings != null)
            SetSettings(party, settings.Value, updates: false);

        UpdateClientPartyData(party);
        PartyStatusChanged?.Invoke(party);
        UserJoinedParty?.Invoke(party.Host);
        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(Party party, bool updates = true)
    {
        UpdateClientPartyData(party);

        foreach (var member in party.Members)
        {
            UserLeavedParty?.Invoke(member);

            if (updates)
                UpdateClientPartyData(null, member.Session);
        }

        party.Disband();

        PartyStatusChanged?.Invoke(party);
        _parties.Remove(party);
    }

    public bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyById(id);
        return party != null;
    }

    public bool TryGetPartyByHost(ICommonSession host, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByHost(host);
        return party != null;
    }

    public bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party)
    {
        party = GetPartyByMember(member);
        return party != null;
    }

    public Party? GetPartyById(uint id)
    {
        return _parties.Find(p => p.Id == id);
    }

    public Party? GetPartyByHost(ICommonSession session)
    {
        return _parties.Find(p => p.IsHost(session));
    }

    public Party? GetPartyByMember(ICommonSession session)
    {
        return _parties.Find(p => p.ContainsMember(session));
    }

    public bool AddUserToParty(ICommonSession session,
        Party party,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool updates = true,
        bool throwException = false)
    {
        if (IsAnyPartyMember(session))
        {
            if (force)
                EnsureNotPartyMember(session);
            else
            {
                if (throwException)
                    throw new Exception($"User {session.Name} is already a member of another party");

                return false;
            }
        }

        if (!party.AddMember(session, role, throwException, force))
            return false;

        if (updates)
        {
            UpdateClientPartyData(party);
            PartyDataUpdated?.Invoke(party);
        }

        var chatMesage = Loc.GetString("partymanager-user-join-party-mesage", ("user", session.Name));
        ChatMessageToParty(chatMesage, party, PartyChatMessageType.Info);

        return true;
    }

    public bool RemoveUserFromParty(ICommonSession session, Party party, bool throwException = false, bool updates = true)
    {
        if (party.IsHost(session))
        {
            if (throwException)
                throw new Exception($"\"Cannot remove user with the {PartyMemberRole.Host} role. " +
                    $"Use the \"{nameof(SetNewHost)}\" function to set a new party host and then remove this user");

            return false;
        }

        if (!party.TryFindMember(session, out var member))
            return false;

        if (!party.RemoveMember(session, true))
            return false;

        if (updates)
        {
            UpdateClientPartyData(null, session);
            UpdateClientPartyData(party);
        }

        UserLeavedParty?.Invoke(member);
        return true;
    }

    public bool SetNewHost(ICommonSession session, Party party, bool force = false, bool throwException = false, bool updates = true)
    {
        if (party.IsHost(session))
        {
            if (throwException)
                throw new Exception($"User {session.Name} is already a host of this party");

            return false;
        }

        var isMember = party.ContainsMember(session);
        if (!isMember && IsAnyPartyMember(session))
        {
            if (force)
                EnsureNotPartyMember(session);
            else
            {
                if (throwException)
                    throw new Exception($"User {session.Name} is already a member of another party");

                return false;
            }
        }

        party.SetHost(session);

        if (!isMember)
            UserJoinedParty?.Invoke(party.Host);

        if (updates)
        {
            UpdateClientPartyData(party);

            PartyDataUpdated?.Invoke(party);
        }

        return true;
    }

    public void EnsureNotPartyMember(ICommonSession session, bool updates = true)
    {
        if (!TryGetPartyByMember(session, out var party))
            return;

        if (party.IsHost(session))
            DisbandParty(party, updates);
        else
            RemoveUserFromParty(session, party, updates);
    }

    public bool IsAnyPartyMember(ICommonSession session)
    {
        return TryGetPartyByMember(session, out _);
    }

    private void UpdateClientPartyData(Party party)
    {
        foreach (var member in party.Members)
            UpdateClientPartyData(party, member.Session);
    }

    private void UpdateClientPartyData(Party? party, ICommonSession session)
    {
        var ev = new UpdatePartyDataMessage(party?.GetState());
        SendNetMessage(ev, session);
    }

    private uint GeneratePartyId()
    {
        return _nextPartyId++;
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

    public void SetSettings(Party party, PartySettingsState state, bool updates = true)
    {
        var settings = party.Settings;
        settings.MaxMembers = Math.Clamp(state.MaxMembers, party.Members.Count, _membersLimit);

        if (updates)
        {
            PartyDataUpdated?.Invoke(party);
            UpdateClientPartyData(party);
        }
    }
    #endregion
}
