
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Console;
using System.Text;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class ListPartiesCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "list";

    private const string IdTitle = "Id";
    private const string HostTitle = "Host";
    private const string MembersTitle = "Members";
    private const string StatusTitle = "Status";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var parties = _party.Parties;
        var format = GetStringFormat(parties);

        var builder = new StringBuilder();
        var title = string.Format(format, IdTitle, HostTitle, MembersTitle, StatusTitle);
        builder.AppendLine(title);

        var devider = new string('=', title.Length);
        builder.AppendLine(devider);

        foreach (var party in parties)
        {
            var partyLine = string.Format(format, party.Id, party.Host.Session.Name, party.Members.Count, party.Status.ToString());
            builder.AppendLine(partyLine);
        }

        shell.WriteLine(builder.ToString());
    }

    private static string GetStringFormat(IEnumerable<Party> parties)
    {
        const int tab = 4;

        var idWidth = tab;
        var hostWidth = tab;
        var membersWidth = tab;
        var statusWidth = tab;

        CalculateExtraWidth(IdTitle, ref idWidth);
        CalculateExtraWidth(HostTitle, ref hostWidth);
        CalculateExtraWidth(MembersTitle, ref membersWidth);
        CalculateExtraWidth(StatusTitle, ref statusWidth);

        foreach (var party in parties)
        {
            CalculateExtraWidth(party.Id.ToString(), ref idWidth);
            CalculateExtraWidth(party.Host.Session.Name, ref hostWidth);
            CalculateExtraWidth(party.Members.Count.ToString(), ref membersWidth);
            CalculateExtraWidth(party.Status.ToString(), ref statusWidth);
        }

        return $"{{0,-{idWidth}}}{{1,-{hostWidth}}}{{2,-{membersWidth}}}{{3,-{statusWidth}}}";

        static void CalculateExtraWidth(string param, ref int paramWidth)
        {
            var diff = param.Length - paramWidth;

            switch (diff)
            {
                case int i when i == 0:
                    paramWidth += tab;
                    return;

                case int i when i > 0:
                    paramWidth += (int)Math.Ceiling((double)diff / tab) * tab;
                    return;

                default:
                    return;
            }
        }
    }
}
