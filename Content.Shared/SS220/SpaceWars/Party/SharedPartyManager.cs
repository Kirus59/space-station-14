
using Content.Shared.SS220.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Threading.Tasks;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract partial class SharedPartyManager : ISharedPartyManager
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IDynamicTypeFactory _dynamicTypeFactory = default!;

    protected ISawmill Sawmill = default!;

    private readonly Dictionary<Type, INetSubscription> _netSubs = [];
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

        TryInvokeSubscription(message);

        switch (message)
        {
            case PartyResponceMessage responce:
                OnResponceMsg?.Invoke(responce);
                break;
        }

        bool TryInvokeSubscription(PartyMessage message)
        {
            var type = message.GetType();
            if (!_netSubs.TryGetValue(type, out var subscribe))
                return false;

            subscribe.Invoke(message);
            return true;
        }
    }

    protected void SubscribeNetMessage<T>(Action<T> callback) where T : PartyMessage
    {
        var type = typeof(T);
        if (_netSubs.ContainsKey(type))
            throw new Exception($"Already exist a subscription for an element with type: {type}");

        _netSubs.Add(type, new NetSubscription<T>(callback));
    }

    protected void SubscribeNetMessage<T>(Action<T, ICommonSession> callback) where T : PartyMessage
    {
        var type = typeof(T);
        if (_netSubs.ContainsKey(type))
            throw new Exception($"Already exist a subscription for an element with type: {type}");

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

    private interface INetSubscription
    {
        void Invoke(PartyMessage message);
    }

    private sealed class NetSubscription<T>(Action<T> callback) : INetSubscription where T : PartyMessage
    {
        public Action<T> Callback = callback;

        public void Invoke(PartyMessage message)
        {
            Callback.Invoke((T)message);
        }
    }

    private sealed class NetSubscription<TMsg, TSession>(Action<TMsg, TSession> callback) : INetSubscription
        where TMsg : PartyMessage
        where TSession : ICommonSession
    {
        public Action<TMsg, TSession> Callback = callback;

        public void Invoke(PartyMessage message)
        {
            if (message.Sender is null)
            {
                DebugTools.Assert($"Failed to invoke a callback for \"{message.GetType()}\" by reason: " +
                    $"The field \"{nameof(PartyMessage.Sender)}\" of \"{message.GetType()}\" must not be null.");
                return;
            }

            var playerMng = IoCManager.Resolve<ISharedPlayerManager>();
            if (!playerMng.TryGetSessionById(message.Sender, out var session))
            {
                DebugTools.Assert($"Failed to invoke a callback for \"{message.GetType()}\" by reason: " +
                    $"Doesn't found a \"{nameof(ICommonSession)}\" for \"{nameof(NetUserId)}\" with id: \"{message.Sender.Value.UserId}\"");
                return;
            }

            Callback.Invoke((TMsg)message, (TSession)session);
        }
    }
}

[Serializable, NetSerializable]
public abstract class SharedPartyData(uint id)
{
    public readonly uint Id = id;

    public bool Disbanded = false;
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
public abstract class SharedPartyInvite(uint id, InviteStatus status = InviteStatus.None)
{
    public readonly uint Id = id;

    public InviteStatus Status = status;

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool Equals(SharedPartyInvite? invite1, SharedPartyInvite? invite2)
    {
        if (ReferenceEquals(invite1, invite2))
            return true;

        if (invite1 is null)
            return false;

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
