using Content.Shared.SS220.SpaceWars.Party;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInvite>? InviteAdded;
    event Action<PartyInvite>? InviteRemoved;
    event Action<PartyInvite>? InviteUpdated;

    IEnumerable<PartyInvite> ReceivedInvites { get; }
    IEnumerable<PartyInvite> LocalPartyInvites { get; }
    IEnumerable<PartyInvite> AllInvites { get; }

    void InviteInitialize();

    bool TryGetInvite(uint id, [NotNullWhen(true)] out PartyInvite? invite);
    PartyInvite? GetInvite(uint id);

    void InviteUserRequest(string username);
    Task<InviteUserResponceMessage> InviteUserRequestAsync(string username);

    void AcceptInviteRequest(uint inviteId);

    void DenyInviteRequest(uint inviteId);

    void DeleteInviteRequest(uint inviteId);

    void SetReceiveInvitesStatus(bool receiveInvites);
}
