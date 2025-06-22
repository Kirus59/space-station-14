// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

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
    ///     Is dash active?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsBoosted = false;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntProtoId Action = "ActionToggleDash";

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionEntity;
}
