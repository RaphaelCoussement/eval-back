using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DungeonCrawler_Game_Service.Domain.Entities;


public class Dungeon
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Seed { get; set; } = string.Empty;

    public List<Level> Levels { get; set; } = new();

    public List<RoomLink> Links { get; set; } = new();
}