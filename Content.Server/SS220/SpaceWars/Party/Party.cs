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

    public void SetHost(ICommonSession session, bool ignoreLimit = false, bool throwException = false)
    {
        DebugTools.Assert(!_disposed);

        if (Host.Session == session)
            return;

        var oldHost = Host;
        if (!TryFindMember(session, out var newHost))
        {
            if (!ignoreLimit)
            {
                try
                {
                    CheckLimit(true);
                }
                catch (Exception e)
                {
                    if (throwException)
                        throw new Exception($"Failed to set {session.Name} as host of the party with id: {Id}, by reason: \"{e.Message}\"");
                }
            }

            newHost = new PartyMember(session, Id, PartyMemberRole.Host);
            _members.Add(newHost);
        }
        else
            newHost.Role = PartyMemberRole.Host;

        Host = newHost;
        oldHost.Role = PartyMemberRole.Member;
    }

    public bool AddMember(ICommonSession session, PartyMemberRole role, bool ignoreLimit = false, bool throwException = false)
    {
        DebugTools.Assert(!_disposed);

        if (ContainsMember(session))
        {
            TryThrow("User is already a member of the party");
            return false;
        }

        if (role is PartyMemberRole.Host)
        {
            TryThrow($"Cannot add member with the {PartyMemberRole.Host} role. Use the \"{nameof(SetHost)}\" function to set a new party host");
            return false;
        }

        if (!ignoreLimit)
        {
            try
            {
                CheckLimit(true);
            }
            catch (Exception e)
            {
                TryThrow(e.Message);
            }

            return false;
        }

        var member = new PartyMember(session, Id, role);
        return _members.Add(member);

        void TryThrow(string message)
        {
            if (!throwException)
                return;

            throw new Exception($"Failed to add {session.Name} to the party with id: \"{Id}\", by reason: \"{message}\"");
        }
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

    public bool RemoveMember(ICommonSession session, bool throwException = false)
    {
        DebugTools.Assert(!_disposed);

        if (!TryFindMember(session, out var member))
            return false;

        if (Host == member)
        {
            if (throwException)
                throw new Exception($"Failed to remove {session.Name} from party with id: \"{Id}\" by reason: " +
                    $"\"Cannot remove user with the {PartyMemberRole.Host} role. Use the \"{nameof(SetHost)}\" function to set a new party host " +
                    $"and then remove this user.\"");

            return false;
        }

        return _members.Remove(member);
    }

    public bool IsHost(ICommonSession session)
    {
        DebugTools.Assert(!_disposed);
        return Host.Session == session;
    }

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
        return new PartyState(Id, [.. _members.Select(x => x.GetState())], Settings.GetState(), Status);
    }

    private bool CheckLimit(bool throwException = false)
    {
        if (LimitReached && throwException)
            throw new Exception("The party has reached the limit of members");

        return LimitReached;
    }
}
