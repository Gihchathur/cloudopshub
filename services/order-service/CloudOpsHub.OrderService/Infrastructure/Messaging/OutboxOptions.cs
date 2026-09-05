namespace CloudOpsHub.OrderService.Infrastructure.Messaging;

public sealed class OutboxOptions
{
    public int PollingIntervalSeconds { get; set; } = 5;

    public int MaxAttempts { get; set; } = 10;

    public int BatchSize { get; set; } = 20;
}