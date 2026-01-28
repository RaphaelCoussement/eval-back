using DungeonCrawler_Quests_Service.Application.Features.PlayerQuests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Quests_Service.Controllers;

[ApiController]
[Route("api/players")]
public class PlayerQuestsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PlayerQuestsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{playerId}/quests")]
    public async Task<IActionResult> GetPlayerQuests(Guid playerId) 
        => Ok(await _mediator.Send(new GetPlayerQuestsQuery(playerId)));

    [HttpPost("{playerId}/quests/{questId}/reset")]
    public async Task<IActionResult> Reset(Guid playerId, Guid questId) 
        => Ok(await _mediator.Send(new ResetPlayerQuestCommand(playerId, questId)));
}