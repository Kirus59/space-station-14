
using Content.Server.SS220.SpaceWars.Party.Systems;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public interface IPartyManager : ISharedPartyManager
{
    event Action<PartyData>? OnPartyDataUpdated;
    event Action<PartyData>? OnPartyDisbanding;
    event Action<PartyUser>? OnPartyUserUpdated;
    event Action<PartyInvite>? OnPartyInviteUpdated;

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

    void RemovePlayerFromParty(NetUserId member, PartyData party);

    PartyUser GetPartyUser(NetUserId userId);

    void AcceptInvite(PartyInvite invite);

    void DenyInvite(PartyInvite invite);

    public void SetPartyUserRole(PartyUser user, PartyRole role);

    public void SetPartyUserConnected(PartyUser user, bool connected);

    void SendInviteToUser(ICommonSession sender, string username);
    public void SendInviteToUser(ICommonSession sender, ICommonSession target);
    public void SendInviteToUser(PartyInvite invite, string username);
    public void SendInviteToUser(PartyInvite invite, ICommonSession target);

    #region PartyMenuUI
    void OpenPartyMenu(ICommonSession session);

    void ClosePartyMenu(ICommonSession session);
    #endregion
}
