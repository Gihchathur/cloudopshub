using System.Text.Json;
using CloudOpsHub.OrderService.Application.Events;
using CloudOpsHub.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CloudOpsHub.OrderService.Infrastructure.Messaging;

public sealed class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherWorker> _logger;
    private readonly OutboxOptions _options;

    public OutboxPublisherWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherWorker> logger,
        IOptions<OutboxOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing outbox messages.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task ProcessMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        var publisher = scope.ServiceProvider
            .GetRequiredService<IRabbitMqPublisher>();

        var now = DateTime.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(x =>
                x.ProcessedOnUtc == null &&
                x.DeadLetteredOnUtc == null &&
                x.Attempts < _options.MaxAttempts &&
                (x.NextAttemptAtUtc == null ||
                x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                if (message.Type == nameof(OrderCreatedEvent))
                {
                    var eventData =
                        JsonSerializer.Deserialize<OrderCreatedEvent>(
                            message.Payload);

                    if (eventData is null)
                    {
                        throw new InvalidOperationException(
                            $"Unable to deserialize outbox message {message.Id}.");
                    }

                    await publisher.PublishAsync(
                        exchange: "cloudopshub.events",
                        routingKey: "order.created",
                        message: eventData,
                        cancellationToken);

                    message.MarkProcessed();

                    _logger.LogInformation(
                        "Published outbox message {MessageId}.",
                        message.Id);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unknown outbox message type: {message.Type}");
                }
            }
            catch (Exception ex)
{
                message.MarkFailed(ex.Message);

                if (message.Attempts >= _options.MaxAttempts)
                {
                    message.MarkDeadLettered();

                    _logger.LogCritical(
                        "Outbox message {MessageId} has been " +
                        "dead-lettered after {Attempts} attempts. " +
                        "Error: {Error}",
                        message.Id,
                        message.Attempts,
                        ex.Message);
                }
                else
                {
                    _logger.LogError(
                        ex,
                        "Failed to publish outbox message {MessageId}. " +
                        "Attempt {Attempt}/{MaxAttempts}. " +
                        "Next attempt at {NextAttemptAtUtc}.",
                        message.Id,
                        message.Attempts,
                        _options.MaxAttempts,
                        message.NextAttemptAtUtc);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}