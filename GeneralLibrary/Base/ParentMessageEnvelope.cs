using JetBrains.Annotations;

namespace GeneralLibrary.Base;

public sealed class ParentMessageEnvelope
{
    public int HopLevel2 { get; set; }
    
    public bool IsReQueued { get; set; }
    public int ReQueueCount { get; set; }

    public Guid MessageId { get; set; }

    [CanBeNull]
    public string CorrelationId { get; set; }

    [CanBeNull]
    public string UserId { get; set; }

    [CanBeNull]
    public string UserRoleUniqueName { get; set; }

    [CanBeNull]
    public string Channel { get; set; }

    [CanBeNull]
    public string Producer { get; set; }
}