using DungeonCrawlerAssembly.Messages;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace DungeonCrawler_Quests_Service.Controllers;

[ApiController]
[Route("api/simulator")]
public class SimulatorController : ControllerBase
{
    private readonly IBus _bus;

    public SimulatorController(IBus bus)
    {
        _bus = bus;
    }

    /// <summary>
    /// Simule l'envoi d'un événement DungeonCompleted par le service Game
    /// </summary>
    [HttpPost("dungeon-completed")]
    public async Task<IActionResult> SimulateDungeonCompleted([FromBody] SimulateDungeonRequest request)
    {
        var message = new DungeonCompleted
        {
            EventId = request.EventId ?? Guid.NewGuid(), // Permet de forcer un ID pour tester l'idempotence
            PlayerId = request.PlayerId,
            DungeonId = Guid.NewGuid(),
            CompletedAt = DateTime.UtcNow
        };

        // Publication sur le bus (RabbitMQ)
        await _bus.Publish(message);

        return Ok(new { 
            Message = "Événement publié sur le bus", 
            EventId = message.EventId,
            PlayerId = message.PlayerId 
        });
    }
}

public record SimulateDungeonRequest(Guid PlayerId, Guid? EventId = null);