
using Content.Server.SS220.SpaceWars.Party.Systems;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager : ISharedPartyManager
{
    event Action<PartyData>? OnPartyDataUpdated;
    event Action<PartyData>? OnPartyDisbanding;
    event Action<PartyUser>? OnPartyUserUpdated;

    [Access(Other = AccessPermissions.Read)]
    List<PartyData> Parties { get; }

    [Access(typeof(SharedPartySystem))]
    void SetPartySystem(PartySystem partySystem);

    bool TryCreateParty(NetUserId leader, [NotNullWhen(false)] out string? reason);

    /// <summary>
    ///     Creates a new party with the <paramref name="leader"/>
    /// </summary>
    /// <exception cref="ArgumentException"> <paramref name="leader"/> must not be present at another party </exception>
    PartyData? CreateParty(NetUserId leader);

    void DisbandParty(PartyData party);

    PartyData? GetPartyByLeader(NetUserId leader);

    PartyData? GetPartyByMember(NetUserId member);

    bool TryAddPlayerToParty(NetUserId member, PartyData party, [NotNullWhen(false)] out string? reason);

    /// <summary>
    ///     Adds a <paramref name="member"/> to the <paramref name="party"/>
    /// </summary>
    /// <exception cref="ArgumentException"> <paramref name="member"/> must not be present at another party </exception>
    void AddPlayerToParty(NetUserId member, PartyData party);

    void RemoveUserFromParty(NetUserId member, PartyData party);

    PartyUser GetPartyUser(NetUserId userId);

    public void SetPartyUserRole(PartyUser user, PartyRole role);

    public void SetPartyUserConnected(PartyUser user, bool connected);

    #region PartyMenuUI
    void OpenPartyMenu(ICommonSession session);

    void ClosePartyMenu(ICommonSession session);
    #endregion
}
