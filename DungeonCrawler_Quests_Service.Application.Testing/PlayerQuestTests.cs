using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Domain.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace DungeonCrawler_Quests_Service.Application.Testing;

[TestFixture]
public class PlayerQuestTests
{
    [Test]
    public void UpdateProgress_ShouldSetStatusToCompleted_WhenTargetReached()
    {
        // Arrange : Joueur à 2/3 sur sa quête
        var playerQuest = new PlayerQuest { ProgressCount = 2, Status = QuestStatus.InProgress };
        int targetCount = 3;

        // Act : On termine le 3ème donjon
        playerQuest.UpdateProgress(1, targetCount);

        // Assert : Vérification de la transition d'état
        playerQuest.Status.Should().Be(QuestStatus.Completed);
        playerQuest.ProgressCount.Should().Be(3);
        playerQuest.CompletedAt.Should().NotBeNull();
    }

    [Test]
    public void UpdateProgress_ShouldStayCompleted_WhenExtraProgressAdded()
    {
        // Arrange : Quête déjà finie
        var playerQuest = new PlayerQuest { ProgressCount = 3, Status = QuestStatus.Completed };

        // Act
        playerQuest.UpdateProgress(1, 3);

        // Assert : Le compteur ne doit pas bouger et le statut rester Completed
        playerQuest.ProgressCount.Should().Be(3);
        playerQuest.Status.Should().Be(QuestStatus.Completed);
    }
}