
using Content.Server.SS220.SpaceWars.Party.Systems;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<ServerPartyData>? OnPartyDataUpdated;
    event Action<ServerPartyData>? OnPartyDisbanding;

    List<ServerPartyData> Parties { get; }

    void SetPartySystem(PartySystem partySystem);

    /// <inheritdoc/>
    bool TryCreateParty(ICommonSession leader, [NotNullWhen(false)] out string? reason);

    /// <inheritdoc/>
    ServerPartyData CreateParty(ICommonSession leader);

    /// <inheritdoc/>
    void DisbandParty(ServerPartyData party);

    ServerPartyData? GetPartyById(uint id);

    ServerPartyData? GetPartyByLeader(ICommonSession leader);

    ServerPartyData? GetPartyByMember(ICommonSession member);

    bool TryAddUserToParty(ICommonSession user, ServerPartyData party, [NotNullWhen(false)] out string? reason);

    void AddUserToParty(ICommonSession user, ServerPartyData party, PartyRole role = PartyRole.Member);

    void RemoveUserFromParty(ICommonSession user, ServerPartyData party);

    #region PartyMenuUI
    void OpenPartyMenu(ICommonSession session);

    void ClosePartyMenu(ICommonSession session);
    #endregion
}
