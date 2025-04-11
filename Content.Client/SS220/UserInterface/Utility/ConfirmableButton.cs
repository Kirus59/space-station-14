
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using System.Threading.Tasks;

namespace Content.Client.SS220.UserInterface.Utility;

public sealed class ConfirmableButton : Button
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public Action? OnConfirmed;

    public ConfirmableButtonClicksAction ClicksActionWhenConfirmed = ConfirmableButtonClicksAction.Reset;
    public ConfirmableButtonClicksAction ClicksActionWhenNotConfirmed = ConfirmableButtonClicksAction.Reset;

    public TimeSpan ConfirmDelay = TimeSpan.FromMilliseconds(5000);

    public uint ClicksForConfirm = 2;

    private TimeSpan _lastClick = TimeSpan.Zero;

    private int _loopedUpdateRate = 10;

    private int _curClicks = 0;
    private Dictionary<uint, ConfirmableButtonState> _clickStates = new();

    public ConfirmableButton(ConfirmableButtonState? defaultState = null)
    {
        IoCManager.InjectDependencies(this);

        OnPressed += _ => ProcessClick();
        if (defaultState != null)
            SetDefaultState(defaultState.Value);
        else
            _clickStates[0] = new ConfirmableButtonState(Text, ModulateSelfOverride);

        LoopedUpdate();
    }

    public ConfirmableButton(string? text, Color? overrideColor) : this(new ConfirmableButtonState(text, overrideColor)) { }

    public void SetDefaultState(ConfirmableButtonState state)
    {
        _clickStates[0] = state;
        UpdateState();
    }

    public void SetClickState(Dictionary<uint, ConfirmableButtonState> clickStates)
    {
        foreach (var (key, value) in clickStates)
        {
            SetClickState(key, value);
        }
    }

    public void SetClickState(uint click, ConfirmableButtonState state)
    {
        if (click == 0) /// Use <see cref="SetDefaultState(ConfirmableButtonState)"/> for default state
            return;

        _clickStates[click] = state;
        UpdateState();
    }

    public void Update()
    {
        if (Disposed)
            return;

        if (_curClicks >= ClicksForConfirm)
            Confirmed();

        if (_curClicks != 0 && _gameTiming.CurTime >= _lastClick + ConfirmDelay)
            NotConfirmed();

        UpdateState();
    }

    private async void LoopedUpdate()
    {
        await Task.Delay(1000 / _loopedUpdateRate);
        LoopedUpdate();

        if (_curClicks > 0)
            Update();
    }

    private void UpdateState()
    {
        if (Disposed)
            return;

        if (_clickStates.TryGetValue((uint)_curClicks, out var state))
        {
            Text = state.Text;
            ModulateSelfOverride = state.OverrideColor;
        }
    }

    private void ProcessClick()
    {
        _lastClick = _gameTiming.CurTime;
        IncreaceClicks();
    }

    private void Confirmed()
    {
        OnConfirmed?.Invoke();
        ProcessActionWithClicks(ClicksActionWhenConfirmed);
    }

    private void NotConfirmed()
    {
        ProcessActionWithClicks(ClicksActionWhenNotConfirmed);
    }

    private void ResetClicks()
    {
        _curClicks = 0;
        Update();
    }

    private void IncreaceClicks()
    {
        _curClicks++;
        Update();
    }

    private void DecreaceClicks()
    {
        var newValue = _curClicks - 1;
        _curClicks = Math.Max(newValue, 0);
        Update();
    }

    private void ProcessActionWithClicks(ConfirmableButtonClicksAction action)
    {
        switch (action)
        {
            case ConfirmableButtonClicksAction.Decreace:
                DecreaceClicks();
                break;

            case ConfirmableButtonClicksAction.Increace:
                DecreaceClicks();
                break;

            case ConfirmableButtonClicksAction.Reset:
                ResetClicks();
                break;
        }
    }
}

public record struct ConfirmableButtonState(string? Text, Color? OverrideColor);

public enum ConfirmableButtonClicksAction
{
    None,
    Decreace,
    Increace,
    Reset
}
