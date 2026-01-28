using DungeonCrawler_Quests_Service.Application.Features.Events;
using DungeonCrawler_Quests_Service.Domain.Entities;
using DungeonCrawler_Quests_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly.Messages;
using Moq;
using Microsoft.Extensions.Logging;

namespace DungeonCrawler_Quests_Service.Application.Testing;

[TestFixture]
public class DungeonCompletedHandlerTests
{
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ILogger<DungeonCompletedHandler>> _loggerMock;
    private DungeonCompletedHandler _handler;

    [SetUp]
    public void Setup()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<DungeonCompletedHandler>>();
        _handler = new DungeonCompletedHandler(_uowMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Handle_ShouldNotUpdateProgress_IfEventIdAlreadyProcessed()
    {
        // Arrange : Utilisation de IRepository (nom correct dans ton infra)
        var existingEventId = Guid.NewGuid();
        var processedEvents = new List<ProcessedEvent> 
        { 
            new ProcessedEvent { EventId = existingEventId } 
        };

        // On mock IRepository car c'est ce que ton UnitOfWork renvoie
        var eventRepoMock = new Mock<IRepository<ProcessedEvent>>();
        eventRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(processedEvents);
        
        // Configuration du UnitOfWork pour renvoyer notre mock de repository
        _uowMock.Setup(x => x.GetRepository<ProcessedEvent>()).Returns(eventRepoMock.Object);

        var message = new DungeonCompleted { EventId = existingEventId, PlayerId = Guid.NewGuid() };

        // Act
        await _handler.Handle(message);

        // Assert : On vérifie l'idempotence
        // Si l'événement est déjà traité, on ne doit jamais appeler le repo des quêtes
        _uowMock.Verify(x => x.GetRepository<QuestDefinition>(), Times.Never);
    }
}