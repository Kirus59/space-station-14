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

    void HandleInviteState(ClientPartyInviteState state);

    void AddSendedInvite(SendedPartyInvite invite);

    void RemoveSendedInvite(SendedPartyInvite invite);

    void AddIncomingInvite(IncomingPartyInvite invite);

    void RemoveIncomingInvite(IncomingPartyInvite invite);
}
