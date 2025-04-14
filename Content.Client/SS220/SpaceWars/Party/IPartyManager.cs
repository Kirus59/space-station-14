
using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action? OnCurrentPartyUpdated;

    PartyMenu? PartyMenu { get; }

    ClientPartyData? CurrentParty { get; }

    PartyUserInfo? LocalPartyUserInfo { get; }

    void SetPartySystem(PartySystem partySystem);

    void SetCurrentParty(ClientPartyDataState? state);

    void UpdateCurrentParty(ClientPartyDataState state);

    void SendCreatePartyRequest();

    void SendDisbandPartyRequest();

    void SendLeavePartyRequest();

    void SendKickFromPartyRequest(uint partyUserId);

    #region PartyMenuUI
    void SetPartyMenu(PartyMenu? partyMenu);
    #endregion
}
