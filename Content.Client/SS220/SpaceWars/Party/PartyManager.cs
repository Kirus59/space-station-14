
using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Client.Player;
using System.Linq;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    private PartySystem? _partySystem;

    public event Action<PartyData?>? OnPartyDataUpdated;

    public PartyMenu? PartyMenu => _partyMenu;
    private PartyMenu? _partyMenu;

    public PartyData? CurrentParty => _currentParty;
    private PartyData? _currentParty;

    public override void Initialize()
    {
        base.Initialize();

        SetPartyMenu(new PartyMenu());
    }

    public void SetPartySystem(PartySystem partySystem)
    {
        _partySystem = partySystem;
    }

    public void SetPartyData(PartyData? currentParty)
    {
        _currentParty = currentParty;
        OnPartyDataUpdated?.Invoke(currentParty);
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

    #region PartyMenuUI
    public void SetPartyMenu(PartyMenu? partyMenu)
    {
        _partyMenu = partyMenu;
    }
    #endregion
}
