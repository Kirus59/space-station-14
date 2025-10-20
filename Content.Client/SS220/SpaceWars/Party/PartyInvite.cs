
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.SS220.SpaceWars.Party;

public interface IPartyInvite<T> : ISharedPartyInvite where T : IPartyInviteState
{
    uint PartyId { get; }
    NetUserId Target { get; }
    string SenderName { get; }
    string TargetName { get; }
    PartyInviteKind Kind { get; }

    void SetKind(PartyInviteKind kind);
    void HandleState(T state);
}

public sealed class PartyInvite(uint id,
    PartyInviteStatus status,
    uint partyId,
    NetUserId target,
    string? senderName = null,
    string? targetName = null,
    PartyInviteKind kind = PartyInviteKind.Other) : IPartyInvite<PartyInviteState>, IEquatable<PartyInvite>
{
    public uint Id { get; } = id;
    public PartyInviteStatus Status { get; private set; } = status;

    public uint PartyId { get; } = partyId;
    public NetUserId Target { get; } = target;
    public string SenderName { get; private set; } = senderName ?? string.Empty;
    public string TargetName { get; private set; } = targetName ?? string.Empty;
    public PartyInviteKind Kind { get; private set; } = kind;

    public PartyInvite(PartyInviteState state, PartyInviteKind kind = PartyInviteKind.Other)
        : this(state.Id, state.Status, state.PartyId, state.Target, state.SenderName, state.TargetName, kind) { }

    public void SetKind(PartyInviteKind kind)
    {
        Kind = kind;
    }

    public void HandleState(PartyInviteState state)
    {
        if (state.Id != Id)
            return;

        DebugTools.Assert(PartyId == state.PartyId);
        DebugTools.Assert(Target == state.Target);

        Status = state.Status;
        SenderName = state.SenderName;
        TargetName = state.TargetName;
    }

    public bool Equals(PartyInvite? other)
    {
        if (other is null)
            return false;

        return Id.Equals(other.Id);
    }

    public static bool Equals(PartyInvite? left, PartyInvite? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator ==(PartyInvite? left, PartyInvite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PartyInvite? left, PartyInvite? right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PartyInvite invite)
            return false;

        return Equals(invite);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public enum PartyInviteKind
{
    Sended,
    Received,
    Other
}
