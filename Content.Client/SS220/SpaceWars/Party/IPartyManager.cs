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

    /// <summary>
    /// Sends the request to create a new party with specified <paramref name="settings"/>
    /// </summary>
    void CreatePartyRequest(PartySettingsState? settings = null);

    /// <summary>
    /// Sends the request to disband the <see cref="LocalParty"/>
    /// </summary>
    void DisbandPartyRequest();

    /// <summary>
    /// Sends the request to leave the <see cref="LocalParty"/>
    /// </summary>
    void LeavePartyRequest();

    /// <summary>
    /// Sends the request to kick a specified member from the <see cref="LocalParty"/>
    /// </summary>
    void KickFromPartyRequest(NetUserId userId);

    /// <summary>
    /// Sends the request to set a new settings in the <see cref="LocalParty"/>
    /// </summary>
    void SetSettingsRequest(PartySettingsState settingsState);
}
