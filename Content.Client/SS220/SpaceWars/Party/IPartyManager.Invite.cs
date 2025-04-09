using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInvite>? OnOutgoingInviteAdded;
    event Action<PartyInvite>? OnOutgoingInviteRemoved;
    event Action<PartyInvite>? OnIncomingInviteAdded;
    event Action<PartyInvite>? OnIncomingInviteRemoved;

    event Action<PartyInvite>? OnPartyInviteUpdated;

    public Dictionary<uint, PartyInvite> OutgoingInvites { get; }
    public Dictionary<uint, PartyInvite> IncomingInvites { get; }

    void SendInvite(string username);

    void AcceptInvite(uint inviteId);

    void DenyInvite(uint inviteId);

    void DeleteInvite(uint inviteId);

    void HandleInviteState(PartyInviteState state);

    void HandleInviteState(PartyInviteState state, PartyInvite invite);

    void AddOutgoingInvite(PartyInvite invite);

    void RemoveOutgoingInvite(PartyInvite invite);

    void AddIncomingInvite(PartyInvite invite);

    void RemoveIncomingInvite(PartyInvite invite);
}
