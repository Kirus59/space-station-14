
using Content.Shared.SS220.CCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Serialization;

namespace Content.Shared.SS220.SpaceWars.Party;

public abstract partial class SharedPartyManager : ISharedPartyManager
{
    protected ISawmill _sawmill = default!;

    public virtual void Initialize()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = Logger.GetSawmill("PartyManager");
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
    Error,
    Deleted,
    TargetNotFound,
    TargetIsSender,
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
    public uint MaxMembers;

    public PartySettingsState()
    {
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        MaxMembers = cfg.GetCVar(CCVars220.PartyMembersLimit);
    }
}
