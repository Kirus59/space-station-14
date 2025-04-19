using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem
{
    public void InviteInitialize()
    {
        SubscribeNetworkEvent<InviteInPartyRequestMessage>(OnInviteInPartyRequest);
        SubscribeNetworkEvent<AcceptInviteMessage>(OnAcceptInvite);
        SubscribeNetworkEvent<DenyInviteMessage>(OnDenyInvite);
        SubscribeNetworkEvent<DeleteInviteMessage>(OnDeleteInvite);
    }

    private void OnInviteInPartyRequest(InviteInPartyRequestMessage message, EntitySessionEventArgs args)
    {
        if (_partyManager.TrySendInvite(args.SenderSession, message.Username, out var failReason))
            return;

        var ev = new InviteInPartyFailMessage(failReason);
        RaiseNetworkEvent(ev, args.SenderSession);
    }

    private void OnAcceptInvite(AcceptInviteMessage message, EntitySessionEventArgs args)
    {
        _partyManager.AcceptInvite(message.InviteId, args.SenderSession);
    }

    private void OnDenyInvite(DenyInviteMessage message, EntitySessionEventArgs args)
    {
        _partyManager.DenyInvite(message.InviteId, args.SenderSession);
    }

    private void OnDeleteInvite(DeleteInviteMessage message, EntitySessionEventArgs args)
    {
        _partyManager.DeleteInvite(message.InviteId, args.SenderSession);
    }

    public void SendInvite(ServerPartyInvite invite)
    {
        if (_playerManager.TryGetSessionById(invite.Sender, out var sender))
        {
            var state = invite.GetSendedInviteState();
            var ev = new CreatedNewInviteMessage(state);
            RaiseNetworkEvent(ev, sender);
        }

        if (_playerManager.TryGetSessionById(invite.Target, out var target))
        {
            var state = invite.GetIncomingInviteState();
            var ev = new InviteReceivedMessage(state);
            RaiseNetworkEvent(ev, target);
        }
    }

    public void DirtyInvite(ServerPartyInvite invite)
    {
        if (_playerManager.TryGetSessionById(invite.Sender, out var sender))
        {
            var state = invite.GetSendedInviteState();
            UpdateSendedInvite(state, sender);
        }

        if (_playerManager.TryGetSessionById(invite.Target, out var target))
        {
            var state = invite.GetIncomingInviteState();
            UpdateIncomingInvite(state, target);
        }
    }

    public void UpdateSendedInvite(SendedInviteState state, ICommonSession session)
    {
        var ev = new UpdateSendedInviteMessage(state);
        RaiseNetworkEvent(ev, session);
    }

    public void UpdateIncomingInvite(IncomingInviteState state, ICommonSession session)
    {
        var ev = new UpdateIncomingInviteMessage(state);
        RaiseNetworkEvent(ev, session);
    }

    public void UpdateInvitesInfo(List<SendedInviteState> sendedInvites, List<IncomingInviteState> incomingInvites, ICommonSession session)
    {
        var ev = new UpdateInvitesInfoMessage(sendedInvites, incomingInvites);
        RaiseNetworkEvent(ev, session);
    }
}
