using Content.Client.SS220.Overlays;
using Content.Shared.SS220.NightVision;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.SS220.NightVision;

public sealed class NightVisionSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private NightVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, ComponentInit>(OnInit);

        _overlay = new();
    }

    private void OnInit(Entity<NightVisionComponent> entity, ref ComponentInit args)
    {
        if (_player.LocalEntity == entity.Owner)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<NightVisionComponent> entity, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == entity.Owner)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnPlayerAttached(Entity<NightVisionComponent> entity, ref PlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(Entity<NightVisionComponent> entity, ref PlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }
}
