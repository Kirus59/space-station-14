using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;
using Robust.Shared.Timing;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem : SharedPartySystem
{
    [Dependency] private readonly IPartyManager _partyManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public Action<bool, string?>? CreatedPartyResponce;

    public TimeSpan _invitesDequeuePeriod = TimeSpan.FromSeconds(3);
    public TimeSpan _nextInviteDequeueTick = TimeSpan.Zero;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTiming.CurTime < _nextInviteDequeueTick)
            return;

        var invite = _partyManager.DequeueIncomingInvite();
        if (invite == null)
            return;

        var invitedWindow = new InvitedInPartyWindow(invite);
        invitedWindow.OpenCentered();
        _nextInviteDequeueTick = _gameTiming.CurTime + _invitesDequeuePeriod;
    }

    public override void Initialize()
    {
        base.Initialize();

        _partyManager.SetPartySystem(this);

        SubscribeNetworkEvent<CreatePartyResponceMessage>(OnCreatePartyResponce);
        SubscribeNetworkEvent<UpdatePartyDataInfoMessage>(OnUpdatePartyDataInfo);
        SubscribeNetworkEvent<UpdateInviteInfo>(OnInviteUpdate);

        SubscribeNetworkEvent<OpenPartyMenuMessage>(OnOpenPartyMenuMessage);
        SubscribeNetworkEvent<ClosePartyMenuMessage>(OnClosePartyMenuMessage);
    }

    private void OnCreatePartyResponce(CreatePartyResponceMessage message)
    {
        CreatedPartyResponce?.Invoke(message.IsCreated, message.Reason);
    }

    private void OnUpdatePartyDataInfo(UpdatePartyDataInfoMessage message)
    {
        _partyManager.SetPartyData(message.PartyData);
    }

    private void OnOpenPartyMenuMessage(OpenPartyMenuMessage message)
    {
        _partyManager.PartyMenu?.OpenCentered();
    }

    private void OnClosePartyMenuMessage(ClosePartyMenuMessage message)
    {
        _partyManager.PartyMenu?.Close();
    }

    private void OnInviteUpdate(UpdateInviteInfo message)
    {
        _partyManager.UpdateInviteInfo(message.Invite);
    }

    public void SendCreatePartyRequest()
    {
        var request = new CreatePartyRequestMessage();
        RaiseNetworkEvent(request);
    }

    public void SendDisbandPartyRequest()
    {
        var ev = new DisbandPartyRequestMessage();
        RaiseNetworkEvent(ev);
    }

    public void SendLeavePartyRequest()
    {
        var ev = new LeavePartyRequestMessage();
        RaiseNetworkEvent(ev);
    }

    public void SendInviteRequest(string username)
    {
        var ev = new InviteInPartyRequestMessage(username);
        RaiseNetworkEvent(ev);
    }

    public void AcceptInvite(PartyInvite invite)
    {
        var ev = new AcceptInviteMessage(invite);
        RaiseNetworkEvent(ev);
    }

    public void DenyInvite(PartyInvite invite)
    {
        var ev = new DenyInviteMessage(invite);
        RaiseNetworkEvent(ev);
    }
}
