using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInviteStatusChangedActionArgs>? PartyInviteStatusChanged;

    bool TryAcceptInvite(PartyInvite invite, bool force = false, bool ignoreLimit = false);
    void AcceptInvite(PartyInvite invite, bool force = false, bool ignoreLimit = false);

    void DenyInvite(PartyInvite invite);

    void DeleteInvite(uint inviteId);
    void DeleteInvite(Party party, ICommonSession receiver);
    void DeleteInvite(PartyInvite invite);

    bool TryCreateInvite(Party party, ICommonSession target, [NotNullWhen(true)] out PartyInvite? invite);
    bool TryCreateInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite);
    PartyInvite CreateInvite(Party party, ICommonSession target, bool checkout = true);

    bool TryCreateAndSendInvite(Party party, ICommonSession target, out PartyInviteCheckoutResult result, [NotNullWhen(true)] out PartyInvite? invite);
    PartyInvite CreateAndSendInvite(Party party, ICommonSession target, bool checkout = true);

    void SendInvite(PartyInvite invite);

    bool InviteAvailable(Party party, ICommonSession target);
    bool InviteCheckout(Party party, ICommonSession target, out PartyInviteCheckoutResult result);

    bool TryGetInvite(uint inviteId, [NotNullWhen(true)] out PartyInvite? invite);
    bool TryGetInvite(Party party, ICommonSession receiver, [NotNullWhen(true)] out PartyInvite? invite);

    PartyInvite? GetInvite(uint inviteId);
    PartyInvite? GetInvite(Party party, ICommonSession receiver);

    IEnumerable<PartyInvite> GetInvitesByParty(Party party);
    IEnumerable<PartyInvite> GetInvitesByReceiver(ICommonSession receiver);

    void SetInviteStatus(PartyInvite invite, PartyInviteStatus status, bool updates = true);
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

public record struct PartyInviteStatusChangedActionArgs(uint PartyId, PartyInviteStatus OldStatus, PartyInviteStatus NewStatus);
