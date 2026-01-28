using DungeonCrawler_Quests_Service.Application.Features.Quests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Quests_Service.Controllers;

[ApiController]
[Route("api/quests")]
public class QuestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuestsController(IMediator mediator) => _mediator = mediator;

    // GET: api/quests
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActiveOnly)
    {
        var result = await _mediator.Send(new GetAllQuestsQuery(isActiveOnly));
        return Ok(result);
    }

    // GET: api/quests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetQuestByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST: api/quests
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestCommand command)
    {
        var id = await _mediator.Send(command);
        // On pointe maintenant vers GetById pour respecter les standards REST
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    // PUT: api/quests/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestCommand command)
    {
        // On s'assure que l'ID de l'URL correspond à l'ID de la commande
        if (id != command.Id) 
        {
            return BadRequest("L'ID dans l'URL ne correspond pas à l'ID du corps de la requête.");
        }

        await _mediator.Send(command);
        return NoContent(); // 204: Succès mais pas de contenu à renvoyer
    }

    // DELETE: api/quests/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteQuestCommand(id));
        return NoContent();
    }
}