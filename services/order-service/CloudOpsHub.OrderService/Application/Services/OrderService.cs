using CloudOpsHub.OrderService.Application.DTOs;
using CloudOpsHub.OrderService.Domain.Entities;
using CloudOpsHub.OrderService.Application.Events;
using CloudOpsHub.OrderService.Infrastructure.Messaging;
using CloudOpsHub.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudOpsHub.OrderService.Application.Services;

public class OrderService
{
    private readonly OrderDbContext _dbContext;

    private readonly IRabbitMqPublisher _publisher;

    public OrderService(OrderDbContext dbContext, IRabbitMqPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<OrderResponse> CreateAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = new Order(
            request.UserId,
            request.TotalAmount,
            request.Currency);

        _dbContext.Orders.Add(order);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.Currency,
            order.CreatedAt);

        await _publisher.PublishAsync(
            exchange: "cloudopshub.events",
            routingKey: "order.created",
            message: orderCreatedEvent,
            cancellationToken);

        return Map(order);
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