using CloudOpsHub.OrderService.Domain.Entities;
using CloudOpsHub.OrderService.Domain;
using Microsoft.EntityFrameworkCore;

namespace CloudOpsHub.OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(x => x.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Payload)
                .IsRequired();

            entity.Property(x => x.OccurredOnUtc)
                .IsRequired();

            entity.Property(x => x.ProcessedOnUtc)
                .IsRequired(false);

            entity.Property(x => x.Attempts)
                .IsRequired();

            entity.Property(x => x.Error)
                .IsRequired(false);

            entity.Property(x => x.NextAttemptAtUtc)
                .IsRequired(false);

            entity.Property(x => x.DeadLetteredOnUtc)
                .IsRequired(false);

            entity.HasIndex(x => new
            {
                x.ProcessedOnUtc,
                x.DeadLetteredOnUtc,
                x.NextAttemptAtUtc
            });
        });
    }
}