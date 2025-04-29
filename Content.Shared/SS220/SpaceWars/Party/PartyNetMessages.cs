
using Content.Shared.Store;
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
public sealed class CreatedNewInviteMessage(SendedInviteState state) : EntityEventArgs
{
    public readonly SendedInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class InviteReceivedMessage(IncomingInviteState state) : EntityEventArgs
{
    public readonly IncomingInviteState State = state;
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
public sealed class UpdateSendedInviteMessage(SendedInviteState state) : EntityEventArgs
{
    public readonly SendedInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class UpdateIncomingInviteMessage(IncomingInviteState state) : EntityEventArgs
{
    public readonly IncomingInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class UpdateInvitesInfoMessage(List<SendedInviteState> sendedInvites, List<IncomingInviteState> incomingInvites) : EntityEventArgs
{
    public readonly List<SendedInviteState> SendedInvites = sendedInvites;
    public readonly List<IncomingInviteState> IncomingInvites = incomingInvites;
}
#endregion

#region Chat
[Serializable, NetSerializable]
public sealed class ReceivePartyChatMessage(string message) : EntityEventArgs
{
    public readonly string Message = message;
}
#endregion
