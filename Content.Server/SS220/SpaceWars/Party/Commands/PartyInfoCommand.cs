
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;
using System.Text;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class PartyInfoCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "info";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine("Неверное число аргументов!");
            return;
        }

        if (!uint.TryParse(args[0], out var partyId))
        {
            shell.WriteLine($"{args[0]} не является числом!");
            return;
        }

        if (!_party.TryGetPartyById(partyId, out var party))
        {
            shell.WriteLine($"Не удалось найти пати с id '{partyId}'!");
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Id: {party.Id}");
        builder.AppendLine($"Host: {party.Host.Session.Name}");
        builder.AppendLine("Members:");
        foreach (var member in party.Members)
            builder.AppendLine($"- {member.Session.Name}");

        builder.AppendLine($"Status: {party.Status.ToString()}");

        shell.WriteLine(builder.ToString());
    }
}
