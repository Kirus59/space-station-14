
using Content.Client.SS220.SpaceWars.Party.UI;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager : SharedPartyManager, IPartyManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;

    public event Action? CurrentPartyUpdated;

    public PartyUIController UIController => _ui.GetUIController<PartyUIController>();

    public Party? LocalParty { get; private set; }

    public bool IsLocalPartyHost => LocalMember is { } member && member.Role is PartyMemberRole.Host;

    public PartyMember? LocalMember
    {
        get
        {
            if (LocalParty is not { } party)
                return null;

            if (_player.LocalUser is not { } userId)
                return null;

            var member = party.FindMember(userId);
            DebugTools.AssertNotNull(member);
            return member;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetMessage<MsgUpdateClientParty>(OnUpdatePartyMessage);

        InviteInitialize();
        ChatInitialize();
    }

    private void OnUpdatePartyMessage(MsgUpdateClientParty message)
    {
        var state = message.State;
        if (state is null)
        {
            if (LocalParty is not null)
            {
                LocalParty = null;
                CurrentPartyUpdated?.Invoke();
                UIController.RefreshWindow();
            }

            return;
        }

        if (LocalParty is null)
            LocalParty = new Party(state.Value);
        else if (LocalParty.Id != state.Value.Id)
        {
            LocalParty.Dispose();
            LocalParty = new Party(state.Value);
        }
        else
            LocalParty.HandleState(state.Value);

        CurrentPartyUpdated?.Invoke();
        UIController.RefreshWindow();
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

    public void CreatePartyRequest(PartySettingsState? settings = null)
    {
        var msg = new MsgCreatePartyRequest(settings);
        SendNetMessage(msg);
    }

    public void DisbandPartyRequest()
    {
        var msg = new MsgDisbandPartyRequest();
        SendNetMessage(msg);
    }

    public void LeavePartyRequest()
    {
        var msg = new MsgLeavePartyRequest();
        SendNetMessage(msg);
    }

    public void KickFromPartyRequest(NetUserId userId)
    {
        var msg = new MsgKickFromPartyRequest(userId);
        SendNetMessage(msg);
    }

    public void SetSettingsRequest(PartySettingsState settingsState)
    {
        var msg = new MsgSetPartySettingsRequest(settingsState);
        SendNetMessage(msg);
    }

    public void SendInviteRequest(string username)
    {
        var msg = new MsgInviteUserRequest(username);
        SendNetMessage(msg);
    }
}
