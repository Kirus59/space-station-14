using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<SendedPartyInvite>? OnSendedInviteAdded;
    event Action<SendedPartyInvite>? OnSendedInviteRemoved;
    event Action<SendedPartyInvite>? OnSendedInviteUpdated;

    event Action<IncomingPartyInvite>? OnIncomingInviteAdded;
    event Action<IncomingPartyInvite>? OnIncomingInviteRemoved;
    event Action<IncomingPartyInvite>? OnIncomingInviteUpdated;

    event Action<string>? OnSendInviteFail;
    Dictionary<uint, SendedPartyInvite> SendedInvites { get; }

    Dictionary<uint, IncomingPartyInvite> IncomingInvites { get; }

    void SendInvite(string username);

    void SendInviteFail(string reason);
    void AcceptInvite(uint inviteId);

    void DenyInvite(uint inviteId);

    void DeleteInvite(uint inviteId);

    void UpdateSendedInvitesInfo(List<SendedInviteState> sendedInvites);

    void UpdateIncomingInvitesInfo(List<IncomingInviteState> incomingInvites);

    void UpdateSendedInvite(SendedInviteState state);

    void UpdateIncomingInvite(IncomingInviteState state);

    void AddSendedInvite(SendedInviteState state);

    void AddSendedInvite(SendedPartyInvite invite);

    void RemoveSendedInvite(uint id);

    void RemoveSendedInvite(SendedPartyInvite invite);

    void AddIncomingInvite(IncomingInviteState state);

    void AddIncomingInvite(IncomingPartyInvite invite);

    void RemoveIncomingInvite(uint id);

    void RemoveIncomingInvite(IncomingPartyInvite invite);
}
