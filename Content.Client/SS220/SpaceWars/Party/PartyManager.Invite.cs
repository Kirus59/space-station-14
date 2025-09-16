
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<IPartyInvite>? InviteAdded;
    public event Action<IPartyInvite>? InviteRemoved;
    public event Action<IPartyInvite>? InviteUpdated;

    public IReadOnlyList<IPartyInvite> SendedInvites => [.. _invites.Where(i => i.Kind is PartyInviteKind.Sended)];
    public IReadOnlyList<IPartyInvite> ReceivedInvites => [.. _invites.Where(i => i.Kind is PartyInviteKind.Received)];
    public IReadOnlyList<IPartyInvite> AllInvites => [.. _invites];

    private readonly HashSet<PartyInvite> _invites = [];

    public void InviteInitialize()
    {
        SubscribeNetMessage<MsgPartyInviteSended>(OnPartyInviteSendedMessage);
        SubscribeNetMessage<MsgPartyInviteReceived>(OnPartyInviteReceivedMessage);
        SubscribeNetMessage<MsgPartyInviteDeleted>(OnPartyInviteDeletedMessage);
        SubscribeNetMessage<MsgHandlePartyInviteState>(OnUpdateClientPartyInviteMessage);
        SubscribeNetMessage<MsgUpdateClientPartyInvites>(OnUpdateClientPartyInvitesMessage);
    }

    private void OnPartyInviteSendedMessage(MsgPartyInviteSended message)
    {
        if (message.State is not PartyInviteState state)
            return;

        AddInvite(state, PartyInviteKind.Sended);
    }

    private void OnPartyInviteReceivedMessage(MsgPartyInviteReceived message)
    {
        if (message.State is not PartyInviteState state)
            return;

        AddInvite(state, PartyInviteKind.Received);
    }

    private void OnPartyInviteDeletedMessage(MsgPartyInviteDeleted message)
    {
        RemoveInvite(message.InviteId);
    }

    private void OnUpdateClientPartyInviteMessage(MsgHandlePartyInviteState message)
    {
        if (message.State is not PartyInviteState state)
            return;

        HandleInviteState(state);
    }

    private void OnUpdateClientPartyInvitesMessage(MsgUpdateClientPartyInvites message)
    {
        var toRemove = _invites.Select(i => i.Id).ToList();
        foreach (var state in message.States)
        {
            if (state is not PartyInviteState inviteState)
                continue;

            var id = inviteState.Id;
            toRemove.Remove(id);

            if (!InviteExist(id))
                AddInvite(inviteState);
        }

        foreach (var id in toRemove)
            RemoveInvite(id);
    }

    public bool InviteExist(uint id)
    {
        return TryGetInvite(id, out _);
    }

    public bool TryGetInvite(uint id, [NotNullWhen(true)] out IPartyInvite? invite)
    {
        invite = GetInvite(id);
        return invite != null;
    }

    public IPartyInvite? GetInvite(uint id)
    {
        return _invites.FirstOrDefault(i => i.Id == id);
    }

    public void InviteUserRequest(string username)
    {
        if (!IsLocalPartyHost)
            return;

        var msg = new MsgInviteUserRequest(username);
        SendNetMessage(msg);
    }

    public async Task<MsgInviteUserResponce> InviteUserRequestAsync(string username)
    {
        if (!IsLocalPartyHost)
            return new MsgInviteUserResponce();

        var id = GenerateMessageId();

        var msg = new MsgInviteUserRequest(username)
        {
            Id = id
        };
        SendNetMessage(msg);

        var responce = await WaitResponce<MsgInviteUserResponce>(id: id);
        return responce;
    }

    public void AcceptInviteRequest(uint inviteId)
    {
        if (!ReceivedInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new MsgAcceptInviteRequest(inviteId);
        SendNetMessage(msg);
    }

    public void DenyInviteRequest(uint inviteId)
    {
        if (!ReceivedInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new MsgDenyInviteRequest(inviteId);
        SendNetMessage(msg);
    }

    public void DeleteInviteRequest(uint inviteId)
    {
        if (!IsLocalPartyHost)
            return;

        if (!ReceivedInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new MsgInviteRequest(inviteId);
        SendNetMessage(msg);
    }

    public void SetReceiveInvitesStatus(bool receiveInvites)
    {
        var msg = new MsgSetReceiveInvitesStatus(receiveInvites);
        SendNetMessage(msg);
    }

    private bool AddInvite(PartyInviteState state, PartyInviteKind kind = PartyInviteKind.Other)
    {
        if (InviteExist(state.Id))
            return false;

        var invite = new PartyInvite(state, kind);
        return AddInvite(invite);
    }

    private bool AddInvite(PartyInvite invite)
    {
        var result = _invites.Add(invite);

        if (result)
            InviteAdded?.Invoke(invite);

        return result;
    }

    private bool RemoveInvite(uint id)
    {
        if (!TryGetInvite(id, out var invite) || invite is not PartyInvite partyInvite)
            return false;

        return RemoveInvite(partyInvite);
    }

    private bool RemoveInvite(PartyInvite invite)
    {
        var result = _invites.Remove(invite);

        if (result)
            InviteRemoved?.Invoke(invite);

        return result;
    }

    private void HandleInviteState(PartyInviteState state)
    {
        if (TryGetInvite(state.Id, out var invite) && invite is PartyInvite partyInvite)
            HandleInviteState(partyInvite, state);
    }

    private void HandleInviteState(PartyInvite invite, PartyInviteState state)
    {
        DebugTools.Assert(_invites.Contains(invite));

        invite.HandleState(state);
        InviteUpdated?.Invoke(invite);
    }
}
