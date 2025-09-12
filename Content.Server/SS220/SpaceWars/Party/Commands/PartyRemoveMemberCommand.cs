// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

[AdminCommand(AdminFlags.Moderator)]
public sealed class PartyRemoveMemberCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "removemember";
    public override string Description => Loc.GetString("cmd-party-remove-member-desc");
    public override string Help => Loc.GetString("cmd-party-remove-member-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-invalid-arguments-count", ("help", Help)));
            return;
        }

        if (!uint.TryParse(args[0], out var partyId))
        {
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-invalid-argument-1", ("arg", args[0])));
            return;
        }

        if (!_party.TryGetPartyById(partyId, out var party))
        {
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-invalid-party-id", ("id", partyId)));
            return;
        }

        if (!_player.TryGetSessionByUsername(args[1], out var session))
        {
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-invalid-username", ("username", args[1])));
            return;
        }

        if (!party.ContainsMember(session))
        {
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-user-is-not-in-party", ("user", session.Name), ("partyId", party.Id)));
            return;
        }

        if (party.IsHost(session))
        {
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-user-is-host", ("user", session.Name), ("partyId", party.Id)));
            return;
        }

        try
        {
            _party.RemoveMember(party, session);
            shell.WriteLine(Loc.GetString("cmd-party-remove-member-success"));
        }
        catch (Exception e)
        {
            shell.WriteLine(e.Message);
            return;
        }
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(_party.GetPartiesCompletionOptions(), Loc.GetString("cmd-party-remove-member-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("cmd-party-remove-member-hint-2")),
            _ => CompletionResult.Empty
        };
    }
}
