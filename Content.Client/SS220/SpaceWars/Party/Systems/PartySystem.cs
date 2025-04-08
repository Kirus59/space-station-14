
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.SS220.SpaceWars.Party.Systems;

namespace Content.Client.SS220.SpaceWars.Party.Systems;

public sealed partial class PartySystem : SharedPartySystem
{
    [Dependency] private readonly IPartyManager _partyManager = default!;

    public Action<bool, string?>? CreatedPartyResponce;

    public override void Initialize()
    {
        base.Initialize();

        _partyManager.SetPartySystem(this);

        SubscribeNetworkEvent<CreatePartyResponceMessage>(OnCreatePartyResponce);
        SubscribeNetworkEvent<UpdatePartyDataInfoMessage>(OnUpdatePartyDataInfo);

        SubscribeNetworkEvent<OpenPartyMenuMessage>(OnOpenPartyMenuMessage);
        SubscribeNetworkEvent<ClosePartyMenuMessage>(OnClosePartyMenuMessage);

        InviteInitialize();
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
}
