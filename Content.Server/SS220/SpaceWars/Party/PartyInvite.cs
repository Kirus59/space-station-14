
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party;

public interface IPartyInvite : ISharedPartyInvite
{
    Party Party { get; }
    ICommonSession Target { get; }
    void SetStatus(PartyInviteStatus newStatus);
    IPartyInviteState GetState();
}

public sealed class PartyInvite(uint id,
    Party party,
    ICommonSession target,
    PartyInviteStatus status = PartyInviteStatus.None) : IPartyInvite, IEquatable<PartyInvite>
{
    public uint Id { get; } = id;
    public PartyInviteStatus Status { get; private set; } = status;

    public Party Party { get; } = party;
    public ICommonSession Target { get; } = target;

    public void SetStatus(PartyInviteStatus newStatus)
    {
        Status = newStatus;
    }

    public IPartyInviteState GetState()
    {
        return new PartyInviteState(Id, Status, Party.Id, Target.UserId, Party.Host.Name, Target.Name);
    }

    public bool Equals(PartyInvite? other)
    {
        if (other is null)
            return false;

        return Id.Equals(other.Id);
    }

    public static bool Equals(PartyInvite? left, PartyInvite? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator ==(PartyInvite? left, PartyInvite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyInvite? left, PartyInvite? right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PartyInvite invite)
            return false;

        return Equals(invite);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
