using System.Text;
using System.Text.Json;
using CloudOpsHub.NotificationService.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CloudOpsHub.NotificationService.Messaging;

public sealed class OrderCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public OrderCreatedConsumer(
        IConfiguration configuration,
        ILogger<OrderCreatedConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var options = _configuration
            .GetSection("RabbitMq")
            .Get<RabbitMqOptions>()
            ?? new RabbitMqOptions();

        var factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };

        _connection = await factory.CreateConnectionAsync(
            stoppingToken);

        _channel = await _connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: "cloudopshub.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "notification-service.order-created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: "notification-service.order-created",
            exchange: "cloudopshub.events",
            routingKey: "order.created",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(
                    eventArgs.Body.ToArray());

                var orderCreated =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(json);

                if (orderCreated is null)
                {
                    _logger.LogWarning(
                        "Received invalid OrderCreated event.");

                    await _channel.BasicNackAsync(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false);

                    return;
                }

                _logger.LogInformation(
                    "Received OrderCreated event. " +
                    "OrderId: {OrderId}, UserId: {UserId}, " +
                    "Amount: {Amount} {Currency}",
                    orderCreated.OrderId,
                    orderCreated.UserId,
                    orderCreated.TotalAmount,
                    orderCreated.Currency);

                _logger.LogInformation(
                    "Notification would be sent to user {UserId} " +
                    "for order {OrderId}.",
                    orderCreated.UserId,
                    orderCreated.OrderId);

                await _channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing OrderCreated event.");

                await _channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "notification-service.order-created",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "Notification Service is consuming order.created events.");

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}