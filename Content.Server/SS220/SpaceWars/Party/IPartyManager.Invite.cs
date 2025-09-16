using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    bool TryAcceptInvite(IPartyInvite invite, bool force = false, bool ignoreLimit = false);
    void AcceptInvite(IPartyInvite invite, bool force = false, bool ignoreLimit = false);

    void DenyInvite(IPartyInvite invite);

    void DeleteInvite(uint inviteId);
    void DeleteInvite(Party party, ICommonSession target);
    void DeleteInvite(IPartyInvite invite);

    bool TryCreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out IPartyInvite? invite);
    bool TryCreateInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out IPartyInvite? invite);
    IPartyInvite CreateInvite(Party party, ICommonSession target, bool checkout = true);

    bool TryCreateAndSendInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out IPartyInvite? invite);
    IPartyInvite CreateAndSendInvite(Party party, ICommonSession target, bool checkout = true);

    void SendInvite(IPartyInvite invite);

    bool InviteAvailable(Party party, ICommonSession target);
    bool InviteAvailableCheckout(Party party, ICommonSession target, out PartyInviteCheckoutResult result);

    bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out IPartyInvite? invite);
    bool TryGetInvite(Party party, ICommonSession target, [NotNullWhen(true)] out IPartyInvite? invite);

    IPartyInvite? GetInvite(uint inviteId);
    IPartyInvite? GetInvite(Party party, ICommonSession target);

    IEnumerable<IPartyInvite> GetInvitesByParty(Party party);
    IEnumerable<IPartyInvite> GetInvitesByTarget(ICommonSession target);

    void SetInviteStatus(IPartyInvite invite, PartyInviteStatus status, bool updates = true);
}

public enum PartyInviteCheckoutResult
{
    Available,

    PartyNotExist,
    AlreadyMember,
    LimitReached,
    AlreadyInvited,
    DoesNotReseive
}

public record struct PartyInviteStatusChangedActionArgs(uint Id, PartyInviteStatus OldStatus, PartyInviteStatus NewStatus);
