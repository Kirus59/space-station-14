
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract class SharedPartySettings
{
    public abstract int MembersLimit { get; set; }
}

[Serializable, NetSerializable]
public record struct PartySettingsState(int MembersLimit);
