using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Models.Requests;
using DungeonCrawler_Game_Service.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DungeonCrawler_Game_Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DungeonController : ControllerBase
{
    private readonly IDungeonService _dungeonService;

    public DungeonController(IDungeonService dungeonService)
    {
        _dungeonService = dungeonService;
    }

    [HttpPost("generate")]
    [ProducesResponseType(typeof(DungeonResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<DungeonResponse>> GenerateDungeon()
    {
        try
        {
            var dungeon = await _dungeonService.GenerateDungeonAsync();
            var response = new DungeonResponse
            {
                Id = dungeon.Id,
                Levels = dungeon.Levels.Select(level => new LevelResponse
                {
                    Number = level.Number,
                    Rooms = level.Rooms.Select(room => new RoomResponse
                    {
                        Id = room.Id,
                        NextRoomId = room.NextRoomId,
                        Type = room.Type.ToString()
                    }).ToList()
                }).ToList()
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse
            {
                Code = "DUNGEON_GENERATION_FAILED",
                Message = ex.Message
            });
        }
    }

    [HttpGet("{dungeonId}/room/{roomId}/next")]
    [ProducesResponseType(typeof(NextRoomsResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public ActionResult<NextRoomsResponse> GetNextRooms(string dungeonId, string roomId)
    {
        var nextRooms = _dungeonService.GetNextRooms(dungeonId, roomId);
        if (nextRooms == null || !nextRooms.Any())
        {
            return NotFound(new ErrorResponse
            {
                Code = "ROOM_NOT_FOUND",
                Message = $"No next rooms found for room {roomId}."
            });
        }

        return Ok(new NextRoomsResponse
        {
            CurrentRoomId = roomId,
            NextRooms = nextRooms.Select(r => new RoomResponse
            {
                Id = r.Id,
                NextRoomId = r.NextRoomId,
                Type = r.Type.ToString()
            }).ToList()
        });
    }

    [HttpPost("{dungeonId}/enter")]
    [ProducesResponseType(typeof(EnterRoomResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public ActionResult<EnterRoomResponse> EnterRoom(string dungeonId, [FromBody] EnterRoomRequest request)
    {
        try
        {
            var response = _dungeonService.EnterRoom(dungeonId, request.CurrentRoomId, request.NextRoomId);

            if (response == null)
            {
                return NotFound(new ErrorResponse
                {
                    Code = "ROOM_NOT_FOUND",
                    Message = "The specified room does not exist."
                });
            }

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "INVALID_REQUEST",
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse
            {
                Code = "ENTER_ROOM_FAILED",
                Message = ex.Message
            });
        }
    }
}