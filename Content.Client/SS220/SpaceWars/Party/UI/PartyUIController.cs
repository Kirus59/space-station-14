
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client.SS220.SpaceWars.Party.UI;

[UsedImplicitly]
public sealed class PartyUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IPartyManager _party = default!;

    private PartyWindow? _window;
    private MenuButton? PartyButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.PartyButton;

    public override void Initialize()
    {
        base.Initialize();

        _party.CurrentPartyUpdated += () => _window?.Refresh();
    }

    public void OnStateEntered(GameplayState state)
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

    public void OnStateExited(GameplayState state)
    {
        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<PartyUIController>();
    }

    public void UnloadButton()
    {
        if (PartyButton == null)
            return;

        PartyButton.OnPressed -= PartyButtonPressed;
    }

    public void LoadButton()
    {
        if (PartyButton == null)
        {
            return;
        }

        PartyButton.OnPressed += PartyButtonPressed;
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

    private void DeactivateButton()
    {
        if (PartyButton == null)
        {
            return;
        }

        PartyButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (PartyButton == null)
        {
            return;
        }

        PartyButton.Pressed = true;
    }

    private void PartyButtonPressed(ButtonEventArgs args)
    {
        if (_window == null)
            return;

        PartyButton?.SetClickPressed(!_window.IsOpen);
        ToggleWindow();
    }
}
