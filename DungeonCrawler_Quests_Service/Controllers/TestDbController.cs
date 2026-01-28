using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Quests_Service.Controllers;

[ApiController]
[Route("api/test-db")]
public class TestDbController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TestDbController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("ping-mongo")]
    public async Task<IActionResult> Ping()
    {
        var testQuest = new QuestDefinition
        {
            Id = Guid.NewGuid(),
            Code = "TEST_CONNECTION",
            Title = "Tester la DB",
            IsActive = true
        };

        // Utilise ton GenericRepository via UnitOfWork
        var repo = _unitOfWork.GetRepository<QuestDefinition>();
        await repo.AddAsync(testQuest);
        
        return Ok(new { Message = "Connexion réussie, quête de test créée !", QuestId = testQuest.Id });
    }
}