
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
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.SS220.SpaceWars.Party.UI;

[UsedImplicitly]
public sealed class PartyUIController : UIController, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
{
    private PartyWindow? _window;
    private MenuButton? GamePartyButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.PartyButton;
    private Button? LobbyPartyButton => (UIManager.ActiveScreen as LobbyGui)?.PartyButton;

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

    private void OnStateEntered()
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<PartyWindow>();
        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenPartyWindow,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<PartyUIController>();
    }

    private void OnStateExited()
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<PartyUIController>();
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
        UIManager.ClickSound();
    }

    public void OnStateEntered(GameplayState state)
    {
        if (GamePartyButton != null)
        {
            GamePartyButton.OnPressed -= PartyButtonPressed;
            GamePartyButton.OnPressed += PartyButtonPressed;
        }

        OnStateEntered();
    }

    public void OnStateExited(GameplayState state)
    {
        if (GamePartyButton != null)
            GamePartyButton.OnPressed -= PartyButtonPressed;

        OnStateExited();
    }

    public void OnStateEntered(LobbyState state)
    {
        if (LobbyPartyButton != null)
        {
            LobbyPartyButton.OnPressed -= PartyButtonPressed;
            LobbyPartyButton.OnPressed += PartyButtonPressed;
        }

        OnStateEntered();
    }

    public void OnStateExited(LobbyState state)
    {
        if (LobbyPartyButton != null)
            LobbyPartyButton.OnPressed -= PartyButtonPressed;

        OnStateExited();
    }
}
