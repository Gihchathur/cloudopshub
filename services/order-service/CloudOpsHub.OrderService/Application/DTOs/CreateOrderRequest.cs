namespace CloudOpsHub.OrderService.Application.DTOs;

public record CreateOrderRequest(
    Guid UserId,
    decimal TotalAmount,
    string Currency);