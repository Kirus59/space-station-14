using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using Robust.Shared.Enums;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class PartyMember(ICommonSession session, PartyMemberRole role) : SharedPartyMember(), IEquatable<PartyMember>
{
    public ICommonSession Session = session;
    public PartyMemberRole Role = role;

    public PartyMemberState GetState()
    {
        var connected = Session.Status is SessionStatus.Connected or SessionStatus.InGame;
        return new PartyMemberState(Session.UserId, Session.Name, Role, connected);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PartyMember other)
            return false;

        return Equals(other);
    }

    public bool Equals(PartyMember? other)
    {
        if (other is null)
            return false;

        return Session == other.Session;
    }

    public static bool Equals(PartyMember? left, PartyMember? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator ==(PartyMember? left, PartyMember? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyMember? left, PartyMember? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return Session.GetHashCode();
    }
}
