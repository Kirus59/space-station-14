using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

[Serializable, NetSerializable]
public abstract class SharedPartyInvite(uint id, uint partyId, PartyInviteStatus status = PartyInviteStatus.None)
{
    public readonly uint Id = id;
    public readonly uint PartyId = partyId;
    public PartyInviteStatus Status = status;

    public override bool Equals(object? obj)
    {
        if (obj is not SharedPartyInvite other)
            return false;

        return Equals(other);
    }

    public bool Equals(SharedPartyInvite other)
    {
        if (ReferenceEquals(this, other))
            return true;

        return Id == other.Id;
    }

    public static bool Equals(SharedPartyInvite? invite1, SharedPartyInvite? invite2)
    {
        if (ReferenceEquals(invite1, invite2))
            return true;

        if (invite1 is null) return false;
        if (invite2 is null) return false;

        return invite1.Equals(invite2);
    }

    public static bool operator ==(SharedPartyInvite? left, SharedPartyInvite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SharedPartyInvite? left, SharedPartyInvite? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

[Serializable, NetSerializable]
public record struct InviteState(uint Id,
    uint partyId,
    NetUserId Sender,
    NetUserId Target,
    PartyInviteStatus Status,
    string SenderName = "unknown");

public enum PartyInviteStatus
{
    None,
    Created,
    Sended,

    Accepted,
    Denied,

    Deleted
}
