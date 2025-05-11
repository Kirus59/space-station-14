
using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            var party = new ClientPartyData(state.Value);
            _currentParty = party;
        }

        OnCurrentPartyUpdated?.Invoke();
    }

    public void UpdateCurrentParty(ClientPartyDataState state)
    {
        if (_currentParty == null || state.Id != _currentParty.Id)
            return;

        _currentParty.UpdateState(state);
        OnCurrentPartyUpdated?.Invoke();
    }

    public void SendCreatePartyRequest(PartySettingsState? settings = null)
    {
        _partySystem?.SendCreatePartyRequest(settings);
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
    public PartySettings Settings;

    public ClientPartyData(uint id, PartyUserInfo localUserInfo, List<PartyUserInfo> members, PartySettingsState state) : base(id)
    {
        LocalUserInfo = localUserInfo;
        Members = members;
        Settings = new PartySettings(state);
    }

    public ClientPartyData(ClientPartyDataState state) : this(state.Id, state.LocalUserInfo, state.Members, state.SettingsState) { }

    public void UpdateState(ClientPartyDataState state)
    {
        LocalUserInfo = state.LocalUserInfo;
        Members = state.Members;
        Settings.UpdateState(state.SettingsState);
    }
}

public sealed class PartySettings()
{
    public uint MaxMembers;

    public PartySettings(PartySettingsState state) : this()
    {
        UpdateState(state);
    }

    public void UpdateState(PartySettingsState state)
    {
        MaxMembers = state.MaxMembers;
    }
}
