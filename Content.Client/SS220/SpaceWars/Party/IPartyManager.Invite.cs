using Content.Shared.SS220.SpaceWars.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public partial interface IPartyManager
{
    event Action<PartyInvite>? OnOutgoingInviteAdded;
    event Action<PartyInvite>? OnOutgoingInviteRemoved;
    event Action<PartyInvite>? OnIncomingInviteAdded;
    event Action<PartyInvite>? OnIncomingInviteRemoved;

    public void HandleInviteState(PartyInviteState state);


    public void HandleInviteState(PartyInviteState state, PartyInvite invite);


    public void AddOutgoingInvite(PartyInvite invite);


    public void RemoveOutgoingInvite(PartyInvite invite);


    public void AddIncomingInvite(PartyInvite invite);


    public void RemoveIncomingInvite(PartyInvite invite);
}
