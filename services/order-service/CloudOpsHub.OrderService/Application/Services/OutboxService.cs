using CloudOpsHub.OrderService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudOpsHub.OrderService.Application.Services;

public sealed class OutboxService
{
    private readonly OrderDbContext _dbContext;

    public OutboxService(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ReplayAsync(
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var message = await _dbContext.OutboxMessages
            .FirstOrDefaultAsync(
                x => x.Id == messageId,
                cancellationToken);

        if (message is null)
            return false;

        if (message.DeadLetteredOnUtc is null)
            throw new InvalidOperationException(
                "Only dead-lettered messages can be replayed.");

        message.Replay();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}