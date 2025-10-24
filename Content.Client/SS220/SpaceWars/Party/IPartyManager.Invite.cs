// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInvite>? ReceivedInviteAdded;
    event Action<PartyInvite>? ReceivedInviteRemoved;
    event Action<PartyInvite>? ReceivedInviteUpdated;

    IReadOnlyList<PartyInvite> ReceivedInvites { get; }

    void InviteInitialize();

    bool TryGetInvite(uint id, [NotNullWhen(true)] out PartyInvite? invite);
    PartyInvite? GetInvite(uint id);

    void InviteUserRequest(string username);
    Task<MsgInviteUserResponce> InviteUserRequestAsync(string username);

    void AcceptInviteRequest(uint inviteId);

    void DenyInviteRequest(uint inviteId);

    void DeleteInviteRequest(uint inviteId);

    void SetReceiveInvitesStatus(bool receiveInvites);
}
