
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action? OnCurrentPartyUpdated;

    PartyMenu? PartyMenu { get; }

    ClientPartyData? CurrentParty { get; }

    PartyUserInfo? LocalPartyUserInfo { get; }

    void SendCreatePartyRequest(PartySettingsState? settings = null);

    void SendDisbandPartyRequest();

    void SendLeavePartyRequest();

    void SendKickFromPartyRequest(uint partyUserId);

    void SetSettingsRequest(PartySettingsState settingsState);

    #region PartyMenuUI
    void SetPartyMenu(PartyMenu? partyMenu);
    #endregion
}
