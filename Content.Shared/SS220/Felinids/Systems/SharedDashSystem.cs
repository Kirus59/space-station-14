// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Actions;
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

    private void OnDashToggle(EntityUid uid, DashComponent component, ref ToggleActionEvent args)
    {
        if (args.Handled || component.IsBoosted)
            return;

        args.Handled = true;

        if (TryComp<HungerComponent>(uid, out var hunger)
            && hunger.CurrentThreshold is HungerThreshold.Peckish)
        {
            _popup.PopupClient(Loc.GetString("popup-dash-no-hunger"), uid, PopupType.Small);
            return;
        }

        if (TryComp<ThirstComponent>(uid, out var thirst)
            && thirst.CurrentThirstThreshold == ThirstThreshold.Parched)
        {
            _popup.PopupClient(Loc.GetString("popup-dash-no-thirst"), uid, PopupType.Small);
            return;
        }

        component.IsBoosted = true;
        _speedModifier.RefreshMovementSpeedModifiers(uid);

        Timer.Spawn(component.DashTime * 1000, () =>
        {
            if (!uid.Valid)
                return;

            if (TryComp<DashComponent>(uid, out var comp))
                comp.IsBoosted = false;
            _speedModifier.RefreshMovementSpeedModifiers(uid);

            if (hunger != null)
                _hunger.SetHunger(uid, hunger.LastAuthoritativeHungerValue - hunger.Thresholds[HungerThreshold.Overfed] * 0.2f, hunger);
            if (thirst != null)
                _thirst.SetThirst(uid, thirst, thirst.CurrentThirst - thirst.ThirstThresholds[ThirstThreshold.OverHydrated] * 0.2f);
        });
    }

    private void OnRefreshSpeed(EntityUid uid, DashComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (component.IsBoosted)
            args.ModifySpeed(1.3f);
    }
}
