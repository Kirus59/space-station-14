
using Content.Shared.SS220.CCVars;
using Lidgren.Network;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using System.Threading.Tasks;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract partial class SharedPartyManager : ISharedPartyManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IDynamicTypeFactory _dynamicTypeFactory = default!;

    protected ISawmill Sawmill = default!;

    private readonly Dictionary<Type, NetSub> _netSubs = [];
    private event Action<PartyResponceMessage>? OnResponceMsg;

    public virtual void Initialize()
    {
        IoCManager.InjectDependencies(this);

        Sawmill = Logger.GetSawmill("PartyManager");

        _net.RegisterNetMessage<PartyNetMessage>(OnReceiveNetMessage);
    }

    private void OnReceiveNetMessage(PartyNetMessage msg)
    {
        if (msg.Message is not { } message)
            return;

        InvokeSubscription(message);

        switch (message)
        {
            case PartyResponceMessage responce:
                OnResponceMsg?.Invoke(responce);
                break;
        }

        void InvokeSubscription(PartyMessage message)
        {
            var type = message.GetType();
            if (!_netSubs.TryGetValue(type, out var subscribe))
                return;

            subscribe.Invoke(message);
        }
    }

    protected void SubscribeNetMessage<T>(Action<T> callback) where T : PartyMessage
    {
        var type = typeof(T);
        if (_netSubs.ContainsKey(type))
            throw new Exception($"already exist a subscription for an element with type: {type}");

        _netSubs.Add(type, new NetSubscription<T>(callback));
    }

    protected void SubscribeNetMessage<T>(Action<T, ICommonSession> callback) where T : PartyMessage
    {
        var type = typeof(T);
        if (_netSubs.ContainsKey(type))
            throw new Exception($"already exist a subscription for an element with type: {type}");

        _netSubs.Add(type, new NetSubscription<T, ICommonSession>(callback));
    }

    protected async Task<T> WaitResponce<T>(TimeSpan? timeout = null) where T : PartyResponceMessage, new()
    {
        var tcs = new TaskCompletionSource<T>();

        void OnResponce(PartyResponceMessage responce)
        {
            if (responce is T result)
                tcs.SetResult(result);
        }

        OnResponceMsg += OnResponce;

        timeout ??= TimeSpan.FromSeconds(4);
        var delayTask = Task.Delay(timeout.Value);

        var completedTask = await Task.WhenAny(tcs.Task, delayTask);

        T result;
        if (completedTask == tcs.Task)
            result = await tcs.Task;
        else
        {
            result = _dynamicTypeFactory.CreateInstance<T>();
            result.Timeout = true;
        }

        OnResponceMsg -= OnResponce;
        return result;
    }

    private abstract class NetSub
    {
        public abstract void Invoke(PartyMessage message);
    }

    private sealed class NetSubscription<T>(Action<T> callback) : NetSub where T : PartyMessage
    {
        public Action<T> Callback = callback;

        public override void Invoke(PartyMessage message)
        {
            Callback.Invoke((T)message);
        }
    }

    private sealed class NetSubscription<TMsg, TSession>(Action<TMsg, TSession> callback) : NetSub
        where TMsg : PartyMessage
        where TSession : ICommonSession
    {
        public Action<TMsg, TSession> Callback = callback;

        public override void Invoke(PartyMessage message)
        {
            var playerMng = IoCManager.Resolve<ISharedPlayerManager>();
            if (!playerMng.TryGetSessionById(message.Sender, out var session))
                return;

            Callback.Invoke((TMsg)message, (TSession)session);
        }
    }
}

[Serializable, NetSerializable]
public abstract class SharedPartyData
{
    public readonly uint Id;

    public bool Disbanded = false;

    public SharedPartyData(uint id)
    {
        Id = id;
    }
}

[Serializable, NetSerializable]
public record struct ClientPartyDataState(uint Id,
    PartyUserInfo LocalUserInfo,
    List<PartyUserInfo> Members,
    PartySettingsState SettingsState,
    bool Disbanded);

[Serializable, NetSerializable]
[Access(typeof(SharedPartyManager), Other = AccessPermissions.Read)]
public sealed class PartyUserInfo
{
    public readonly uint Id;
    public PartyRole Role;
    public string Name;
    public bool Connected;

    public PartyUserInfo(uint id, PartyRole role, string name, bool connected)
    {
        Id = id;
        Role = role;
        Name = name;
        Connected = connected;
    }
}

[Serializable, NetSerializable]
public abstract class SharedPartyInvite
{
    public readonly uint Id;

    public InviteStatus Status;

    public SharedPartyInvite(uint id, InviteStatus status = InviteStatus.None)
    {
        Id = id;
        Status = status;
    }

    public bool Equals(SharedPartyInvite invite)
    {
        return Id == invite.Id;
    }

    public static bool Equals(SharedPartyInvite? invite1, SharedPartyInvite? invite2)
    {
        if (invite1 is null) return invite2 is null;
        if (invite2 is null) return invite1 is null;

        return invite1.Equals(invite2);
    }

    public static bool operator ==(SharedPartyInvite? left, SharedPartyInvite? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SharedPartyInvite? left, SharedPartyInvite? right)
    {
        return !Equals(left, right);
    }
}

public enum PartyRole
{
    None,
    Member,
    Leader
}

public enum InviteStatus
{
    None,
    Deleted,
    Sended,

    Accepted,
    Denied
}

[Serializable, NetSerializable]
public record struct SendedInviteState(uint Id, string TargetName, InviteStatus Status);

[Serializable, NetSerializable]
public record struct IncomingInviteState(uint Id, string SenderName, InviteStatus Status);

[Serializable, NetSerializable]
public record struct PartySettingsState
{
    public int MaxMembers;

    public PartySettingsState()
    {
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        MaxMembers = cfg.GetCVar(CCVars220.PartyMembersLimit);
    }
}
