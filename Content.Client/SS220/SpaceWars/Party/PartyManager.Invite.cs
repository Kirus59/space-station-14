
using Content.Shared.SS220.SpaceWars.Party;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInvite>? InviteAdded;
    public event Action<PartyInvite>? InviteRemoved;
    public event Action<PartyInvite>? InviteUpdated;

    public IEnumerable<PartyInvite> ReceivedInvites => _invites.Where(i => i.Receiver == _player.LocalUser);
    public IEnumerable<PartyInvite> LocalPartyInvites => _invites.Where(i => i.PartyId == LocalParty?.Id);
    public IEnumerable<PartyInvite> AllInvites => _invites.ToHashSet();

    private readonly HashSet<PartyInvite> _invites = [];

    public void InviteInitialize()
    {
        SubscribeNetMessage<UpdateClientPartyInviteMessage>(OnUpdateClientPartyInviteMessage);
        SubscribeNetMessage<UpdateClientPartyInvitesMessage>(OnUpdateClientPartyInvitesMessage);
    }

    private void OnUpdateClientPartyInviteMessage(UpdateClientPartyInviteMessage message)
    {
        UpdateInvite(message.State);
    }

    private void OnUpdateClientPartyInvitesMessage(UpdateClientPartyInvitesMessage message)
    {
        foreach (var state in message.States)
            UpdateInvite(state);
    }

    public bool TryGetInvite(uint id, [NotNullWhen(true)] out PartyInvite? invite)
    {
        invite = GetInvite(id);
        return invite != null;
    }

    public PartyInvite? GetInvite(uint id)
    {
        var result = _invites.Where(i => i.Id == id);
        var count = result.Count();
        if (count <= 0)
            return null;

        DebugTools.Assert(count == 1);
        return result.First();
    }

    public void InviteUserRequest(string username)
    {
        if (!IsLocalPartyHost)
            return;

        var msg = new InviteUserRequestMessage(username);
        SendNetMessage(msg);
    }

    public async Task<InviteUserResponceMessage> InviteUserRequestAsync(string username)
    {
        if (!IsLocalPartyHost)
            return new InviteUserResponceMessage();

        var id = GenerateMessageId();

        var msg = new InviteUserRequestMessage(username)
        {
            Id = id
        };
        SendNetMessage(msg);

        var responce = await WaitResponce<InviteUserResponceMessage>(id: id);
        return responce;
    }

    public void AcceptInviteRequest(uint inviteId)
    {
        if (!ReceivedInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new AcceptInviteRequestMessage(inviteId);
        SendNetMessage(msg);
    }

    public void DenyInviteRequest(uint inviteId)
    {
        if (!ReceivedInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new DenyInviteRequestMessage(inviteId);
        SendNetMessage(msg);
    }

    public void DeleteInviteRequest(uint inviteId)
    {
        if (!IsLocalPartyHost)
            return;

        if (!LocalPartyInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new DeleteInviteRequestMessage(inviteId);
        SendNetMessage(msg);
    }

    public void SetReceiveInvitesStatus(bool receiveInvites)
    {
        var msg = new SetReceiveInvitesStatusMessage(receiveInvites);
        SendNetMessage(msg);
    }

    private void UpdateInvite(PartyInviteState state)
    {
        HandleInviteState(state);

        if (state.Status is PartyInviteStatus.Deleted)
            RemoveInvite(state.Id);
        else
            AddInvite(new PartyInvite(state));
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
        if (!TryGetInvite(id, out var invite))
            return false;

        return RemoveInvite(invite);
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
        if (TryGetInvite(state.Id, out var invite))
            HandleInviteState(invite, state);
    }

    private void HandleInviteState(PartyInvite invite, PartyInviteState state)
    {
        DebugTools.Assert(_invites.Contains(invite));
        invite.HandleState(state);

        InviteUpdated?.Invoke(invite);
    }
}
