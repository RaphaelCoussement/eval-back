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
    public ActionResult<DungeonResponse> GenerateDungeon([FromBody] GenerateDungeonRequest request)
    {
        Dungeon dungeon = _dungeonService.GenerateDungeon(request.MinRooms, request.MaxRooms);

        var response = new DungeonResponse
        {
            Id = dungeon.Id,
            Levels = dungeon.Levels.Select(level => new LevelResponse
            {
                Number = level.Number,
                Rooms = level.Rooms.Select(room => new RoomResponse
                {
                    Id = room.Id,
                    NextRoomId = room.NextRoomId
                }).ToList()
            }).ToList()
        };

        return Ok(response);
    }
    
    [HttpGet("{dungeonId}/room/{roomId}/next")]
    public ActionResult<NextRoomsResponse> GetNextRooms(string dungeonId, string roomId)
    {
        var nextRooms = _dungeonService.GetNextRooms(dungeonId, roomId);

        return Ok(new NextRoomsResponse
        {
            CurrentRoomId = roomId,
            NextRooms = nextRooms.Select(r => new RoomResponse
            {
                Id = r.Id,
                NextRoomId = r.NextRoomId
            }).ToList()
        });
    }

    [HttpPost("{dungeonId}/enter")]
    public ActionResult<EnterRoomResponse> EnterRoom(string dungeonId, [FromBody] EnterRoomRequest request)
    {
        var response = _dungeonService.EnterRoom(dungeonId, request.CurrentRoomId, request.NextRoomId);
        return Ok(response);
    }
}