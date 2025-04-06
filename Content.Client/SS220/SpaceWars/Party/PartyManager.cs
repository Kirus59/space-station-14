using Content.Client.SS220.CultYogg.Cultists;
using Content.Client.SS220.SpaceWars.Party.Systems;
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Content.Shared.VoiceMask;
using Robust.Client.Player;
using System.Linq;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private PartySystem? _partySystem;

    public event Action<PartyData?>? OnPartyDataUpdated;
    public event Action<PartyInvite>? OnPartyInviteUpdated;

    public PartyMenu? PartyMenu => _partyMenu;
    private PartyMenu? _partyMenu;

    public PartyData? CurrentParty => _currentParty;
    private PartyData? _currentParty;

    public PartyInvite? LastSendedInvite => _sendedInvites.LastOrDefault();

    public HashSet<PartyInvite> SendedInvites => _sendedInvites;
    private HashSet<PartyInvite> _sendedInvites = new();
    private HashSet<PartyInvite> _incomingInvites = new();

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

    public void SendInviteRequest(string username)
    {
        _partySystem?.SendInviteRequest(username);
    }

    public void UpdateInviteInfo(PartyInvite invite)
    {
        var player = _playerManager.LocalUser;
        if (player == null)
            return;

        OnPartyInviteUpdated?.Invoke(invite);
        switch (invite.InviteStatus)
        {
            case InviteStatus.Sended:
                if (invite.Sender == player)
                    _sendedInvites.Add(invite);
                else if (invite.Target == player)
                    _incomingInvites.Add(invite);
                break;

            case InviteStatus.Accepted:
            case InviteStatus.Denied:
                _sendedInvites.Remove(invite);
                _incomingInvites.Remove(invite);
                break;
        }
    }

    public PartyInvite? DequeueIncomingInvite()
    {
        if (_incomingInvites.Count <= 0)
            return null;

        var invite = _incomingInvites.First();
        _incomingInvites.Remove(invite);
        return invite;
    }

    public void AcceptInvite(PartyInvite invite)
    {
        _partySystem?.AcceptInvite(invite);
    }

    public void DenyInvite(PartyInvite invite)
    {
        _partySystem?.DenyInvite(invite);
    }

    #region PartyMenuUI
    public void SetPartyMenu(PartyMenu? partyMenu)
    {
        _partyMenu = partyMenu;
    }
    #endregion
}
