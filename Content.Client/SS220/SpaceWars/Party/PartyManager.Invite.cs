// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInvite>? ReceivedInviteAdded;
    public event Action<PartyInvite>? ReceivedInviteRemoved;
    public event Action<PartyInvite>? ReceivedInviteUpdated;

    public IReadOnlyList<PartyInvite> ReceivedInvites => [.. _receiveInvites];
    private readonly HashSet<PartyInvite> _receiveInvites = [];

    public void InviteInitialize()
    {
        SubscribeNetMessage<MsgPartyInviteReceived>(OnPartyInviteReceivedMessage);
        SubscribeNetMessage<MsgPartyInviteDeleted>(OnPartyInviteDeletedMessage);
        SubscribeNetMessage<MsgHandlePartyInviteState>(OnUpdateClientPartyInviteMessage);
        SubscribeNetMessage<MsgUpdateReceivedPartyInvitesList>(OnUpdateClientPartyInvitesMessage);
    }

    private void OnPartyInviteReceivedMessage(MsgPartyInviteReceived message)
    {
        AddInvite(message.State);
    }

    private void OnPartyInviteDeletedMessage(MsgPartyInviteDeleted message)
    {
        RemoveInvite(message.InviteId);
    }

    private void OnUpdateClientPartyInviteMessage(MsgHandlePartyInviteState message)
    {
        HandleInviteState(message.State);
    }

    private void OnUpdateClientPartyInvitesMessage(MsgUpdateReceivedPartyInvitesList message)
    {
        var toRemove = _receiveInvites.Select(i => i.Id).ToList();
        foreach (var state in message.States)
        {
            toRemove.Remove(state.Id);

            if (!InviteExist(state.Id))
                AddInvite(state);
        }

        foreach (var id in toRemove)
            RemoveInvite(id);
    }

    public bool InviteExist(uint id)
    {
        return TryGetInvite(id, out _);
    }

    public bool TryGetInvite(uint id, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(id);
        return invite != null;
    }

    public PartyInvite? GetInvite(uint id)
    {
        return _receiveInvites.FirstOrDefault(i => i.Id == id);
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

    public void AcceptInviteRequest(uint id)
    {
        if (!InviteExist(id))
            return;

        var msg = new MsgAcceptInviteRequest(id);
        SendNetMessage(msg);
    }

    public void DenyInviteRequest(uint id)
    {
        if (!InviteExist(id))
            return;

        var msg = new MsgDenyInviteRequest(id);
        SendNetMessage(msg);
    }

    public void DeleteInviteRequest(uint id)
    {
        if (!IsLocalPartyHost)
            return;

        if (!InviteExist(id))
            return;

        var msg = new MsgDeleteInviteRequest(id);
        SendNetMessage(msg);
    }

    public void SetReceiveInvitesStatus(bool receiveInvites)
    {
        var msg = new MsgSetReceiveInvitesStatus(receiveInvites);
        SendNetMessage(msg);
    }

    private bool AddInvite(PartyInviteState state)
    {
        if (InviteExist(state.Id))
            return false;

        var invite = new PartyInvite(state);
        return AddInvite(invite);
    }

    private bool AddInvite(PartyInvite invite)
    {
        var result = _receiveInvites.Add(invite);

        if (result)
            ReceivedInviteAdded?.Invoke(invite);

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
        var result = _receiveInvites.Remove(invite);

        if (result)
            ReceivedInviteRemoved?.Invoke(invite);

        return result;
    }

    private void HandleInviteState(PartyInviteState state)
    {
        if (TryGetInvite(state.Id, out var invite))
            HandleInviteState(invite, state);
    }

    private void HandleInviteState(PartyInvite invite, PartyInviteState state)
    {
        DebugTools.Assert(_receiveInvites.Contains(invite));

        invite.HandleState(state);
        ReceivedInviteUpdated?.Invoke(invite);
    }
}
