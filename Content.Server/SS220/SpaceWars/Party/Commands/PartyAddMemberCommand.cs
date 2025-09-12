// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt

using Content.Shared.SS220.SpaceWars.Party;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server.SS220.SpaceWars.Party.Commands;

public sealed class PartyAddMemberCommand : LocalizedCommands
{
    [Dependency] private readonly IPartyManager _party = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override string Command => SharedPartyManager.CommandsPrefix + "adduser";
    public override string Description => Loc.GetString("cmd-party-add-member-desc");
    public override string Help => Loc.GetString("cmd-party-add-member-help");

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length < 2 || args.Length > 3)
        {
            shell.WriteLine(Loc.GetString("cmd-party-add-member-invalid-arguments-count", ("help", Help)));
            return;
        }

        if (!uint.TryParse(args[0], out var partyId))
        {
            shell.WriteLine(Loc.GetString("cmd-party-add-member-invalid-arguments-1", ("arg", args[0])));
            return;
        }

        if (!_party.TryGetPartyById(partyId, out var party))
        {
            shell.WriteLine(Loc.GetString("cmd-party-add-member-invalid-party-id", ("id", partyId)));
            return;
        }

        if (!_player.TryGetSessionByUsername(args[1], out var session))
        {
            shell.WriteLine(Loc.GetString("cmd-party-add-member-invalid-username", ("username", args[1])));
            return;
        }

        var force = false;
        if (args.Length >= 3 && !bool.TryParse(args[2], out force))
        {
            shell.WriteLine(Loc.GetString("cmd-party-add-member-invalid-argument-3", ("arg", args[2])));
            return;
        }

        if (!force)
        {
            if (_party.IsAnyPartyMember(session))
            {
                shell.WriteLine(Loc.GetString("cmd-party-add-member-user-is-another-party-member", ("user", session.Name)));
                return;
            }

            if (party.LimitReached)
            {
                shell.WriteLine(Loc.GetString("cmd-party-add-member-members-limit-reached"));
                return;
            }
        }

        try
        {
            _party.AddMember(party, session, force: force, ignoreLimit: force);
            shell.WriteLine(Loc.GetString("cmd-party-add-member-success"));
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
            1 => CompletionResult.FromHintOptions(_party.GetPartiesCompletionOptions(), Loc.GetString("cmd-party-add-member-hint-1")),
            2 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), Loc.GetString("cmd-party-add-member-hint-2")),
            3 => CompletionResult.FromHintOptions(CompletionHelper.Booleans, Loc.GetString("cmd-party-add-member-hint-3")),
            _ => CompletionResult.Empty
        };
    }
}
