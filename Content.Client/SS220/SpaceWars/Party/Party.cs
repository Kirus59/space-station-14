// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Client.SS220.SpaceWars.Party;

[Access(typeof(PartyManager), Other = AccessPermissions.ReadExecute)]
public sealed class Party
{
    public readonly uint Id;

    public PartyMember Host;
    public PartyStatus Status;

    public IReadOnlyList<PartyMember> Members => [.. _members.Values];
    private Dictionary<NetUserId, PartyMember> _members = [];

    public IReadOnlyList<PartyInvite> Invites => [.. _invites.Values];
    private readonly Dictionary<uint, PartyInvite> _invites = [];

    public PartySettings Settings;

    public event Action<PartyMember>? HostChanged;
    public event Action<PartyMember>? HostUpdated;

    public event Action<PartyStatus>? StatusChanged;

    public event Action<PartyMember>? MemberAdded;
    public event Action<PartyMember>? MemberUpdated;
    public event Action<PartyMember>? MemberRemoved;

    public event Action<PartyInvite>? InviteAdded;
    public event Action<PartyInvite>? InviteUpdated;
    public event Action<PartyInvite>? InviteRemoved;

    public Party(PartyState state)
    {
        Id = state.Id;

        DebugTools.Assert(state.Host.Role is PartyMemberRole.Host);
        Host = new PartyMember(state.Host);
        Status = state.Status;

        _members = state.Members.ToDictionary(x => x.UserId, x => new PartyMember(x));
        _invites = state.Invites.ToDictionary(x => x.Id, x => new PartyInvite(x));
        Settings = state.Settings;
    }

    public bool TryFindMember(NetUserId userId, [NotNullWhen(true)] out PartyMember? member)
    {
        if (_members.TryGetValue(userId, out var exist))
        {
            member = exist;
            return true;
        }
        else
        {
            member = null;
            return false;
        }
    }

    public PartyMember? FindMember(NetUserId userId)
    {
        TryFindMember(userId, out var member);
        return member;
    }

    public bool IsHost(NetUserId userId)
    {
        return Host.UserId == userId;
    }

    [Access(typeof(PartyManager))]
    public void HandleState(PartyState state)
    {
        if (Id != state.Id)
            return;

        if (Host.UserId != state.Host.UserId)
        {
            DebugTools.Assert(state.Host.Role is PartyMemberRole.Host);
            Host = new PartyMember(state.Host);
            HostChanged?.Invoke(Host);
        }
        else
        {
            Host.HandleState(state.Host);
            HostUpdated?.Invoke(Host);
        }

        if (Status != state.Status)
        {
            Status = state.Status;
            StatusChanged?.Invoke(Status);
        }

        UpdateMembers(state.Members);
        UpdateInvites(state.Invites);

        Settings = state.Settings;
    }

    private void UpdateMembers(List<PartyMemberState> memberStates)
    {
        var toRemove = _members.Values.ToList();
        foreach (var state in memberStates)
        {
            var userId = state.UserId;
            if (_members.TryGetValue(userId, out var exist))
            {
                exist.HandleState(state);
                MemberUpdated?.Invoke(exist);
                toRemove.Remove(exist);
            }
            else
            {
                var member = new PartyMember(state);
                _members.Add(userId, member);
                MemberAdded?.Invoke(member);
            }
        }

        foreach (var member in toRemove)
        {
            if (_members.Remove(member.UserId))
                MemberRemoved?.Invoke(member);
        }

    }

    private void UpdateInvites(List<PartyInviteState> inviteStates)
    {
        var toRemove = _invites.Values.ToList();
        foreach (var state in inviteStates)
        {
            if (_invites.TryGetValue(state.Id, out var exist))
            {
                exist.HandleState(state);
                InviteUpdated?.Invoke(exist);
                toRemove.Remove(exist);
            }
            else
            {
                var invite = new PartyInvite(state);
                _invites.Add(state.Id, invite);
                InviteAdded?.Invoke(invite);
            }
        }

        foreach (var invite in toRemove)
        {
            if (_invites.Remove(invite.Id))
                InviteRemoved?.Invoke(invite);
        }
    }
}
