using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class Party : SharedParty, IDisposable
{
    public PartyMember Host { get; private set; }

    public IReadOnlyCollection<PartyMember> Members => _members;
    private readonly HashSet<PartyMember> _members = [];

    public PartySettings Settings;
    public bool LimitReached => Members.Count >= Settings.MembersLimit;

    private bool _disposed;

    public Party(uint id, ICommonSession host, PartySettingsState? settingsState = null) : base(id)
    {
        Host = new PartyMember(host, Id, PartyMemberRole.Host);
        _members.Add(Host);

        Settings = new(this);
        if (settingsState is { } state)
            Settings.HandleState(state);
    }

    [Access(typeof(PartyManager))]
    public PartyMember SetHost(ICommonSession session, bool ignoreLimit = false)
    {
        DebugTools.Assert(!_disposed);

        if (Host.Session == session)
            return Host;

        var oldHost = Host;
        if (!TryFindMember(session, out var newHost))
        {
            if (!ignoreLimit)
                LimitCheckout(true);

            newHost = new PartyMember(session, Id, PartyMemberRole.Host);
            _members.Add(newHost);
        }
        else
            newHost.Role = PartyMemberRole.Host;

        Host = newHost;
        oldHost.Role = PartyMemberRole.Member;

        return newHost;
    }

    [Access(typeof(PartyManager))]
    public PartyMember AddMember(ICommonSession session, PartyMemberRole role, bool ignoreLimit = false)
    {
        DebugTools.Assert(!_disposed);

        if (TryFindMember(session, out var member))
            return member;

        if (role is PartyMemberRole.Host)
            throw new ArgumentException($"Cannot add member with the {PartyMemberRole.Host} role. " +
                $"Use the \"{nameof(SetHost)}\" function to set a new party host");

        if (!ignoreLimit)
            LimitCheckout(true);

        member = new PartyMember(session, Id, role);
        _members.Add(member);

        return member;
    }

    public bool ContainsMember(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);
        return TryFindMember(session, out _);
    }

    public bool TryFindMember(ICommonSession session, [NotNullWhen(true)] out PartyMember? member)
    {
        DebugTools.Assert(!_disposed);

        var sorted = _members.Where(x => x.Session == session);
        var count = sorted.Count();
        if (count <= 0)
        {
            member = null;
            return false;
        }

        DebugTools.Assert(sorted.Count() == 1, $"The user with the nickname {session.Name} is listed more than once in the party members. Party_id: \"{Id}\"");
        member = sorted.First();
        return true;
    }

    [Access(typeof(PartyManager))]
    public void RemoveMember(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);

        if (!ContainsMember(session))
            return;

        if (Host.Session == session)
            throw new Exception($"\"Cannot remove user with the {PartyMemberRole.Host} role. " +
                $"Use the \"{nameof(SetHost)}\" function to set a new party host and then remove this user.\"");
    }

    public bool IsHost(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);
        return Host.Session == session;
    }

    [Access(typeof(PartyManager))]
    public void Dispose()
    {
        if (_disposed)
            return;

        _members.Clear();
        _disposed = true;
    }

    public PartyState GetState()
    {
        DebugTools.Assert(!_disposed);
        return new PartyState(Id, Host.GetState(), [.. _members.Select(x => x.GetState())], Settings.GetState(), Status);
    }

    private bool LimitCheckout(bool throwException = false)
    {
        if (LimitReached && throwException)
            throw new Exception("The party has reached the limit of members!");

        return LimitReached;
    }
}
