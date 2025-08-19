
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<ServerPartyData>? PartyDataUpdated;
    event Action<ServerPartyData>? PartyDisbanding;

    IReadOnlyList<ServerPartyData> Parties { get; }

    bool TryCreateParty(ICommonSession leader, [NotNullWhen(false)] out string? reason, PartySettingsState? settings = null, bool force = false);

    ServerPartyData? CreateParty(ICommonSession leader, PartySettingsState? settings = null, bool force = false);

    void DisbandParty(ServerPartyData party);

    bool TryGetPartyById(uint id, [NotNullWhen(true)] out ServerPartyData? party);

    bool TryGetPartyByLeader(ICommonSession leader, [NotNullWhen(true)] out ServerPartyData? party);

    bool TryGetPartyByLeader(NetUserId leader, [NotNullWhen(true)] out ServerPartyData? party);

    bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out ServerPartyData? party);

    bool TryGetPartyByMember(NetUserId member, [NotNullWhen(true)] out ServerPartyData? party);

    ServerPartyData? GetPartyById(uint id);

    ServerPartyData? GetPartyByLeader(ICommonSession leader);

    ServerPartyData? GetPartyByLeader(NetUserId leader);

    ServerPartyData? GetPartyByMember(ICommonSession member);

    ServerPartyData? GetPartyByMember(NetUserId member);

    bool TryAddUserToParty(ICommonSession user,
        ServerPartyData party,
        [NotNullWhen(false)] out string? reason,
        PartyRole role = PartyRole.Member,
        bool force = false);

    void RemoveUserFromParty(ICommonSession user, ServerPartyData party);

    void RemoveUserFromParty(NetUserId user, ServerPartyData party);

    #region Settings
    void SetSettings(ServerPartyData partyData, PartySettingsState state);
    #endregion
}
