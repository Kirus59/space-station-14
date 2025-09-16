using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

public interface ISharedPartyInvite
{
    uint Id { get; }
    PartyInviteStatus Status { get; }

    static string GetPartyInviteStatusName(PartyInviteStatus status)
    {
        return Loc.GetString($"party-invite-status-{status.ToString().ToLower()}");
    }
}

public interface IPartyInviteState
{
}

[Serializable, NetSerializable]
public record struct PartyInviteState(uint Id,
    PartyInviteStatus Status,
    uint PartyId,
    NetUserId Target,
    string SenderName,
    string TargetName) : IPartyInviteState;

[Serializable, NetSerializable]
public enum PartyInviteStatus : byte
{
    None,
    Created,
    Sended,

    Accepted,
    Denied,

    Deleted
}
