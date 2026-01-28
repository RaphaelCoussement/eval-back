namespace DungeonCrawler_Quests_Service.Domain.Entities;

public class QuestDefinition
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "DUNGEON_COMPLETION";
    public int TargetCount { get; set; } // Ex: 3
    public bool IsActive { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string Reward { get; set; } = string.Empty;
}