using CloudOpsHub.OrderService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudOpsHub.OrderService.Api;

[ApiController]
[Route("api/outbox")]
public sealed class OutboxController : ControllerBase
{
    private readonly OutboxService _outboxService;

    public OutboxController(OutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    [HttpPost("{id:guid}/replay")]
    public async Task<IActionResult> Replay(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var replayed = await _outboxService.ReplayAsync(
                id,
                cancellationToken);

            if (!replayed)
                return NotFound();

            return Ok(new
            {
                messageId = id,
                status = "queued_for_replay"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
    }
}