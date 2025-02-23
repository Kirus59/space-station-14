// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Client.Resources;
using Content.Client.SS220.Paper.Systems;
using Content.Client.SS220.StyleTools;
using Content.Client.Stylesheets;
using Content.Shared.SS220.Paper;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.SS220.Paper.UI;

[GenerateTypedNameReferences]
public sealed partial class DocumentHelperWindow : Control
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IStylesheetManager _stylesheetManager = default!;

    private const string DocumentHelperOptionLocPrefix = "document-helper-option-";

    private readonly SharedDocumentHelperSystem _documentHelper;
    private readonly Dictionary<DocumentHelperOptions, BoxContainer> _optionContainer = [];
    private bool _isExpanded = false;

    public event Action<string>? OnButtonPressed;

    public DocumentHelperWindow() : this(DocumentHelperOptions.All) { }

    public DocumentHelperWindow(DocumentHelperOptions options = DocumentHelperOptions.All)
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        Stylesheet = new DocumentHelperWindowStyle().Create(_stylesheetManager.SheetNano, _resourceCache);

        ExpandButton.OnPressed += args =>
        {
            SetExpanded(!_isExpanded);
        };
        SetExpanded(_isExpanded);

        _documentHelper = _entityManager.System<DocumentHelperSystem>();

        var optionValuesPair = _documentHelper.GetOptionValuesPair(options, _player.LocalSession?.AttachedEntity);
        GenerateOptions(optionValuesPair);
    }

    public void GenerateOptions(Dictionary<DocumentHelperOptions, List<string>> optionValuesPair)
    {
        foreach (var (option, values) in optionValuesPair)
        {
            BoxContainer container;
            if (_optionContainer.TryGetValue(option, out var dictContainer))
            {
                container = dictContainer;
                container.DisposeAllChildren();
            }
            else
            {
                container = new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Horizontal,
                    VerticalAlignment = VAlignment.Center,
                };

                OptionsContainer.AddChild(container);
            }

            var label = new Label
            {
                Text = Loc.GetString($"{DocumentHelperOptionLocPrefix + option.ToString().ToLower()}"),
                VerticalAlignment = VAlignment.Top,
                Margin = new Thickness(0, 6, 0, 0),
            };
            label.AddStyleClass("OptionLabel");
            container.AddChild(label);

            var buttonsContainer = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical
            };
            foreach (var value in values)
            {
                var button = new Button
                {
                    Text = value
                };
                button.AddStyleClass("OptionButton");
                button.OnPressed += _ => OnButtonPressed?.Invoke(button.Text);
                buttonsContainer.AddChild(button);
            }
            container.AddChild(buttonsContainer);

            _optionContainer[option] = container;
        }
    }

    public void UpdateState(DocumentHelperOptionsMessage state)
    {
        GenerateOptions(state.OptionValuesPair);
    }

    private void SetExpanded(bool isExpanded)
    {
        _isExpanded = isExpanded;
        BodyContainer.Margin = new Thickness(0, 0, isExpanded ? 0 : SetSize.X - 12, 0);
        ExpandIcon.TextureScale = new(isExpanded ? -1 : 1, 1);
    }
}

public sealed class DocumentHelperWindowStyle : QuickStyle
{
    protected override void CreateRules()
    {
        var placeholder = new StyleBoxTexture { Texture = Resources.GetTexture("/Textures/Interface/Nano/placeholder.png") };
        placeholder.SetPatchMargin(StyleBox.Margin.All, 19);
        placeholder.Mode = StyleBoxTexture.StretchMode.Tile;

        Builder.Element<Label>()
            .Prop("modulate-self", new Color(10, 10, 10));

        Builder.Element<RichTextLabel>()
            .Prop("modulate-self", new Color(10, 10, 10));

        Builder.Element<PanelContainer>().Class("PaperDefaultBorder")
            .Prop("modulate-self", Color.TryParse("#e7e4df", out var color) ? color : default);

        Builder.Element<TextureRect>().Class("ArrowRight")
            .Prop("texture", Tex("/Textures/Interface/Nano/triangle_right_hollow.svg.png"))
            .Prop("modulate-self", new Color(10, 10, 10));

        Builder.Element<Button>().Class("OptionButton")
            .Prop("stylebox", placeholder)
            .Prop("SetHeight", 34f)
            .Prop("Margin", new Thickness(6, 2));

        Builder.Element<Button>().Class("OptionButton")
            .Pseudo("normal")
            .Prop("modulate-self", Color.White);
        Builder.Element<Button>().Class("OptionButton")
            .Pseudo("hover")
            .Prop("modulate-self", Color.Gray);
        Builder.Element<Button>().Class("OptionButton")
            .Pseudo("pressed")
            .Prop("modulate-self", Color.Black);
        Builder.Element<Button>().Class("OptionButton")
            .Pseudo("disabled")
            .Prop("modulate-self", Color.White);

        Builder.Element<Button>().Class("OptionButton")
            .Child<Label>()
            .Prop("Margin", new Thickness(0, -14));
    }
}
