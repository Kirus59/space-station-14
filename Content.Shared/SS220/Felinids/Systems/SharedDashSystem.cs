// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Toggleable;
using Content.Shared.Wagging;
using Robust.Shared.Timing;

namespace Content.Shared.SS220.Felinids.Systems;

public sealed class SharedDashSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DashComponent, MapInitEvent>(OnDashMapInit);
        SubscribeLocalEvent<DashComponent, ComponentShutdown>(OnDashShutdown);
        SubscribeLocalEvent<DashComponent, ToggleActionEvent>(OnDashToggle);
        SubscribeLocalEvent<DashComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnDashMapInit(EntityUid uid, DashComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.ActionEntity, component.Action, uid);
    }

    private void OnDashShutdown(EntityUid uid, DashComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ActionEntity);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DashComponent>();
        while (query.MoveNext(out var uid, out var dashComp))
        {
            if (!dashComp.NeedToStop
                && dashComp.Active
                && dashComp.EndTime <= _gameTiming.CurTime)
                dashComp.NeedToStop = true;

            if (!dashComp.NeedToStop
                && dashComp.Active
                && TryComp<MobStateComponent>(uid, out var mobState)
                && mobState.CurrentState >= MobState.Critical)
                dashComp.NeedToStop = true;

            if (dashComp.NeedToStop)
            {
                dashComp.Active = false;
                dashComp.NeedToStop = false;
                _speedModifier.RefreshMovementSpeedModifiers(uid);
                if (TryComp<HungerComponent>(uid, out var hunger))
                    _hunger.SetHunger(uid, hunger.LastAuthoritativeHungerValue - hunger.Thresholds[HungerThreshold.Overfed] * dashComp.DashPrice, hunger);
                if (TryComp<ThirstComponent>(uid, out var thirst))
                    _thirst.SetThirst(uid, thirst, thirst.CurrentThirst - thirst.ThirstThresholds[ThirstThreshold.OverHydrated] * dashComp.DashPrice);
            }
        }
    }

    private void OnDashToggle(EntityUid uid, DashComponent dashComp, ref ToggleActionEvent args)
    {
        if (args.Handled || dashComp.Active || dashComp.NeedToStop)
            return;
        args.Handled = true;

        if (TryComp<HungerComponent>(uid, out var hunger)
            && hunger.CurrentThreshold < dashComp.HungerThreshold)
        {
            _popup.PopupClient(Loc.GetString("popup-dash-no-hunger"), uid, PopupType.Small);
            _actions.ClearCooldown(dashComp.ActionEntity);
            return;
        }

        if (TryComp<ThirstComponent>(uid, out var thirst)
            && thirst.CurrentThirstThreshold < dashComp.ThirstThreshold)
        {
            _popup.PopupClient(Loc.GetString("popup-dash-no-thirst"), uid, PopupType.Small);
            _actions.ClearCooldown(dashComp.ActionEntity);
            return;
        }

        dashComp.Active = true;
        _speedModifier.RefreshMovementSpeedModifiers(uid);
        dashComp.EndTime = _gameTiming.CurTime + TimeSpan.FromSeconds(dashComp.DashTime);
    }

    private void OnRefreshSpeed(EntityUid uid, DashComponent dashComp, RefreshMovementSpeedModifiersEvent args)
    {
        if (dashComp.Active)
            args.ModifySpeed(dashComp.DashSpeed);
    }
}
