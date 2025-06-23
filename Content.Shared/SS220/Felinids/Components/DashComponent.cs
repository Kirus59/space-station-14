// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.Nutrition.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Wagging;

[RegisterComponent, NetworkedComponent]
public sealed partial class DashComponent : Component
{
    /// <summary>
    ///     Dash duration in seconds.
    /// </summary>
    [DataField]
    public int DashTime = 10;

    /// <summary>
    ///     The hunger threshold for using the dash ability.
    /// </summary>
    [DataField]
    public HungerThreshold HungerThreshold = HungerThreshold.Peckish;

    /// <summary>
    ///     The thirst threshold for using the dash ability.
    /// </summary>
    [DataField]
    public ThirstThreshold ThirstThreshold = ThirstThreshold.Thirsty;

    /// <summary>
    ///      The cost of a dash in thirst and hunger. A percentage of the maximum value.
    /// </summary>
    [DataField]
    public float DashPrice = 0.2f;

    /// <summary>
    ///     Dash speed modifier.
    /// </summary>
    [DataField]
    public float DashSpeed = 1.3f;

    /// <summary>
    ///     is need to stop dash action?
    /// </summary>
    [DataField]
    public bool NeedToStop = false;

    /// <summary>
    ///     Dash end time.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan EndTime;

    /// <summary>
    ///     Is dash active?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool Active = false;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Action = "ActionToggleDash";

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionEntity;
}
