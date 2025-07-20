
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Client.Player;
using Robust.Shared.Network;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

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

        SubscribeNetMessage<SetCurrentPartyMessage>(OnSetCurrentParty);
        SubscribeNetMessage<UpdateCurrentPartyMessage>(OnUpdatePartyDataInfo);

        InviteInitialize();
        ChatInitialize();
    }

    private void OnSetCurrentParty(SetCurrentPartyMessage message)
    {
        SetCurrentParty(message.State);
    }

    private void OnUpdatePartyDataInfo(UpdateCurrentPartyMessage message)
    {
        UpdateCurrentParty(message.State);
    }

    private void SendNetMessage(PartyMessage message)
    {
        if (_player.LocalSession is { } session)
            message.Sender = session.UserId;

        var msg = new PartyNetMessage
        {
            Message = message,
        };

        _net.ClientSendMessage(msg);
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
        var msg = new CreatePartyRequestMessage(settings);
        SendNetMessage(msg);
    }

    public void SendDisbandPartyRequest()
    {
        var msg = new DisbandPartyRequestMessage();
        SendNetMessage(msg);
    }

    public void SendLeavePartyRequest()
    {
        var msg = new LeavePartyRequestMessage();
        SendNetMessage(msg);
    }

    public void SendKickFromPartyRequest(uint partyUserId)
    {
        var msg = new KickFromPartyRequestMessage(partyUserId);
        SendNetMessage(msg);
    }

    public void SetSettingsRequest(PartySettingsState settingsState)
    {
        var msg = new SetPartySettingsRequestMessage(settingsState);
        SendNetMessage(msg);
    }

    public void SendInviteRequest(string username)
    {
        var msg = new InviteInPartyRequestMessage(username);
        SendNetMessage(msg);
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
