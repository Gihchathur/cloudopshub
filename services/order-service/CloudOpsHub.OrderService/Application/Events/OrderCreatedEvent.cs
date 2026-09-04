namespace CloudOpsHub.OrderService.Application.Events;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    string Currency,
    DateTime CreatedAt);