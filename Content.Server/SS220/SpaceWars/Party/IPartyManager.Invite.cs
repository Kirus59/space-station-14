using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Player;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    void AcceptInvite(uint inviteId, ICommonSession target);
    void DenyInvite(uint inviteId, ICommonSession target);
    void DeleteInvite(PartyInvite invite);

    bool TrySendInvite(ICommonSession sender, string username, [NotNullWhen(false)] out string? failReason);
    bool TrySendInvite(ICommonSession sender, ICommonSession target, [NotNullWhen(false)] out string? failReason);
    bool TrySendInvite(PartyData party, ICommonSession sender, ICommonSession target, [NotNullWhen(false)] out string? failReason);
    void SendInvite(PartyInvite invite, ICommonSession sender, ICommonSession target);
}
