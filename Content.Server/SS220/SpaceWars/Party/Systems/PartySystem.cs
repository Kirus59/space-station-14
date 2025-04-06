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

        _partyManager.OnPartyDataUpdated += UpdatePartyDataForMembers;
        _partyManager.OnPartyUserUpdated += UpdatePartyData;

        SubscribeNetworkEvent<CreatePartyRequestMessage>(OnCreatePartyRequest);
        SubscribeNetworkEvent<PartyDataInfoRequestMessage>(OnPartyDataInfoRequest);
    }

    private void OnCreatePartyRequest(CreatePartyRequestMessage message, EntitySessionEventArgs args)
    {
        var created = _partyManager.TryCreateParty(args.SenderSession.UserId, out var reason);
        var responce = new CreatePartyResponceMessage(created, reason);
        RaiseNetworkEvent(responce, args.SenderSession.Channel);
    }

    private void OnPartyDataInfoRequest(PartyDataInfoRequestMessage message, EntitySessionEventArgs args)
    {
        var party = _partyManager.GetPartyByMember(args.SenderSession.UserId);
        var partyUser = _partyManager.GetPartyUser(args.SenderSession.UserId);
        UpdatePartyData(partyUser);
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
}
