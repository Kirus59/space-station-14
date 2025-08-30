
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed class PartySettings : SharedPartySettings
{
    public override int MembersLimit { get; set; } = 0;

    public PartySettings(PartySettingsState state)
    {
        HandleState(state);
    }

    public void HandleState(PartySettingsState state)
    {
        MembersLimit = state.MembersLimit;
    }
}
