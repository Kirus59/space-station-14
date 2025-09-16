using Content.Shared.SS220.SpaceWars.Party;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<IPartyInvite>? InviteAdded;
    event Action<IPartyInvite>? InviteRemoved;
    event Action<IPartyInvite>? InviteUpdated;

    IReadOnlyList<IPartyInvite> SendedInvites { get; }
    IReadOnlyList<IPartyInvite> ReceivedInvites { get; }
    IReadOnlyList<IPartyInvite> AllInvites { get; }

    void InviteInitialize();

    bool TryGetInvite(uint id, [NotNullWhen(true)] out IPartyInvite? invite);
    IPartyInvite? GetInvite(uint id);

    void InviteUserRequest(string username);
    Task<MsgInviteUserResponce> InviteUserRequestAsync(string username);

    void AcceptInviteRequest(uint inviteId);

    void DenyInviteRequest(uint inviteId);

    void DeleteInviteRequest(uint inviteId);

    void SetReceiveInvitesStatus(bool receiveInvites);
}
