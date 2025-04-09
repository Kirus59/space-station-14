
using Pidgin.Expression;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract partial class SharedPartyManager : ISharedPartyManager
{
    protected ISawmill _sawmill = default!;

    public virtual void Initialize()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = Logger.GetSawmill("PartyManager");
    }
}

[Serializable, NetSerializable]
[Access(typeof(SharedPartyManager), Other = AccessPermissions.ReadExecute)]
public sealed class PartyData
{
    public PartyUser? Leader => GetLeader();

    public List<PartyUser> Members = new();

    public bool Disbanded = false;

    public PartyData() { }

    public PartyData(List<PartyUser> players)
    {
        Members = players;
    }

    public PartyUser? GetLeader()
    {
        return Members.Find(u => u.Role == PartyRole.Leader);
    }

    public bool IsLeader(NetUserId user)
    {
        return Leader?.Id == user;
    }

    public bool IsLeader(PartyUser user)
    {
        return Leader == user;
    }

    public bool ContainUser(PartyUser user)
    {
        return Members.Find(u => u == user) != null;
    }

    public bool TryGetMember(NetUserId netUser, [NotNullWhen(true)] out PartyUser? partyUser)
    {
        partyUser = Members.Find(u => u.Id == netUser);
        return partyUser != null;
    }

    [Access(typeof(SharedPartyManager))]
    public void AddMember(PartyUser user)
    {
        if (Members.Contains(user))
            throw new ArgumentException($"{user.Name} is currently added in this party");

        Members.Add(user);
    }

    [Access(typeof(SharedPartyManager))]
    public void RemoveMember(PartyUser user)
    {
        Members.Remove(user);
    }
}

[Serializable, NetSerializable]
[Access(typeof(SharedPartyManager), Other = AccessPermissions.Read)]
public sealed class PartyUser
{
    public readonly NetUserId Id;

    public PartyRole Role;
    public string Name;
    public bool Connected;

    public PartyUser(NetUserId userId, PartyRole role, string name, bool connected)
    {
        Id = userId;
        Role = role;
        Name = name;
        Connected = connected;
    }

    public bool Equals(PartyUser user)
    {
        return Id == user.Id;
    }

    public static bool Equals(PartyUser? user1, PartyUser? user2)
    {
        if (user1 is null) return user2 is null;
        if (user2 is null) return user1 is null;

        return user1.Equals(user2);
    }

    public static bool operator ==(PartyUser? left, PartyUser? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyUser? left, PartyUser? right)
    {
        return !Equals(left, right);
    }
}

[Serializable, NetSerializable]
public sealed class PartyInvite
{
    public readonly uint Id;
    public readonly PartyUser Sender;
    public readonly PartyUser Target;
    public readonly PartyData Party;

    public InviteStatus Status;

    public PartyInvite(uint id, PartyUser sender, PartyUser target, PartyData party, InviteStatus status = InviteStatus.None)
    {
        Id = id;
        Sender = sender;
        Target = target;
        Party = party;
        Status = status;
    }

    public bool Equals(PartyInvite invite)
    {
        return Id == invite.Id;
    }

    public static bool Equals(PartyInvite? invite1, PartyInvite? invite2)
    {
        if (invite1 is null) return invite2 is null;
        if (invite2 is null) return invite1 is null;

        return invite1.Equals(invite2);
    }

    public static bool operator ==(PartyInvite? left, PartyInvite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyInvite? left, PartyInvite? right)
    {
        return !Equals(left, right);
    }
}

public enum PartyRole
{
    Member,
    Leader
}

public enum InviteStatus
{
    None,
    Error,
    Deleted,
    TargetNotFound,
    TargetIsSender,
    Sended,

    Accepted,
    Denied
}

[Serializable, NetSerializable]
public record struct PartyInviteState(uint Id, InviteStatus Status);
