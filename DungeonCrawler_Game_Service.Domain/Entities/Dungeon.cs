using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DungeonCrawler_Game_Service.Domain.Entities;


public class Dungeon
{
    [BsonId] // MongoDB Id
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Seed { get; set; }
    public List<Level> Levels { get; set; } = new();
}