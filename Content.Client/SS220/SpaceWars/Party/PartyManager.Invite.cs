
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

    public IEnumerable<PartyInvite> ExternalInvites => _invites.Where(i => i.InviteType is PartyInviteType.External);
    public IEnumerable<PartyInvite> InternalInvites => _invites.Where(i => i.InviteType is PartyInviteType.Internal);
    public IEnumerable<PartyInvite> AllInvites => _invites.ToHashSet();

    private readonly HashSet<PartyInvite> _invites = [];

    public void InviteInitialize()
    {
        SubscribeNetMessage<PartyInviteReceivedMessage>(OnPartyInviteReceivedMessage);
        SubscribeNetMessage<PartyInviteDeletedMessage>(OnPartyInviteDeletedMessage);
        SubscribeNetMessage<HandlePartyInviteStateMessage>(OnUpdateClientPartyInviteMessage);
        SubscribeNetMessage<UpdateClientPartyInvitesMessage>(OnUpdateClientPartyInvitesMessage);
    }

    private void OnPartyInviteReceivedMessage(PartyInviteReceivedMessage message)
    {
        AddInvite(message.State);
    }

    private void OnPartyInviteDeletedMessage(PartyInviteDeletedMessage message)
    {
        RemoveInvite(message.InviteId);
    }

    private void OnUpdateClientPartyInviteMessage(HandlePartyInviteStateMessage message)
    {
        HandleInviteState(message.State);
    }

    private void OnUpdateClientPartyInvitesMessage(UpdateClientPartyInvitesMessage message)
    {
        var toRemove = _invites.Select(i => i.Id).ToList();
        foreach (var state in message.States)
        {
            toRemove.Remove(state.Id);
            AddInvite(state);
        }

        foreach (var id in toRemove)
            RemoveInvite(id);
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
        if (!ExternalInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new AcceptInviteRequestMessage(inviteId);
        SendNetMessage(msg);
    }

    public void DenyInviteRequest(uint inviteId)
    {
        if (!ExternalInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new DenyInviteRequestMessage(inviteId);
        SendNetMessage(msg);
    }

    public void DeleteInviteRequest(uint inviteId)
    {
        if (!IsLocalPartyHost)
            return;

        if (!InternalInvites.Any(i => i.Id == inviteId))
            return;

        var msg = new DeleteInviteRequestMessage(inviteId);
        SendNetMessage(msg);
    }

    public void SetReceiveInvitesStatus(bool receiveInvites)
    {
        var msg = new SetReceiveInvitesStatusMessage(receiveInvites);
        SendNetMessage(msg);
    }

    private bool AddInvite(PartyInviteState state)
    {
        if (TryGetInvite(state.Id, out _))
            return false;

        var invite = new PartyInvite(state);
        return AddInvite(invite);
    }

    private bool AddInvite(PartyInvite invite)
    {
        UpdateInviteType(invite);
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

    private void UpdateInviteType(PartyInvite invite)
    {
        var type = LocalParty?.Id == invite.Id ? PartyInviteType.Internal : PartyInviteType.External;
        invite.InviteType = type;
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
        UpdateInviteType(invite);

        InviteUpdated?.Invoke(invite);
    }
}
