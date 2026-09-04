using CloudOpsHub.OrderService.Domain.Entities;

namespace CloudOpsHub.OrderService.Application.DTOs;

public record OrderResponse(
    Guid Id,
    Guid UserId,
    decimal TotalAmount,
    string Currency,
    OrderStatus Status,
    DateTime CreatedAt);