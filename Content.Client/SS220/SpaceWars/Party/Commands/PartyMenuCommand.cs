
using JetBrains.Annotations;
using Robust.Shared.Console;

namespace Content.Client.SS220.SpaceWars.Party.Commands;

[UsedImplicitly]
public sealed class PartyMenuCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => "partywindow";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _party.UIController.ToggleWindow();
    }
}
