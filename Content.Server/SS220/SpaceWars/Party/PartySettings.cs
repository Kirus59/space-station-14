using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class PartySettings
{
    public int MaxMembers = 10;

    public PartySettingsState GetState()
    {
        return new PartySettingsState()
        {
            MaxMembers = MaxMembers
        };
    }
}
