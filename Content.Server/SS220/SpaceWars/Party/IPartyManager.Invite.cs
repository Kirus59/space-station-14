// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    bool AcceptInvite(PartyInvite invite, bool ignoreLimit = false);

    void DenyInvite(PartyInvite invite);

    bool CreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite);

    bool CreateInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite);

    bool DeleteInvite(uint inviteId);

    bool DeleteInvite(Party party, ICommonSession receiver);

    bool DeleteInvite(PartyInvite invite);

    bool InviteAvailable(Party party, ICommonSession target);

    bool InviteAvailableCheckout(Party party, ICommonSession target, out PartyInviteCheckoutResult result, bool ingoreMembersLimit = false);

    bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out PartyInvite? invite);

    bool TryGetInvite(Party party, ICommonSession receiver, [NotNullWhen(true)] out PartyInvite? invite);

    PartyInvite? GetInvite(uint inviteId);

    PartyInvite? GetInvite(Party party, ICommonSession target);

    IEnumerable<PartyInvite> GetInvitesByParty(Party party);

    IEnumerable<PartyInvite> GetInvitesByTarget(ICommonSession target);

    void SetInviteStatus(PartyInvite invite, PartyInviteStatus status, bool updates = true);

    bool InviteExist(PartyInvite? invite);
}

public enum PartyInviteCheckoutResult
{
    Available,

    PartyNotExist,
    AlreadyMember,
    InvitesLimitReached,
    MembersLimitReached,
    AlreadyInvited,
    DoesNotReseive
}

public record struct PartyInviteStatusChangedActionArgs(uint Id, PartyInviteStatus OldStatus, PartyInviteStatus NewStatus);
