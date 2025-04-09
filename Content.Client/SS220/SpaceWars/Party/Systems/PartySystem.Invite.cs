using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem
{
    public void InviteInitialize()
    {
        SubscribeNetworkEvent<CreatedNewInviteMessage>(OnCreatedNewInvite);
        SubscribeNetworkEvent<InviteReceivedMessage>(OnInviteReceived);
        SubscribeNetworkEvent<HandleInviteState>(OnInviteHandleState);
    }

    private void OnCreatedNewInvite(CreatedNewInviteMessage message)
    {
        _partyManager.AddOutgoingInvite(message.Invite);
    }

    private void OnInviteReceived(InviteReceivedMessage message)
    {
        _partyManager.AddIncomingInvite(message.Invite);
    }

    private void OnInviteHandleState(HandleInviteState message)
    {
        _partyManager.HandleInviteState(message.State);
    }

    public void SendInvite(string username)
    {
        var ev = new InviteInPartyRequestMessage(username);
        RaiseNetworkEvent(ev);
    }

    public void AcceptInvite(uint inviteId)
    {
        var ev = new AcceptInviteMessage(inviteId);
        RaiseNetworkEvent(ev);
    }

    public void DenyInvite(uint inviteId)
    {
        var ev = new AcceptInviteMessage(inviteId);
        RaiseNetworkEvent(ev);
    }

    public void DeleteInvite(uint inviteId)
    {
        var ev = new DeleteInviteMessage(inviteId);
        RaiseNetworkEvent(ev);
    }
}
