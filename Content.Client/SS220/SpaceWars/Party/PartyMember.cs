
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.SS220.SpaceWars.Party;
public sealed class PartyMember(PartyMemberState state) : SharedPartyMember(state.Role), IEquatable<PartyMember>
{
    public readonly NetUserId UserId = state.UserId;

    public string Username = state.Username;
    public bool Connected = state.Connected;

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

        return UserId == other.UserId;
    }

    public static bool Equals(PartyMember? member1, PartyMember? member2)
    {
        if (ReferenceEquals(member1, member2))
            return true;

        if (member1 is null) return false;

        return member1.Equals(member2);
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
        return UserId.GetHashCode();
    }

    public void HandleState(PartyMemberState state)
    {
        DebugTools.AssertEqual(state.UserId, UserId);

        Username = state.Username;
        Connected = state.Connected;
        Role = state.Role;
    }
}
