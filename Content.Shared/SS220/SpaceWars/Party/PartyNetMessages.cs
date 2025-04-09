
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
public sealed class OpenPartyMenuMessage() : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed class ClosePartyMenuMessage() : EntityEventArgs { }

#region Invite
[Serializable, NetSerializable]
public sealed class CreatedNewInviteMessage(PartyInvite invite) : EntityEventArgs
{
    public readonly PartyInvite Invite = invite;
}

[Serializable, NetSerializable]
public sealed class InviteReceivedMessage(PartyInvite invite) : EntityEventArgs
{
    public readonly PartyInvite Invite = invite;
}

[Serializable, NetSerializable]
public sealed class AcceptInviteMessage(uint inviteId) : EntityEventArgs
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class DenyInviteMessage(uint inviteId) : EntityEventArgs
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class InviteInPartyRequestMessage(string username) : EntityEventArgs
{
    public readonly string Username = username;
}

[Serializable, NetSerializable]
public sealed class InviteInPartyAttemptResponceMessage(bool sended, string? message) : EntityEventArgs
{
    public readonly bool Sended = sended;
    public readonly string? Message = message;
}

[Serializable, NetSerializable]
public sealed class GetInviteState() : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed class HandleInviteState(PartyInviteState state) : EntityEventArgs
{
    public readonly PartyInviteState State = state;
}
#endregion
