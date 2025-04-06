
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

[Serializable, NetSerializable]
public sealed class CreatePartyRequestMessage() : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class DisbandPartyRequestMessage() : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class LeavePartyRequestMessage() : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class InviteInPartyRequestMessage(string username) : EntityEventArgs
{
    public readonly string Username = username;
}

[Serializable, NetSerializable]
public sealed class CreatePartyResponceMessage(bool isCreated, string? reason = null) : EntityEventArgs
{
    public readonly bool IsCreated = isCreated;
    public readonly string? Reason = reason;
}

[Serializable, NetSerializable]
public sealed class PartyDataInfoRequestMessage() : EntityEventArgs
{
}

[Serializable, NetSerializable]
public sealed class UpdatePartyDataInfoMessage(PartyData? partyData) : EntityEventArgs
{
    public readonly PartyData? PartyData = partyData;
}

[Serializable, NetSerializable]
public sealed class AcceptInviteMessage(PartyInvite invite) : EntityEventArgs
{
    public readonly PartyInvite Invite = invite;
}

[Serializable, NetSerializable]
public sealed class DenyInviteMessage(PartyInvite invite) : EntityEventArgs
{
    public readonly PartyInvite Invite = invite;
}

[Serializable, NetSerializable]
public sealed class OpenPartyMenuMessage() : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed class ClosePartyMenuMessage() : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed class UpdateInviteInfo(PartyInvite invite) : EntityEventArgs
{
    public readonly PartyInvite Invite = invite;
}
