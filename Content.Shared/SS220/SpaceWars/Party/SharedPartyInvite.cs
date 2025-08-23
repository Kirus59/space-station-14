using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

[Serializable, NetSerializable]
public abstract class SharedPartyInvite(uint id, InviteStatus status = InviteStatus.None)
{
    public readonly uint Id = id;
    public InviteStatus Status = status;

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool Equals(SharedPartyInvite? invite1, SharedPartyInvite? invite2)
    {
        if (ReferenceEquals(invite1, invite2))
            return true;

        if (invite1 is null)
            return false;

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
}

public enum InviteStatus
{
    None,
    Deleted,
    Sended,

    Accepted,
    Denied
}

[Serializable, NetSerializable]
public record struct SendedInviteState(uint Id, string TargetName, InviteStatus Status);

[Serializable, NetSerializable]
public record struct IncomingInviteState(uint Id, string SenderName, InviteStatus Status);
