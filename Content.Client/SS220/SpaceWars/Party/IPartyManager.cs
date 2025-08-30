
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action? OnCurrentPartyUpdated;

    PartyMenu? PartyMenu { get; set; }

    Party? LocalParty { get; }
    bool IsLocalPartyHost { get; }

    PartyMember? LocalMember { get; }

    void SendCreatePartyRequest(PartySettingsState? settings = null);

    void SendDisbandPartyRequest();

    void SendLeavePartyRequest();

    void SendKickFromPartyRequest(NetUserId userId);

    void SetSettingsRequest(PartySettingsState settingsState);
}
