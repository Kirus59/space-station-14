// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<Party?>? LocalPartyChanged;
    event Action<Party>? LocalPartyUpdated;

    PartyUIController UIController { get; }

    Party? LocalParty { get; }
    bool IsLocalPartyHost { get; }

    PartyMember? LocalMember { get; }

    void CreatePartyRequest(PartySettingsState? settings = null);

    void DisbandPartyRequest();

    void LeavePartyRequest();

    void KickFromPartyRequest(NetUserId userId);

    void SetSettingsRequest(PartySettingsState settingsState);
}
