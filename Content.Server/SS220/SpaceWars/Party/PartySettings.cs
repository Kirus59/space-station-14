
using Content.Shared.SS220.CCVars;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Configuration;

namespace Content.Server.SS220.SpaceWars.Party;

public sealed class PartySettings
{
    private readonly Party _party;
    private readonly IConfigurationManager _cfg;

    public int MembersLimit
    {
        get => _membersLimit;
        set => _membersLimit = Math.Clamp(value, _party.Members.Count, _cfg.GetCVar(CCVars220.PartyMembersLimit));
    }
    private int _membersLimit;

    public PartySettings(Party party, IConfigurationManager? cfg = null)
    {
        _party = party;
        _cfg = cfg ?? IoCManager.Resolve<IConfigurationManager>();

        _membersLimit = _cfg.GetCVar(CCVars220.PartyMembersLimit);
    }

    public PartySettings(Party party, PartySettingsState state, IConfigurationManager? cfg = null)
    {
        _party = party;
        _cfg = cfg ?? IoCManager.Resolve<IConfigurationManager>();

        HandleState(state);
    }

    public PartySettingsState GetState()
    {
        return new PartySettingsState(MembersLimit);
    }

    public void HandleState(PartySettingsState state)
    {
        MembersLimit = state.MembersLimit;
    }
}
