using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Moq;

namespace DungeonCrawler_Game_Service.Application.Testing;

[TestFixture]
public class CharacterUnitTests
{
    [Test]
    public async Task CreateCharacterAsync_ShouldCreateCharacterAndCallRepository()
    {
        // --- 1. Arrange Dependencies ---
        
        // Mock UnitOfWork and Repository (Existing code)
        var mockRepo = new Mock<IRepository<Character>>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Character>())).Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.GetRepository<Character>()).Returns(mockRepo.Object);

        // Mock Bus (New requirement)
        var mockBus = new Mock<IBus>();
        mockBus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<IDictionary<string, string>>()))
               .Returns(Task.CompletedTask);

        // Mock Logger (New requirement)
        var mockLogger = new Mock<ILogger<CreateCharacterCommandHandler>>();

        // --- 2. Instantiate Handler with ALL dependencies ---
        
        var handler = new CreateCharacterCommandHandler(
            mockUnitOfWork.Object, 
            mockBus.Object, 
            mockLogger.Object
        );

        var name = "TestHero";
        var characterClass = Classes.Warrior;
        var userId = "user-123";

        var command = new CreateCharacterCommand { Name = name, ClassCode = (int)characterClass, UserId = userId };

        // --- 3. Act ---
        var result = await handler.Handle(command, CancellationToken.None);

        // --- 4. Assert ---
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Class, Is.EqualTo(characterClass));
            Assert.That(result.UserId, Is.EqualTo(userId));
            
            // Verify Repository was called
            mockRepo.Verify(
                r => r.AddAsync(
                    It.Is<Character>(c => c.Name == name && c.Class == characterClass && c.UserId == userId)),
                Times.Once);

            // (Optional) Verify the Event was published to the bus
            mockBus.Verify(
                b => b.Publish(It.IsAny<CreateCharacterEvent>(), It.IsAny<IDictionary<string, string>>()), 
                Times.Once);
        });
    }
}