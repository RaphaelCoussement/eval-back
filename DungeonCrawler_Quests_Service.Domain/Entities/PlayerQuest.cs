using DungeonCrawler_Quests_Service.Domain.Enums;

namespace DungeonCrawler_Quests_Service.Domain.Entities;

public class PlayerQuest
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid QuestId { get; set; }
    public QuestDefinition? Quest { get; set; }

    public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
    public int ProgressCount { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Logique métier simple pour mettre à jour la progression
    public void UpdateProgress(int amount, int targetCount)
    {
        if (Status == QuestStatus.Completed || Status == QuestStatus.Claimed) return;

        ProgressCount += amount;
        UpdatedAt = DateTime.UtcNow;

        if (ProgressCount >= targetCount)
        {
            Status = QuestStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }
        else
        {
            Status = QuestStatus.InProgress;
        }
    }
}