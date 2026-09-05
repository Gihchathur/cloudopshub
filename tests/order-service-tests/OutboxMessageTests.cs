using CloudOpsHub.OrderService.Domain;

namespace CloudOpsHub.OrderService.Tests;

public class OutboxMessageTests
{
    [Fact]
    public void NewMessage_ShouldStartUnprocessed()
    {
        var message = new OutboxMessage(
            "OrderCreatedEvent",
            """{"orderId":"123"}""");

        Assert.Null(message.ProcessedOnUtc);
        Assert.Null(message.DeadLetteredOnUtc);
        Assert.Null(message.NextAttemptAtUtc);
        Assert.Equal(0, message.Attempts);
    }

    [Fact]
    public void MarkFailed_ShouldIncrementAttemptsAndScheduleRetry()
    {
        var message = new OutboxMessage(
            "OrderCreatedEvent",
            """{"orderId":"123"}""");

        message.MarkFailed("RabbitMQ unavailable");

        Assert.Equal(1, message.Attempts);
        Assert.Equal(
            "RabbitMQ unavailable",
            message.Error);

        Assert.NotNull(message.NextAttemptAtUtc);
        Assert.Null(message.ProcessedOnUtc);
        Assert.Null(message.DeadLetteredOnUtc);
    }

    [Fact]
    public void MarkProcessed_ShouldSetProcessedTimestamp()
    {
        var message = new OutboxMessage(
            "OrderCreatedEvent",
            """{"orderId":"123"}""");

        message.MarkProcessed();

        Assert.NotNull(message.ProcessedOnUtc);
        Assert.Null(message.NextAttemptAtUtc);
    }

    [Fact]
    public void MarkDeadLettered_ShouldSetDeadLetterTimestamp()
    {
        var message = new OutboxMessage(
            "OrderCreatedEvent",
            """{"orderId":"123"}""");

        message.MarkFailed("RabbitMQ unavailable");
        message.MarkDeadLettered();

        Assert.NotNull(message.DeadLetteredOnUtc);
    }

    [Fact]
    public void Replay_ShouldReturnMessageToPendingState()
    {
        var message = new OutboxMessage(
            "OrderCreatedEvent",
            """{"orderId":"123"}""");

        message.MarkFailed("RabbitMQ unavailable");
        message.MarkDeadLettered();

        message.Replay();

        Assert.Null(message.ProcessedOnUtc);
        Assert.Null(message.DeadLetteredOnUtc);
        Assert.Null(message.Error);
        Assert.NotNull(message.NextAttemptAtUtc);

        // Attempts are deliberately retained for operational history.
        Assert.Equal(1, message.Attempts);
    }
}