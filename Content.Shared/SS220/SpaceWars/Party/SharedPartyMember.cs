
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract class SharedPartyMember(PartyMemberRole role)
{
    public PartyMemberRole Role = role;

    public static string GetPartyMemberRoleName(PartyMemberRole role)
    {
        return Loc.GetString($"party-member-role-{role.ToString().ToLower()}");
    }
}

[Serializable, NetSerializable]
public record struct PartyMemberState(NetUserId UserId, string Username, PartyMemberRole Role, bool Connected);

[Serializable, NetSerializable]
public enum PartyMemberRole : byte
{
    /// <summary>
    /// Default role of the party member
    /// </summary>
    Member,

    Host = byte.MaxValue
}
