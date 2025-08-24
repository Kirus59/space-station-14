
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.IO;

namespace Content.Shared.SS220.SpaceWars.Party;

public sealed class PartyNetMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public PartyMessage? Message;

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        if (Message is null)
            return;

        var stream = new MemoryStream();
        serializer.Serialize(stream, Message);
        buffer.WriteVariableInt32((int)stream.Length);
        buffer.Write(stream.AsSpan());
    }

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var length = buffer.ReadVariableInt32();
        var stream = new MemoryStream(length);
        buffer.ReadAlignedMemory(stream, length);
        var obj = serializer.Deserialize(stream);
        if (obj is PartyMessage msg)
            Message = msg;
    }
}

[Serializable, NetSerializable]
public abstract class PartyMessage
{
    public NetUserId? Sender;
}

[Serializable, NetSerializable]
public abstract class PartyResponceMessage : PartyMessage
{
    public bool Timeout = false;
}

[Serializable, NetSerializable]
public sealed class CreatePartyRequestMessage(PartySettingsState? settingsState = null) : PartyMessage
{
    public readonly PartySettingsState? SettingsState = settingsState;
}

[Serializable, NetSerializable]
public sealed class DisbandPartyRequestMessage() : PartyMessage
{
}

[Serializable, NetSerializable]
public sealed class LeavePartyRequestMessage() : PartyMessage
{
}

[Serializable, NetSerializable]
public sealed class KickFromPartyRequestMessage(NetUserId userId) : PartyMessage
{
    public readonly NetUserId UserId = userId;
}

[Serializable, NetSerializable]
public sealed class UpdatePartyDataMessage(PartyState? state) : PartyMessage
{
    public readonly PartyState? State = state;
}

#region Settings
[Serializable, NetSerializable]
public sealed class SetPartySettingsRequestMessage(PartySettingsState state) : PartyMessage
{
    public readonly PartySettingsState State = state;
}
#endregion

#region Invite
[Serializable, NetSerializable]
public sealed class CreatedNewInviteMessage(SendedInviteState state) : PartyMessage
{
    public readonly SendedInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class InviteReceivedMessage(IncomingInviteState state) : PartyMessage
{
    public readonly IncomingInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class AcceptInviteMessage(uint inviteId) : PartyMessage
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class DenyInviteMessage(uint inviteId) : PartyMessage
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class DeleteInviteMessage(uint inviteId) : PartyMessage
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class InviteInPartyRequestMessage(string username) : PartyMessage
{
    public readonly string Username = username;
}

[Serializable, NetSerializable]
public sealed class InviteInPartyResponceMessage() : PartyResponceMessage
{
    public bool Success = false;
    public string Text = string.Empty;
}

[Serializable, NetSerializable]
public sealed class UpdateSendedInviteMessage(SendedInviteState state) : PartyMessage
{
    public readonly SendedInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class UpdateIncomingInviteMessage(IncomingInviteState state) : PartyMessage
{
    public readonly IncomingInviteState State = state;
}

[Serializable, NetSerializable]
public sealed class UpdateInvitesInfoMessage(List<SendedInviteState> sendedInvites, List<IncomingInviteState> incomingInvites) : PartyMessage
{
    public readonly List<SendedInviteState> SendedInvites = sendedInvites;
    public readonly List<IncomingInviteState> IncomingInvites = incomingInvites;
}

[Serializable, NetSerializable]
public sealed class SetReceiveInvitesStatusMessage(bool receiveInvites) : PartyMessage
{
    public readonly bool ReceiveInvites = receiveInvites;
}
#endregion

#region Chat
[Serializable, NetSerializable]
public sealed class PartyChatMessage(string message) : PartyMessage
{
    public readonly string Message = message;
}
#endregion
