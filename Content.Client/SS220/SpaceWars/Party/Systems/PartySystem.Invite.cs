using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem
{
    public void InviteInitialize()
    {
        SubscribeNetworkEvent<InviteInPartyFailMessage>(OnSendInviteFail);

        SubscribeNetworkEvent<CreatedNewInviteMessage>(OnCreatedNewInvite);
        SubscribeNetworkEvent<InviteReceivedMessage>(OnInviteReceived);
        SubscribeNetworkEvent<HandleInviteState>(OnInviteHandleState);
    }

    private void OnSendInviteFail(InviteInPartyFailMessage message)
    {
        _partyManager.SendInviteFail(message.Reason);
    }

    private void OnCreatedNewInvite(CreatedNewInviteMessage message)
    {
        var invite = new SendedPartyInvite(message.InviteId, message.TargetName, message.Status);
        _partyManager.AddSendedInvite(invite);
    }

    private void OnInviteReceived(InviteReceivedMessage message)
    {
        var invite = new IncomingPartyInvite(message.InviteId, message.SenderName, message.Status);
        _partyManager.AddIncomingInvite(invite);
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
