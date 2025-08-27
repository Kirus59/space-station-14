using Content.Shared.SS220.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

[Serializable, NetSerializable]
public abstract class SharedParty(uint id)
{
    public readonly uint Id = id;

    public PartyStatus Status { get; protected set; } = PartyStatus.None;

    [Access(typeof(SharedPartyManager))]
    public virtual void SetStatus(PartyStatus newStatus)
    {
        Status = newStatus;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool Equals(SharedParty? party1, SharedParty? party2)
    {
        if (ReferenceEquals(party1, party2))
            return true;

        if (party1 is null)
            return false;

        return party1.Equals(party2);
    }

    public static bool operator ==(SharedParty? left, SharedParty? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SharedParty? left, SharedParty? right)
    {
        return !Equals(left, right);
    }
}

[Serializable, NetSerializable]
public record struct PartyState(uint Id,
    List<PartyMemberState> Members,
    PartySettingsState Settings,
    PartyStatus Status);

[Serializable, NetSerializable]
public record struct PartySettingsState
{
    public int MaxMembers;

    public PartySettingsState()
    {
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        MaxMembers = cfg.GetCVar(CCVars220.PartyMembersLimit);
    }
}

public enum PartyStatus
{
    None,
    Running,
    Disbanding,
    Disbanded
}
