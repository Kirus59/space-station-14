
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.SS220.SpaceWars.Party.UI;

[UsedImplicitly]
public sealed class PartyUIController : UIController, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
{
    private PartyWindow? _window;
    private MenuButton? GamePartyButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.PartyButton;
    private Button? LobbyPartyButton => (UIManager.ActiveScreen as LobbyGui)?.PartyButton;

    private bool _bindsRegistered;

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
            .Bind(ContentKeyFunctions.OpenPartyWindow,
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
    }

    private void PartyButtonPressed(ButtonEventArgs args)
    {
        if (_window == null)
            return;

        ToggleWindow();
    }
}
