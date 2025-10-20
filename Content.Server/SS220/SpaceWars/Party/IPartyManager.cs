// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<PartyStatusChangedActionArgs>? PartyStatusChanged;
    event Action<Party>? PartyUpdated;

    event Action<PartyMember>? UserJoinedParty;
    event Action<PartyMember>? UserLeavedParty;

    IReadOnlyCollection<Party> Parties { get; }

    bool CreateParty(ICommonSession host, PartySettingsState? settings = null, bool force = false);

    bool CreateParty(ICommonSession host, [NotNullWhen(true)] out Party? party, PartySettingsState? settings = null, bool force = false);

    bool DisbandParty(Party party, bool updates = true);

    bool AddMember(Party party,
        ICommonSession session,
        PartyMemberRole role = PartyMemberRole.Member,
        bool force = false,
        bool ignoreLimit = false,
        bool updates = true,
        bool notify = true);

    bool RemoveMember(Party party, ICommonSession session, bool updates = true, bool notify = true);

    bool SetHost(Party party, ICommonSession session, bool force = false, bool updates = true);

    bool SetStatus(Party party, PartyStatus newStatus, bool updates = true);

    void EnsureNotPartyMember(ICommonSession session, bool updates = true);

    bool IsAnyPartyMember(ICommonSession session);

    bool PartyExist(Party? party);

    bool TryGetPartyById(uint id, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByHost(ICommonSession host, [NotNullWhen(true)] out Party? party);

    bool TryGetPartyByMember(ICommonSession member, [NotNullWhen(true)] out Party? party);

    Party? GetPartyById(uint id);

    Party? GetPartyByHost(ICommonSession session);

    Party? GetPartyByMember(ICommonSession session);

    IEnumerable<CompletionOption> GetPartiesCompletionOptions();

    #region Settings
    void SetSettings(Party party, PartySettingsState state, bool updates = true);
    #endregion
}

public record struct PartyStatusChangedActionArgs(uint PartyId, PartyStatus OldStatus, PartyStatus NewStatus);
