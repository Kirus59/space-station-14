
using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    private PartySystem? _partySystem;

    public event Action? OnCurrentPartyUpdated;

    public PartyMenu? PartyMenu => _partyMenu;
    private PartyMenu? _partyMenu;

    public ClientPartyData? CurrentParty => _currentParty;
    private ClientPartyData? _currentParty;

    public PartyUserInfo? LocalPartyUserInfo => CurrentParty?.LocalUserInfo;

    public override void Initialize()
    {
        base.Initialize();

        SetPartyMenu(new PartyMenu());
    }

    public void SetPartySystem(PartySystem partySystem)
    {
        _partySystem = partySystem;
    }

    public void SetCurrentParty(ClientPartyDataState? state)
    {
        if (state == null || state.Value.Disbanded)
        {
            _currentParty = null;
        }
        else
        {
            var party = new ClientPartyData(state.Value.Id, state.Value.LocalUserInfo, state.Value.Members);
            _currentParty = party;
        }

        OnCurrentPartyUpdated?.Invoke();
    }

    public void UpdateCurrentParty(ClientPartyDataState state)
    {
        if (_currentParty == null)
            return;

        UpdatePartyState(_currentParty, state);
        OnCurrentPartyUpdated?.Invoke();
    }

    private void UpdatePartyState(ClientPartyData party, ClientPartyDataState state)
    {
        if (state.Id != party.Id)
            return;

        party.LocalUserInfo = state.LocalUserInfo;
        party.Members = state.Members;
        party.Disbanded = state.Disbanded;
    }

    public void SendCreatePartyRequest()
    {
        _partySystem?.SendCreatePartyRequest();
    }

    public void SendDisbandPartyRequest()
    {
        _partySystem?.SendDisbandPartyRequest();
    }

    public void SendLeavePartyRequest()
    {
        _partySystem?.SendLeavePartyRequest();
    }

    public void SendKickFromPartyRequest(uint partyUserId)
    {
        _partySystem?.SendKickFromPartyRequest(partyUserId);
    }

    #region PartyMenuUI
    public void SetPartyMenu(PartyMenu? partyMenu)
    {
        _partyMenu = partyMenu;
    }
    #endregion
}

public sealed class ClientPartyData : SharedPartyData
{
    public PartyUserInfo LocalUserInfo;
    public List<PartyUserInfo> Members;

    public ClientPartyData(uint id, PartyUserInfo localUserInfo, List<PartyUserInfo> members) : base(id)
    {
        LocalUserInfo = localUserInfo;
        Members = members;
    }
}
