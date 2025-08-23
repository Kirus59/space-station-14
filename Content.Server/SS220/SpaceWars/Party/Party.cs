using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class Party : SharedParty
{
    public PartyMember Host => _host;
    private PartyMember _host;

    public IReadOnlyCollection<PartyMember> Members => _members;
    private readonly HashSet<PartyMember> _members = [];

    public PartySettings Settings => _settings;
    private PartySettings _settings;

    public Party(uint id, ICommonSession host, PartySettings? settings = null) : base(id)
    {
        _host = new PartyMember(host, PartyMemberRole.Host);
        _members.Add(_host);
        _settings = settings ?? new PartySettings();
    }

    public void SetHost(ICommonSession session)
    {
        if (_host.Session == session)
            return;

        var oldHost = _host;
        if (!TryFindMember(session, out var newHost))
        {
            newHost = new PartyMember(session, PartyMemberRole.Host);
            _members.Add(newHost);
        }
        else
            newHost.Role = PartyMemberRole.Host;

        _host = newHost;
        oldHost.Role = PartyMemberRole.Member;
    }

    public bool AddMember(ICommonSession session, PartyMemberRole role, bool throwException = false, bool forceLimit = false)
    {
        if (ContainsMember(session))
        {
            TryThrow("User is already a member of the party");
            return false;
        }

        if (role is PartyMemberRole.Host)
        {
            TryThrow($"Cannot add user with the {PartyMemberRole.Host} role. Use the \"{nameof(SetHost)}\" function to set a new party host");
            return false;
        }

        if (!forceLimit && Members.Count >= Settings.MaxMembers)
        {
            TryThrow("The party has reached the limit of members");
            return false;
        }

        var member = new PartyMember(session, role);
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
        return TryFindMember(session, out _);
    }

    public bool TryFindMember(ICommonSession session, [NotNullWhen(true)] out PartyMember? member)
    {
        var sorted = _members.Where(x => x.Session == session);
        var count = sorted.Count();
        if (count <= 0)
        {
            member = null;
            return false;
        }

        DebugTools.Assert(sorted.Count() == 1, $"Пользователь с ником {session.Name} дважды записан в участниках пати. Party_id: \"{Id}\"");
        member = sorted.First();
        return true;
    }

    public bool RemoveMember(ICommonSession session, bool throwException = false)
    {
        if (!TryFindMember(session, out var member))
            return false;

        if (_host == member)
        {
            if (throwException)
                throw new Exception($"Failed to remove {session.Name} from party with id: \"{Id}\" by reason: " +
                    $"\"Cannot remove user with the {PartyMemberRole.Host} role. Use the \"{nameof(SetHost)}\" function to set a new party host " +
                    $"and then remove this user.\"");

            return false;
        }

        return _members.Remove(member);
    }

    public PartyState GetState()
    {
        return new PartyState(Id, member, [.. Members.Select(x => x.Value)], Settings.GetState(), Disbanded);
    }
}
