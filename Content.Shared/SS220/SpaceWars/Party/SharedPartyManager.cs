
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

    public bool IsLeader(NetUserId userId)
    {
        return Leader?.Id == userId;
    }

    public bool ContainMember(NetUserId userId)
    {
        return Members.Find(u => u.Id == userId) != null;
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
    public NetUserId Id;
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
}

public enum PartyRole
{
    Member,
    Leader
}
