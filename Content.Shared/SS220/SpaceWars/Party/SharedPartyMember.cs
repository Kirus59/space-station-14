
using Robust.Shared.Network;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract class SharedPartyMember(PartyMemberRole role)
{
    public PartyMemberRole Role = role;

    public static string GetPartyMemberRoleName(PartyMemberRole role)
    {
        return Loc.GetString($"party-member-role-{role.ToString().ToLower()}");
    }
}

public record struct PartyMemberState(NetUserId UserId, string Username, PartyMemberRole Role, bool Connected);

public enum PartyMemberRole : byte
{
    /// <summary>
    /// Default role of the party member
    /// </summary>
    Member,

    Host = byte.MaxValue
}
