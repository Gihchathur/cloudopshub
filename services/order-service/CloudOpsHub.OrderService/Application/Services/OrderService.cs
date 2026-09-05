using CloudOpsHub.OrderService.Application.DTOs;
using CloudOpsHub.OrderService.Domain.Entities;
using CloudOpsHub.OrderService.Application.Events;
using CloudOpsHub.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CloudOpsHub.OrderService.Domain;

namespace CloudOpsHub.OrderService.Application.Services;

public class OrderService
{
    private readonly OrderDbContext _dbContext;

    public OrderService(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var order = new Order(
            request.UserId,
            request.TotalAmount,
            request.Currency);

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.Currency,
            order.CreatedAt);

        var outboxMessage = new OutboxMessage(
            type: nameof(OrderCreatedEvent),
            payload: JsonSerializer.Serialize(orderCreatedEvent));

        _dbContext.Orders.Add(order);
        _dbContext.OutboxMessages.Add(outboxMessage);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OrderResponse(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.Currency,
            order.Status,
            order.CreatedAt);
    }

    public async Task<OrderResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        return order is null ? null : Map(order);
    }

    private static OrderResponse Map(Order order)
    {
        return new OrderResponse(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.Currency,
            order.Status,
            order.CreatedAt);
    }
}