
using Robust.Shared.Network;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract class SharedPartyMember()
{
}

public record struct PartyMemberState(NetUserId Id, string Name, PartyMemberRole Role, bool Connected);

public enum PartyMemberRole : byte
{
    None = byte.MinValue,
    Member,

    Host = byte.MaxValue
}
