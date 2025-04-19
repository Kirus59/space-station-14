using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem
{
    public void InviteInitialize()
    {
        SubscribeNetworkEvent<InviteInPartyFailMessage>(OnSendInviteFail);

        SubscribeNetworkEvent<CreatedNewInviteMessage>(OnCreatedNewInvite);
        SubscribeNetworkEvent<InviteReceivedMessage>(OnInviteReceived);
        SubscribeNetworkEvent<UpdateSendedInviteMessage>(OnUpdateSendedInvite);
        SubscribeNetworkEvent<UpdateIncomingInviteMessage>(OnUpdateIncomingInvite);
        SubscribeNetworkEvent<UpdateInvitesInfoMessage>(OnUpdateInvitesInfo);
    }

    private void OnSendInviteFail(InviteInPartyFailMessage message)
    {
        _partyManager.SendInviteFail(message.Reason);
    }

    private void OnCreatedNewInvite(CreatedNewInviteMessage message)
    {
        var invite = new SendedPartyInvite(message.State);
        _partyManager.AddSendedInvite(invite);
    }

    private void OnInviteReceived(InviteReceivedMessage message)
    {
        var invite = new IncomingPartyInvite(message.State);
        _partyManager.AddIncomingInvite(invite);
    }

    private void OnUpdateSendedInvite(UpdateSendedInviteMessage message)
    {
        _partyManager.UpdateSendedInvite(message.State);
    }

    private void OnUpdateIncomingInvite(UpdateIncomingInviteMessage message)
    {
        _partyManager.UpdateIncomingInvite(message.State);
    }

    private void OnUpdateInvitesInfo(UpdateInvitesInfoMessage message)
    {
        _partyManager.UpdateSendedInvitesInfo(message.SendedInvites);
        _partyManager.UpdateIncomingInvitesInfo(message.IncomingInvites);
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
