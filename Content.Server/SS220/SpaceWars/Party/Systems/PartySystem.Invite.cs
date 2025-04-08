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
    }

    private void OnInviteInPartyRequest(InviteInPartyRequestMessage message, EntitySessionEventArgs args)
    {
        var sended = _partyManager.TrySendInvite(args.SenderSession, message.Username, out var failReason);
        var ev = new InviteInPartyAttemptResponceMessage(sended, failReason);
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

    public void SendInvite(PartyInvite invite, ICommonSession sender, ICommonSession target)
    {
        var senderEv = new CreatedNewInviteMessage(invite);
        RaiseNetworkEvent(senderEv, sender);

        var targetEv = new InviteReceivedMessage(invite);
        RaiseNetworkEvent(targetEv, target);
    }

    public void DirtyInvite(PartyInviteState state, ICommonSession session)
    {
        var ev = new HandleInviteState(state);
        RaiseNetworkEvent(ev, session);
    }
}
