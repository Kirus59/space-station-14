using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;


namespace Content.Client.SS220.Overlays
{
    public sealed class NightVisionOverlay : Overlay
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ILightManager _lightManager = default!;

        public override bool RequestScreenTexture => true;

        public override OverlaySpace Space => OverlaySpace.WorldSpace;
        private readonly ShaderInstance _greyscaleShader;
        private readonly ShaderInstance _circleMaskShader;

        public NightVisionOverlay()
        {
            IoCManager.InjectDependencies(this);
            _greyscaleShader = _prototypeManager.Index<ShaderPrototype>("GreyscaleFullscreen").InstanceUnique();
            _circleMaskShader = _prototypeManager.Index<ShaderPrototype>("CircleMask").InstanceUnique();
        }

        protected override bool BeforeDraw(in OverlayDrawArgs args)
        {
            return base.BeforeDraw(args);
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            if (ScreenTexture is null)
                return;

            var playerEntity = _playerManager.LocalSession?.AttachedEntity;

            if (playerEntity is null)
                return;

            if (_entityManager.TryGetComponent<EyeComponent>(playerEntity, out var content))
            {
                _circleMaskShader?.SetParameter("Zoom", content.Zoom.X);
            }

            _greyscaleShader?.SetParameter("SCREEN_TEXTURE", ScreenTexture);

            var worldHandle = args.WorldHandle;
            var viewport = args.WorldBounds;
            worldHandle.UseShader(_greyscaleShader);
            worldHandle.DrawRect(viewport, Color.Green);

            _lightManager.DrawShadows = false;
            _circleMaskShader?.SetParameter("CircleRadius", 60f);
            worldHandle.UseShader(_circleMaskShader);
            worldHandle.DrawRect(viewport, Color.Green);
            worldHandle.UseShader(null);
        }
    }
}
