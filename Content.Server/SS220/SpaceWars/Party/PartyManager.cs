
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetManager _net = default!;

    public event Action<PartyStatusChangedActionArgs>? PartyStatusChanged;
    public event Action<Party>? PartyUpdated;

    public event Action<PartyMember>? UserJoinedParty;
    public event Action<PartyMember>? UserLeavedParty;

    public IReadOnlyCollection<Party> Parties => _parties;
    private readonly HashSet<Party> _parties = [];

    private uint _nextPartyId = 0;

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        _cfg.OnValueChanged(CCVars220.PartyMembersLimit, _ => ResetSettings(), true);

        SubscribeNetMessage<CreatePartyRequestMessage>(OnCreatePartyRequest);
        SubscribeNetMessage<DisbandPartyRequestMessage>(OnDisbandPartyRequest);
        SubscribeNetMessage<LeavePartyRequestMessage>(OnLeavePartyMessage);
        SubscribeNetMessage<KickFromPartyRequestMessage>(OnKickFromPartyRequest);
        SubscribeNetMessage<SetPartySettingsRequestMessage>(OnSetSettingsRequest);

        InviteInitialize();
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        var oldConnected = e.OldStatus is SessionStatus.Connected or SessionStatus.InGame;
        var newConnected = e.NewStatus is SessionStatus.Connected or SessionStatus.InGame;

        if (oldConnected == newConnected)
            return;

        if (TryGetPartyByMember(e.Session, out var party))
            UpdateClientParty(party, [e.Session]);

        UpdateClient(e.Session, party);
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, ICommonSession sender)
    {
        CreateParty(sender, message.SettingsState);
    }

    private void OnDisbandPartyRequest(DisbandPartyRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        DisbandParty(party);
    }

    private void OnLeavePartyMessage(LeavePartyRequestMessage message, ICommonSession sender)
    {
        EnsureNotPartyMember(sender);
    }

    private void OnKickFromPartyRequest(KickFromPartyRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        if (!_playerManager.TryGetSessionById(message.UserId, out var session))
            return;

        TryRemoveMember(party, session);
    }

    private void OnSetSettingsRequest(SetPartySettingsRequestMessage message, ICommonSession sender)
    {
        if (!TryGetPartyByHost(sender, out var party))
            return;

        SetSettings(party, message.State);
    }

    /// <inheritdoc/>
    public bool TryCreateParty(ICommonSession host, [NotNullWhen(true)] out Party? party, PartySettingsState? settings = null, bool force = false)
    {
        try
        {
            party = CreateParty(host, settings, force);
            return true;
        }
        catch
        {
            party = null;
            return false;
        }
    }

    /// <inheritdoc/>
    public Party CreateParty(ICommonSession host, PartySettingsState? settings = null, bool force = false)
    {
        if (force)
            EnsureNotPartyMember(host);
        else if (IsAnyPartyMember(host))
            throw new ArgumentException($"{host.Name} is already a member of another party.");

        var party = new Party(GeneratePartyId(), host);
        _parties.Add(party);

        DebugTools.Assert(PartyExist(party));
        DebugTools.Assert(party.IsHost(host));

        if (settings != null)
            SetSettings(party, settings.Value, false);

        SetStatus(party, PartyStatus.Running, false);

        UpdateClientParty(party);
        UserJoinedParty?.Invoke(party.Host);

        PartyUpdated?.Invoke(party);

        return party;
    }

    /// <inheritdoc/>
    public void DisbandParty(Party party, bool updates = true)
    {
        if (!PartyExist(party))
            return;

        SetStatus(party, PartyStatus.Disbanding, false);

        foreach (var member in party.Members)
        {
            UserLeavedParty?.Invoke(member);

            if (updates)
                UpdateClientParty(member.Session, null);
        }

        foreach (var invite in GetInvitesByParty(party))
            DeleteInvite(invite);

        _parties.Remove(party);
        SetStatus(party, PartyStatus.Disbanded, false);
        DebugTools.Assert(!PartyExist(party));

        party.Dispose();
    }

    public bool TryAddMember(Party party,
        ICommonSession session,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool ignoreLimit = false,
        bool updates = true)
    {
        try
        {
            AddMember(party, session, role, force, ignoreLimit, updates);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void AddMember(Party party,
        ICommonSession session,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool ignoreLimit = false,
        bool updates = true)
    {
        if (!PartyExist(party))
            throw new ArgumentException($"Party \"{party.Id}\" doesn't exist!");

        if (force)
            EnsureNotPartyMember(session);
        else if (IsAnyPartyMember(session))
            throw new Exception($"User {session.Name} is already a member of another party");

        var member = party.AddMember(session, role, ignoreLimit);
        DebugTools.Assert(party.ContainsMember(session));

        var chatMessage = Loc.GetString("party-manager-user-join-party-message", ("username", session.Name));
        ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);

        UserJoinedParty?.Invoke(member);

        if (updates)
        {
            UpdateClientParty(party);
            PartyUpdated?.Invoke(party);
        }

        DeleteInvite(party, session);
    }

    public bool TryRemoveMember(Party party, ICommonSession session, bool updates = true)
    {
        try
        {
            RemoveMember(party, session, updates);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void RemoveMember(Party party, ICommonSession session, bool updates = true)
    {
        if (!PartyExist(party))
            throw new ArgumentException($"Party \"{party.Id}\" doesn't exist!");

        if (party.IsHost(session))
            throw new ArgumentException($"\"Cannot remove user with the {PartyMemberRole.Host} role. " +
                $"Use the \"{nameof(SetHost)}\" function to set a new party host and then remove this user");

        if (!party.TryFindMember(session, out var member))
            return;

        party.RemoveMember(session);
        DebugTools.Assert(!party.ContainsMember(session));

        var chatMessage = Loc.GetString("party-manager-user-left-party-message", ("username", session.Name));
        ChatMessageToParty(chatMessage, party, PartyChatMessageType.Info);

        UserLeavedParty?.Invoke(member);

        if (updates)
        {
            UpdateClientParty(session, null);
            UpdateClientParty(party);

            PartyUpdated?.Invoke(party);
        }
    }

    public bool TrySetHost(Party party, ICommonSession session, bool force = false, bool updates = true)
    {
        try
        {
            SetHost(party, session, force, updates);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SetHost(Party party, ICommonSession session, bool force = false, bool updates = true)
    {
        if (!PartyExist(party))
            throw new ArgumentException($"Party \"{party.Id}\" doesn't exist!");

        if (party.IsHost(session))
            throw new ArgumentException($"User {session.Name} is already a host of this party");

        var isMember = party.ContainsMember(session);
        if (force)
            EnsureNotPartyMember(session);
        else if (!isMember && IsAnyPartyMember(session))
            throw new ArgumentException($"User {session.Name} is a member of another party");

        party.SetHost(session, ignoreLimit: force);
        DebugTools.Assert(party.IsHost(session));

        if (!isMember)
            UserJoinedParty?.Invoke(party.Host);

        if (updates)
        {
            UpdateClientParty(party);
            PartyUpdated?.Invoke(party);
        }

        DeleteInvite(party, session);
    }

    public void SetStatus(Party party, PartyStatus newStatus, bool updates = true)
    {
        var oldStatus = party.Status;
        if (oldStatus == newStatus)
            return;

        party.SetStatus(newStatus);
        DebugTools.Assert(party.Status == newStatus);

        var args = new PartyStatusChangedActionArgs(party.Id, oldStatus, newStatus);
        PartyStatusChanged?.Invoke(args);

        if (updates)
        {
            UpdateClientParty(party);
            PartyUpdated?.Invoke(party);
        }
    }

    public void EnsureNotPartyMember(ICommonSession session, bool updates = true)
    {
        if (!TryGetPartyByMember(session, out var party))
            return;

        if (party.IsHost(session))
            DisbandParty(party, updates);
        else
            RemoveMember(party, session, updates);

        DebugTools.Assert(!IsAnyPartyMember(session));
    }

    public bool IsAnyPartyMember(ICommonSession session)
    {
        return TryGetPartyByMember(session, out _);
    }

    public bool PartyExist(Party? party)
    {
        if (party == null)
            return false;

        var exist = _parties.Contains(party);
        if (exist)
            DebugTools.Assert(party.Status is not PartyStatus.Disbanding or PartyStatus.Disbanded);

        return exist;
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
        var result = _parties.Where(p => p.Id == id);
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        var party = result.First();

        DebugTools.Assert(PartyExist(party));
        return party;
    }

    public Party? GetPartyByHost(ICommonSession session)
    {
        var result = _parties.Where(p => p.IsHost(session));
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        var party = result.First();

        DebugTools.Assert(PartyExist(party));
        return party;
    }

    public Party? GetPartyByMember(ICommonSession session)
    {
        var result = _parties.Where(p => p.ContainsMember(session));
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        var party = result.First();

        DebugTools.Assert(PartyExist(party));
        return party;
    }

    private void UpdateClient(ICommonSession session)
    {
        UpdateClient(session, GetPartyByMember(session));
    }

    private void UpdateClient(ICommonSession session, Party? curParty)
    {
        UpdateClientParty(session, curParty);
        UpdateClientInvites(session);
    }

    private void UpdateClientParty(Party party, List<ICommonSession>? blacklist = null)
    {
        foreach (var member in party.Members)
        {
            if (blacklist != null && blacklist.Contains(member.Session))
                continue;

            UpdateClientParty(member.Session, party);
        }
    }

    private void UpdateClientParty(ICommonSession session)
    {
        UpdateClientParty(session, GetPartyByMember(session));
    }

    private void UpdateClientParty(ICommonSession session, Party? party)
    {
        DebugTools.Assert(party == GetPartyByMember(session));

        var ev = new UpdateClientPartyMessage(party?.GetState());
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
        party.Settings.HandleState(state);

        if (updates)
        {
            UpdateClientParty(party);
            PartyUpdated?.Invoke(party);
        }
    }
    #endregion
}
