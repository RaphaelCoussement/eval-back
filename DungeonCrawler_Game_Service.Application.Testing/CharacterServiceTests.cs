using System.Threading.Tasks;
using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Contracts;
using DungeonCrawler_Game_Service.Application.Services;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using Moq;
using NUnit.Framework;

namespace DungeonCrawler_Game_Service.Application.Testing;

[TestFixture]
public class CharacterServiceTests
{
    /// <summary>
    /// Ce test vérifie que la méthode CreateCharacterAsync du CharacterService crée un personnage avec les propriétés correctes
    /// </summary>
    [Test]
    public async Task CreateCharacterAsync_ShouldCreateCharacterAndCallRepository()
    {
        // Arrangement des données
        var mockRepo = new Mock<IRepository<Character>>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Character>())).Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.GetRepository<Character>()).Returns(mockRepo.Object);

        var service = new CharacterService(mockUnitOfWork.Object);
        var name = "TestHero";
        var characterClass = Classes.Warrior;
        var userId = "user-123";

        // Action testée
        var result = await service.CreateCharacterAsync(name, characterClass, userId);

        // Assertions
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(name));
        Assert.That(result.Class, Is.EqualTo(characterClass));
        Assert.That(result.UserId, Is.EqualTo(userId));
        mockRepo.Verify(r => r.AddAsync(It.Is<Character>(c => c.Name == name && c.Class == characterClass && c.UserId == userId)),
            Times.Once);
    }
}
