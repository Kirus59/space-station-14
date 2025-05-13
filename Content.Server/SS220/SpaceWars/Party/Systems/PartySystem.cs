using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Server.Player;
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

        SubscribeNetworkEvent<CreatePartyRequestMessage>(OnCreatePartyRequest);
        SubscribeNetworkEvent<DisbandPartyRequestMessage>(OnDisbandPartyRequest);
        SubscribeNetworkEvent<LeavePartyRequestMessage>(OnLeavePartyMessage);
        SubscribeNetworkEvent<KickFromPartyRequestMessage>(OnKickFromPartyRequest);
        SubscribeNetworkEvent<SetPartySettingsRequestMessage>(OnSetSettingsRequest);

        InviteInitialize();
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, EntitySessionEventArgs args)
    {
        _partyManager.TryCreateParty(args.SenderSession, out var reason, settings: message.SettingsState);
    }

    private void OnDisbandPartyRequest(DisbandPartyRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByLeader(args.SenderSession);
        if (party == null)
            return;

        _partyManager.DisbandParty(party);
    }

    private void OnLeavePartyMessage(LeavePartyRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByMember(args.SenderSession);
        if (party == null)
            return;

        _partyManager.RemoveUserFromParty(args.SenderSession, party);
    }

    private void OnKickFromPartyRequest(KickFromPartyRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByLeader(args.SenderSession);
        if (party == null)
            return;

        var user = party.GetUserByPartyUserId(message.PartyUserId);
        if (user == null)
            return;

        _partyManager.RemoveUserFromParty(user.Value, party);
    }

    private void OnSetSettingsRequest(SetPartySettingsRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByLeader(args.SenderSession);
        if (party == null)
            return;

        _partyManager.SetSettings(party, message.State);
    }

    public void UpdatePartyData(ClientPartyDataState party, ICommonSession session)
    {
        var ev = new UpdateCurrentPartyMessage(party);
        RaiseNetworkEvent(ev, session);
    }

    public void SetCurrentParty(ClientPartyDataState? state, ICommonSession session)
    {
        var ev = new SetCurrentPartyMessage(state);
        RaiseNetworkEvent(ev, session);
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
