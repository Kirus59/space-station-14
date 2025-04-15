
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
public sealed class KickFromPartyRequestMessage(uint partyUserId) : EntityEventArgs
{
    public readonly uint PartyUserId = partyUserId;
}

[Serializable, NetSerializable]
public sealed class SetCurrentPartyMessage(ClientPartyDataState? state) : EntityEventArgs
{
    public readonly ClientPartyDataState? State = state;
}

[Serializable, NetSerializable]
public sealed class UpdateCurrentPartyMessage(ClientPartyDataState state) : EntityEventArgs
{
    public readonly ClientPartyDataState State = state;
}

[Serializable, NetSerializable]
public sealed class OpenPartyMenuMessage() : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed class ClosePartyMenuMessage() : EntityEventArgs { }

#region Invite
[Serializable, NetSerializable]
public sealed class CreatedNewInviteMessage(uint inviteId, string targetName, InviteStatus status) : EntityEventArgs
{
    public readonly uint InviteId = inviteId;
    public readonly string TargetName = targetName;
    public readonly InviteStatus Status = status;
}

[Serializable, NetSerializable]
public sealed class InviteReceivedMessage(uint inviteId, string senderName, InviteStatus status) : EntityEventArgs
{
    public readonly uint InviteId = inviteId;
    public readonly string SenderName = senderName;
    public readonly InviteStatus Status = status;
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
public sealed class DeleteInviteMessage(uint inviteId) : EntityEventArgs
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class InviteInPartyRequestMessage(string username) : EntityEventArgs
{
    public readonly string Username = username;
}

[Serializable, NetSerializable]
public sealed class InviteInPartyFailMessage(string reason) : EntityEventArgs
{
    public readonly string Reason = reason;
}

[Serializable, NetSerializable]
public sealed class GetInviteState() : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed class HandleInviteState(ClientPartyInviteState state) : EntityEventArgs
{
    public readonly ClientPartyInviteState State = state;
}
#endregion
