
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
    public uint? Id;
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
public sealed class UpdateClientPartyMessage(PartyState? state) : PartyMessage
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
public sealed class AcceptInviteRequestMessage(uint inviteId) : PartyMessage
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class DenyInviteRequestMessage(uint inviteId) : PartyMessage
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class DeleteInviteRequestMessage(uint inviteId) : PartyMessage
{
    public readonly uint InviteId = inviteId;
}

[Serializable, NetSerializable]
public sealed class InviteUserRequestMessage(string username) : PartyMessage
{
    public readonly string Username = username;
}

[Serializable, NetSerializable]
public sealed class InviteUserResponceMessage() : PartyResponceMessage
{
    public bool Success = false;
    public string Text = string.Empty;
}

public sealed class UpdateClientPartyInvitesMessage(List<PartyInviteState> states) : PartyMessage
{
    public readonly List<PartyInviteState> States = states;
}

public sealed class UpdateClientPartyInviteMessage(PartyInviteState state) : PartyMessage
{
    public readonly PartyInviteState State = state;
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
