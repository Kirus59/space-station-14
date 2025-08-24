
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<Party>? PartyStatusChanged;
    event Action<Party>? PartyDataUpdated;

    event Action<PartyMember>? UserJoinedParty;
    event Action<PartyMember>? UserLeavedParty;

    IReadOnlyList<Party> Parties { get; }

    /// <inheritdoc/>
    bool TryCreateParty(ICommonSession host, [NotNullWhen(true)] out Party? party, PartySettingsState? settings = null, bool force = false);

    /// <inheritdoc/>
    Party? CreateParty(ICommonSession host, PartySettingsState? settings = null, bool force = false);

    void DisbandParty(Party party, bool updates = true);

    bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByHost(ICommonSession host, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party);

    Party? GetPartyById(uint id);

    Party? GetPartyByHost(ICommonSession leader);


    Party? GetPartyByMember(ICommonSession member);

    bool AddUserToParty(ICommonSession session,
        Party party,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool updates = true,
        bool throwException = false);

    bool RemoveUserFromParty(ICommonSession session, Party party, bool throwException = false, bool updates = true);

    bool SetNewHost(ICommonSession session, Party party, bool force = false, bool throwException = false, bool updates = true);

    void EnsureNotPartyMember(ICommonSession session, bool updates = true);

    bool IsAnyPartyMember(ICommonSession session);

    #region Settings
    void SetSettings(Party party, PartySettingsState state, bool updates = true);
    #endregion
}
