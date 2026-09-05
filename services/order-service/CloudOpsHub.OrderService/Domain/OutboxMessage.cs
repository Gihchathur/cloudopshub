namespace CloudOpsHub.OrderService.Domain;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Payload { get; private set; } = string.Empty;

    public DateTime? ProcessedOnUtc { get; private set; }

    public DateTime? NextAttemptAtUtc { get; private set; }

    public int Attempts { get; private set; }

    public string? Error { get; private set; }

    public DateTime? DeadLetteredOnUtc { get; private set; }
    

    private OutboxMessage()
    {
    }

    public OutboxMessage(
        string type,
        string payload)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type is required.", nameof(type));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload is required.", nameof(payload));

        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
        Type = type;
        Payload = payload;
        Attempts = 0;
    }

    public void MarkProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
        NextAttemptAtUtc = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        Error = error;

        var delaySeconds = Math.Min(
            Math.Pow(2, Attempts),
            300);

        NextAttemptAtUtc =
            DateTime.UtcNow.AddSeconds(delaySeconds);
    }

    public void MarkDeadLettered()
    {
        DeadLetteredOnUtc = DateTime.UtcNow;
    }

    public void Replay()
    {
        ProcessedOnUtc = null;
        DeadLetteredOnUtc = null;
        NextAttemptAtUtc = DateTime.UtcNow;
        Error = null;
    }
}