
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem : SharedPartySystem
{
    [Dependency] private readonly IPartyManager _partyManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        _partyManager.SetPartySystem(this);

        SubscribeNetworkEvent<SetCurrentPartyMessage>(OnSetCurrentParty);
        SubscribeNetworkEvent<UpdateCurrentPartyMessage>(OnUpdatePartyDataInfo);

        SubscribeNetworkEvent<OpenPartyMenuMessage>(OnOpenPartyMenuMessage);
        SubscribeNetworkEvent<ClosePartyMenuMessage>(OnClosePartyMenuMessage);

        InviteInitialize();
        ChatInitialize();
    }

    private void OnSetCurrentParty(SetCurrentPartyMessage message)
    {
        _partyManager.SetCurrentParty(message.State);
    }

    private void OnUpdatePartyDataInfo(UpdateCurrentPartyMessage message)
    {
        _partyManager.UpdateCurrentParty(message.State);
    }

    private void OnOpenPartyMenuMessage(OpenPartyMenuMessage message)
    {
        _partyManager.PartyMenu?.OpenCentered();
    }

    private void OnClosePartyMenuMessage(ClosePartyMenuMessage message)
    {
        _partyManager.PartyMenu?.Close();
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

    public void SendKickFromPartyRequest(uint partyUserId)
    {
        var ev = new KickFromPartyRequestMessage(partyUserId);
        RaiseNetworkEvent(ev);
    }

    public void SendInviteRequest(string username)
    {
        var ev = new InviteInPartyRequestMessage(username);
        RaiseNetworkEvent(ev);
    }
}
