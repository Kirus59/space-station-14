
using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;

namespace Content.Client.SS220.SpaceWars.Party;

public interface IPartyManager : ISharedPartyManager
{
    event Action<PartyData?>? OnPartyDataUpdated;

    PartyMenu? PartyMenu { get; }

    PartyData? CurrentParty { get; }

    PartyInvite? LastSendedInvite { get; }

    HashSet<PartyInvite> SendedInvites { get; }

    [Access(typeof(SharedPartySystem))]
    void SetPartySystem(PartySystem partySystem);

    [Access(typeof(SharedPartySystem))]
    void SetPartyData(PartyData? currentParty);

    void SendCreatePartyRequest();

    void SendDisbandPartyRequest();

    void SendLeavePartyRequest();

    void SendInviteRequest(string username);

    void AcceptInvite(PartyInvite invite);

    void DenyInvite(PartyInvite invite);

    void UpdateInviteInfo(PartyInvite invite);

    PartyInvite? DequeueIncomingInvite();

    #region PartyMenuUI
    [Access(typeof(SharedPartySystem))]
    void SetPartyMenu(PartyMenu? partyMenu);
    #endregion
}
