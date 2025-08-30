
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed class Party : SharedParty, IDisposable
{
    public PartyMember Host;

    public IReadOnlyCollection<PartyMember> Members => _members;
    private readonly HashSet<PartyMember> _members = [];

    public PartySettings Settings;

    private bool _disposed;

    public Party(PartyState state) : base(state.Id)
    {
        DebugTools.Assert(state.Host.Role is PartyMemberRole.Host);
        Host = new PartyMember(state.Host.Id, state.Host.Name, PartyMemberRole.Host);

        foreach (var memberState in state.Members)
            AddMember(memberState.Id, memberState.Name);

        Settings = new PartySettings(state.Settings);
    }

    public bool AddMember(NetUserId userId, string username, PartyMemberRole role = PartyMemberRole.Member)
    {
        return AddMember(new PartyMember(userId, username, role));
    }

    public bool AddMember(PartyMember member)
    {
        if (_disposed)
            return false;

        return _members.Add(member);
    }

    public bool RemoveMember(NetUserId userId)
    {
        if (!TryFindMember(userId, out var member))
            return false;

        return RemoveMember(member);
    }

    public bool RemoveMember(PartyMember member)
    {
        if (_disposed)
            return false;

        return _members.Remove(member);
    }

    public bool TryFindMember(NetUserId userId, [NotNullWhen(true)] out PartyMember? member)
    {
        member = FindMember(userId);
        return member != null;
    }

    public PartyMember? FindMember(NetUserId userId)
    {
        if (_disposed)
            return null;

        var result = _members.Where(m => m.UserId == userId);
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        return result.First();
    }

    public void HandleState(PartyState state)
    {
        if (_disposed)
            return;

        DebugTools.Assert(state.Host.Role is PartyMemberRole.Host);
        Host = new PartyMember(state.Host.Id, state.Host.Name, PartyMemberRole.Host);

        foreach (var memberState in state.Members)
            AddMember(memberState.Id, memberState.Name);

        Settings = new PartySettings(state.Settings);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _members.Clear();
        _disposed = true;
    }

    public bool IsHost(NetUserId userId)
    {
        return Host.UserId == userId;
    }
}
