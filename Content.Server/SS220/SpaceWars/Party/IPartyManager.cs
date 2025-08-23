
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<Party>? PartyDataUpdated;
    event Action<Party>? PartyDisbanding;

    IReadOnlyList<Party> Parties { get; }

    bool TryCreateParty(ICommonSession leader, [NotNullWhen(false)] out string? reason, PartySettingsState? settings = null, bool force = false);

    Party? CreateParty(ICommonSession leader, PartySettingsState? settings = null, bool force = false);

    void DisbandParty(Party party);

    bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByLeader(ICommonSession leader, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByLeader(NetUserId leader, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByMember(NetUserId member, [NotNullWhen(true)] out Party? party);

    Party? GetPartyById(uint id);

    Party? GetPartyByLeader(ICommonSession leader);

    Party? GetPartyByLeader(NetUserId leader);

    Party? GetPartyByMember(ICommonSession member);

    Party? GetPartyByMember(NetUserId member);

    bool TryAddUserToParty(ICommonSession user,
        Party party,
        [NotNullWhen(false)] out string? reason,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false);

    void RemoveUserFromParty(ICommonSession user, Party party);

    void RemoveUserFromParty(NetUserId user, Party party);

    #region Settings
    void SetSettings(Party partyData, PartySettingsState state);
    #endregion
}
