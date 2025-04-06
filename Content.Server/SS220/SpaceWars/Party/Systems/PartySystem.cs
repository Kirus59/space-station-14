using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem : SharedPartySystem
{
    [Dependency] private readonly IPartyManager _partyManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        _partyManager.SetPartySystem(this);

        _partyManager.OnPartyDataUpdated += UpdatePartyDataForMembers;
        _partyManager.OnPartyUserUpdated += UpdatePartyData;
        _partyManager.OnPartyInviteUpdated += UpdateInviteInfo;

        SubscribeNetworkEvent<CreatePartyRequestMessage>(OnCreatePartyRequest);
        SubscribeNetworkEvent<DisbandPartyRequestMessage>(OnDisbandPartyRequest);
        SubscribeNetworkEvent<LeavePartyRequestMessage>(OnLeavePartyMessage);
        SubscribeNetworkEvent<InviteInPartyRequestMessage>(OnInviteInPartyMessage);

        SubscribeNetworkEvent<PartyDataInfoRequestMessage>(OnPartyDataInfoRequest);

        SubscribeNetworkEvent<AcceptInviteMessage>(OnAcceptInvite);
        SubscribeNetworkEvent<DenyInviteMessage>(OnDenyInvite);
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, EntitySessionEventArgs args)
    {
        var created = _partyManager.TryCreateParty(args.SenderSession.UserId, out var reason);
        var responce = new CreatePartyResponceMessage(created, reason);
        RaiseNetworkEvent(responce, args.SenderSession.Channel);
    }

    private void OnDisbandPartyRequest(DisbandPartyRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByLeader(args.SenderSession.UserId);
        if (party == null)
            return;

        _partyManager.DisbandParty(party);
    }

    private void OnLeavePartyMessage(LeavePartyRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByMember(args.SenderSession.UserId);
        if (party == null)
            return;

        _partyManager.RemovePlayerFromParty(args.SenderSession.UserId, party);
    }

    private void OnInviteInPartyMessage(InviteInPartyRequestMessage message, EntitySessionEventArgs args)
    {
        _partyManager.SendInviteToUser(args.SenderSession, message.Username);
    }

    private void OnPartyDataInfoRequest(PartyDataInfoRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByMember(args.SenderSession.UserId);
        var partyUser = _partyManager.GetPartyUser(args.SenderSession.UserId);
        UpdatePartyData(partyUser);
    }

    private void OnAcceptInvite(AcceptInviteMessage message, EntitySessionEventArgs args)
    {
        _partyManager.AcceptInvite(message.Invite);
    }

    private void OnDenyInvite(DenyInviteMessage message, EntitySessionEventArgs args)
    {
        _partyManager.DenyInvite(message.Invite);
    }

    public void UpdatePartyData(PartyUser user)
    {
        var session = _playerManager.GetSessionById(user.Id);
        var currentParty = _partyManager.GetPartyByMember(user.Id);
        var ev = new UpdatePartyDataInfoMessage(currentParty);
        RaiseNetworkEvent(ev, session);
    }

    private void UpdatePartyDataForMembers(PartyData party)
    {
        foreach (var member in party.Members)
            UpdatePartyData(member);
    }

    public void UpdateInviteInfo(PartyInvite invite)
    {
        if (!_playerManager.TryGetSessionById(invite.Sender, out var sender))
            return;

        var ev = new UpdateInviteInfo(invite);
        RaiseNetworkEvent(ev, sender);

        if (_playerManager.TryGetSessionById(invite.Target, out var target))
            RaiseNetworkEvent(ev, target);
    }

    public void OpenPartyMenu(ICommonSession session)
    {
        var ev = new OpenPartyMenuMessage();
        RaiseNetworkEvent(ev, session);
    }

    public void ClosePartyMenu(ICommonSession session)
    {
        var ev = new ClosePartyMenuMessage();
        RaiseNetworkEvent(ev, session);
    }
}
