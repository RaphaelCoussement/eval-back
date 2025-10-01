using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DungeonCrawler_Game_Service.Domain.Entities;


public class Dungeon
{
    [BsonId] // MongoDB Id
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public List<Level> Levels { get; set; } = new();
}