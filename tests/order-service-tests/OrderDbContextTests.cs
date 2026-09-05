using CloudOpsHub.OrderService.Application.Events;
using CloudOpsHub.OrderService.Domain;
using CloudOpsHub.OrderService.Domain.Entities;
using CloudOpsHub.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CloudOpsHub.OrderService.Tests;

public class OrderDbContextTests
{
    [Fact]
    public async Task OrderAndOutboxMessage_ShouldBeSavedTogether()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new OrderDbContext(options);

        var userId = Guid.NewGuid();

        var order = new Order(
            userId,
            149.99m,
            "SEK");

        var orderCreatedEvent = new OrderCreatedEvent(
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.Currency,
            order.CreatedAt);

        var outboxMessage = new OutboxMessage(
            nameof(OrderCreatedEvent),
            JsonSerializer.Serialize(orderCreatedEvent));

        dbContext.Orders.Add(order);
        dbContext.OutboxMessages.Add(outboxMessage);

        await dbContext.SaveChangesAsync();

        var savedOrder = await dbContext.Orders
            .SingleAsync(x => x.Id == order.Id);

        var savedMessage = await dbContext.OutboxMessages
            .SingleAsync(x => x.Id == outboxMessage.Id);

        Assert.Equal(order.Id, savedOrder.Id);
        Assert.Equal(order.TotalAmount, savedOrder.TotalAmount);

        Assert.Equal(
            nameof(OrderCreatedEvent),
            savedMessage.Type);

        Assert.Contains(
            order.Id.ToString(),
            savedMessage.Payload);

        Assert.Null(savedMessage.ProcessedOnUtc);
    }
}