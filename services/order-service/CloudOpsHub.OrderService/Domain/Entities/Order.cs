namespace CloudOpsHub.OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Order(
        Guid userId,
        decimal totalAmount,
        string currency)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.");

        if (totalAmount <= 0)
            throw new ArgumentException(
                "Total amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException(
                "Currency is required.");

        Id = Guid.NewGuid();
        UserId = userId;
        TotalAmount = totalAmount;
        Currency = currency.ToUpperInvariant();
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                "Only pending orders can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException(
                "Order is already cancelled.");

        Status = OrderStatus.Cancelled;
    }
}