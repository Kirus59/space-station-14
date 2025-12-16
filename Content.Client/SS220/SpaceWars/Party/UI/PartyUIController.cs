// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared.SS220.Input;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.SS220.SpaceWars.Party.UI;

[UsedImplicitly]
public sealed class PartyUIController : UIController, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
{
    [Dependency] private readonly IPartyManager _party = default!;

    private PartyWindow? _window;
    private MenuButton? GamePartyButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.PartyButton;
    private Button? LobbyPartyButton => (UIManager.ActiveScreen as LobbyGui)?.PartyButton;

    private bool _bindsRegistered;
    private bool _hasUnreadInfo;

    public static readonly Color DefaultBackgroundColor = new(60, 60, 60);
    public static readonly Color DefaultInnerBackgroundColor = new(32, 32, 40);

    public override void Initialize()
    {
        base.Initialize();

        _party.LocalPartyChanged += OnLocalPartyChanged;
        _party.ChatMessageReceived += _ => UnreadInfoReceived();

        _party.ReceivedInviteAdded += _ => UnreadInfoReceived();
        _party.ReceivedInviteRemoved += _ => UnreadInfoReceived();
    }

    private void OnLocalPartyChanged(Party? party)
    {
        if (party != null)
        {
            party.HostChanged += _ => UnreadInfoReceived();
            party.MemberAdded += _ => UnreadInfoReceived();
            party.MemberRemoved += _ => UnreadInfoReceived();
        }

        UnreadInfoReceived();
    }

    public void LoadButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed += PartyButtonPressed;

        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed += PartyButtonPressed;
    }

    public void UnloadButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed -= PartyButtonPressed;

        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
    }

    public void OpenWindow()
    {
        _window?.OpenCentered();
    }

    public void CloseWindow()
    {
        _window?.Close();
    }

    public void ToggleWindow()
    {
        if (_window == null)
            return;

        if (_window.IsOpen)
            CloseWindow();
        else
            OpenWindow();
    }

    public void RefreshWindow()
    {
        _window?.Refresh();
    }

    public void OnStateEntered(GameplayState state)
    {
        if (GamePartyButton != null)
        {
            GamePartyButton.OnPressed -= PartyButtonPressed;
            GamePartyButton.OnPressed += PartyButtonPressed;
        }

        if (_window != null &&
            _window.IsOpen)
            ActivateButton();
        else
            DeactivateButton();

        EnsureWindow();
        EnsureBinds();
    }

    public void OnStateExited(GameplayState state)
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed -= PartyButtonPressed;
    }

    public void OnStateEntered(LobbyState state)
    {
        if (LobbyPartyButton != null)
        {
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
            LobbyPartyButton.OnPressed += PartyButtonPressed;
        }

        if (_window != null &&
            _window.IsOpen)
            ActivateButton();
        else
            DeactivateButton();

        EnsureWindow();
        EnsureBinds();
    }

    public void OnStateExited(LobbyState state)
    {
        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
    }

    private void EnsureWindow()
    {
        if (_window != null)
            return;

        _window = new PartyWindow();
        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;
    }

    private void ClearWindow()
    {
        _window?.Close();
        _window = null;
    }

    private void EnsureBinds()
    {
        if (_bindsRegistered)
            return;

        CommandBinds.Builder
            .Bind(KeyFunctions220.TogglePartyWindow,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<PartyUIController>();

        _bindsRegistered = true;
    }

    private void ClearBinds()
    {
        CommandBinds.Unregister<PartyUIController>();
        _bindsRegistered = false;
    }

    private void DeactivateButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.Pressed = false;

        if (LobbyPartyButton != null)
            LobbyPartyButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (GamePartyButton != null)
            GamePartyButton.Pressed = true;

        if (LobbyPartyButton != null)
            LobbyPartyButton.Pressed = true;

        UnreadInfoRead();
    }

    private void PartyButtonPressed(ButtonEventArgs args)
    {
        if (_window == null)
            return;

        ToggleWindow();
    }

    private void UnreadInfoReceived()
    {
        if (_hasUnreadInfo || _window?.IsOpen is true)
            return;

        GamePartyButton?.StyleClasses.Add(MenuButton.StyleClassRedTopButton);
        LobbyPartyButton?.StyleClasses.Add(StyleNano.StyleClassButtonColorRed);
        _hasUnreadInfo = true;
    }

    private void UnreadInfoRead()
    {
        if (!_hasUnreadInfo)
            return;

        GamePartyButton?.StyleClasses.Remove(MenuButton.StyleClassRedTopButton);
        LobbyPartyButton?.StyleClasses.Remove(StyleNano.StyleClassButtonColorRed);
        _hasUnreadInfo = false;
    }
}
