// © SS220, An EULA/CLA with a hosting restriction, full text: https://raw.githubusercontent.com/SerbiaStrong-220/space-station-14/master/CLA.txt
using Content.Shared.SS220.SpaceWars.Party;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Content.Client.SS220.SpaceWars.Party;

public sealed partial class PartyManager
{
    public event Action<PartyInvite>? ReceivedInviteAdded;
    public event Action<PartyInvite>? ReceivedInviteRemoved;
    public event Action<PartyInvite>? ReceivedInviteUpdated;

    public IReadOnlyList<PartyInvite> ReceivedInvites => [.. _receiveInvites.Values];
    private readonly Dictionary<uint, PartyInvite> _receiveInvites = [];

    private readonly PartyInviteStatus[] _validReceivedInviteStatuses =
    [
        PartyInviteStatus.Created,
        PartyInviteStatus.Sended,
    ];

    public void InviteInitialize()
    {
        SubscribeNetMessage<MsgHandleReceivedPartyInviteState>(OnUpdateClientPartyInviteMessage);
        SubscribeNetMessage<MsgUpdateReceivedPartyInvitesList>(OnUpdateClientPartyInvitesMessage);
    }

    private void OnUpdateClientPartyInviteMessage(MsgHandleReceivedPartyInviteState message)
    {
        HandleReceivedInviteState(message.State);
    }

    private void OnUpdateClientPartyInvitesMessage(MsgUpdateReceivedPartyInvitesList message)
    {
        foreach (var state in message.States)
            HandleReceivedInviteState(state);
    }

    public bool InviteExist(uint id)
    {
        return TryGetInvite(id, out _);
    }

    public bool TryGetInvite(uint id, [NotNullWhen(true)] out PartyInvite? invite)
    {
        return _receiveInvites.TryGetValue(id, out invite);
    }

    public PartyInvite? GetInvite(uint id)
    {
        TryGetInvite(id, out var invite);
        return invite;
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

    public bool IsValidReceivedInviteStatus(PartyInviteStatus status)
    {
        return _validReceivedInviteStatuses.Contains(status);
    }

    private bool AddInvite(PartyInviteState state)
    {
        var invite = new PartyInvite(state);
        return AddInvite(invite);
    }

    private bool AddInvite(PartyInvite invite)
    {
        if (!IsValidReceivedInviteStatus(invite.Status))
            return false;

        if (InviteExist(invite.Id))
            return false;

        _receiveInvites.Add(invite.Id, invite);
        ReceivedInviteAdded?.Invoke(invite);

        return true;
    }

    private bool RemoveInvite(uint id)
    {
        if (_receiveInvites.Remove(id, out var invite))
        {
            ReceivedInviteRemoved?.Invoke(invite);
            return true;
        }
        else
            return false;
    }

    private bool RemoveInvite(PartyInvite invite)
    {
        return RemoveInvite(invite.Id);
    }

    private void HandleReceivedInviteState(PartyInviteState state)
    {
        if (TryGetInvite(state.Id, out var exist))
        {
            if (!IsValidReceivedInviteStatus(state.Status))
            {
                RemoveInvite(exist);
                return;
            }

            exist.HandleState(state);
            ReceivedInviteUpdated?.Invoke(exist);
        }
        else
        {
            if (!IsValidReceivedInviteStatus(state.Status))
                return;

            AddInvite(state);
        }
    }
}
