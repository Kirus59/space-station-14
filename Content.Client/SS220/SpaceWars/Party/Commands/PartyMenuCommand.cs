
using JetBrains.Annotations;
using Robust.Shared.Console;

namespace Content.Client.SS220.SpaceWars.Party.Commands;

[UsedImplicitly]
public sealed class PartyMenuCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _partyManager = default!;

    public override string Command => "partymenu";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _partyManager.PartyMenu?.OpenCentered();
    }
}
