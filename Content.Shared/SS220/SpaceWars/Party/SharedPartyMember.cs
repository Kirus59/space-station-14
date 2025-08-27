
using Robust.Shared.Network;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract class SharedPartyMember(PartyMemberRole role)
{
    public PartyMemberRole Role = role;
}

public record struct PartyMemberState(NetUserId Id, string Name, PartyMemberRole Role, bool Connected);

public enum PartyMemberRole : byte
{
    /// <summary>
    /// Default role of the party member
    /// </summary>
    Member,

    Host = byte.MaxValue
}
