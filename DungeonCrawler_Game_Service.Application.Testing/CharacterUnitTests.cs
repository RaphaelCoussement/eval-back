using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Application.Features.Characters.Queries;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawlerAssembly;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Moq;

namespace DungeonCrawler_Game_Service.Application.Testing;

[TestFixture]
public class CharacterUnitTests
{
    private Mock<IRepository<Character>> _mockCharacterRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IBus> _mockBus;
    private Mock<ILogger<CreateCharacterCommandHandler>> _mockCreateLogger;
    private Mock<ILogger<EquipSkinCommandHandler>> _mockEquipSkinLogger;
    private Mock<ILogger<GetCharacterByIdQueryHandler>> _mockGetCharacterLogger;
    private Mock<ILogger<GetEquippedSkinIdQueryHandler>> _mockGetSkinLogger;

    [SetUp]
    public void SetUp()
    {
        // Initialisation des mocks pour tous les tests
        _mockCharacterRepo = new Mock<IRepository<Character>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockBus = new Mock<IBus>();
        _mockCreateLogger = new Mock<ILogger<CreateCharacterCommandHandler>>();
        _mockEquipSkinLogger = new Mock<ILogger<EquipSkinCommandHandler>>();
        _mockGetCharacterLogger = new Mock<ILogger<GetCharacterByIdQueryHandler>>();
        _mockGetSkinLogger = new Mock<ILogger<GetEquippedSkinIdQueryHandler>>();

        // Configuration du UnitOfWork pour retourner le repository
        _mockUnitOfWork.Setup(u => u.GetRepository<Character>()).Returns(_mockCharacterRepo.Object);
        
        // Configuration du Bus pour retourner une tâche complétée
        _mockBus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<IDictionary<string, string>>()))
               .Returns(Task.CompletedTask);
    }

    #region CreateCharacterCommand Tests

    [Test]
    public async Task CreateCharacterCommand_ShouldCreateCharacterSuccessfully()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.AddAsync(It.IsAny<Character>())).Returns(Task.CompletedTask);
        
        var handler = new CreateCharacterCommandHandler(
            _mockUnitOfWork.Object, 
            _mockBus.Object, 
            _mockCreateLogger.Object
        );

        var command = new CreateCharacterCommand 
        { 
            Name = "TestHero", 
            ClassCode = (int)Classes.Warrior, 
            UserId = "user-123" 
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("TestHero"));
            Assert.That(result.Class, Is.EqualTo(Classes.Warrior));
            Assert.That(result.UserId, Is.EqualTo("user-123"));
        });
        
        _mockCharacterRepo.Verify(r => r.AddAsync(It.IsAny<Character>()), Times.Once);
        _mockBus.Verify(b => b.Publish(It.IsAny<object>(), It.IsAny<IDictionary<string, string>>()), Times.Once);
    }

    [Test]
    public async Task CreateCharacterCommand_ShouldCreateWizardClass()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.AddAsync(It.IsAny<Character>())).Returns(Task.CompletedTask);
        
        var handler = new CreateCharacterCommandHandler(
            _mockUnitOfWork.Object, 
            _mockBus.Object, 
            _mockCreateLogger.Object
        );

        var command = new CreateCharacterCommand 
        { 
            Name = "Gandalf", 
            ClassCode = (int)Classes.Wizard, 
            UserId = "user-456" 
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Gandalf"));
            Assert.That(result.Class, Is.EqualTo(Classes.Wizard));
            Assert.That(result.UserId, Is.EqualTo("user-456"));
        });
    }

    [Test]
    public async Task CreateCharacterCommand_ShouldCreateShamanClass()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.AddAsync(It.IsAny<Character>())).Returns(Task.CompletedTask);
        
        var handler = new CreateCharacterCommandHandler(
            _mockUnitOfWork.Object, 
            _mockBus.Object, 
            _mockCreateLogger.Object
        );

        var command = new CreateCharacterCommand 
        { 
            Name = "Thrall", 
            ClassCode = (int)Classes.Shaman, 
            UserId = "user-789" 
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Thrall"));
            Assert.That(result.Class, Is.EqualTo(Classes.Shaman));
            Assert.That(result.UserId, Is.EqualTo("user-789"));
        });
    }

    #endregion

    #region EquipSkinCommand Tests

    [Test]
    public async Task EquipSkinCommand_ShouldEquipSkinSuccessfully()
    {
        // Arrange
        var character = new Character
        {
            Id = "character-123",
            Name = "Hero",
            Class = Classes.Warrior,
            UserId = "user-123",
            ActiveSkinId = "old-skin"
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync("character-123"))
            .ReturnsAsync(character);
        _mockCharacterRepo.Setup(r => r.UpdateAsync(It.IsAny<Character>()))
            .Returns(Task.CompletedTask);

        var handler = new EquipSkinCommandHandler(
            _mockUnitOfWork.Object,
            _mockBus.Object,
            _mockEquipSkinLogger.Object
        );

        var command = new EquipSkinCommand
        {
            CharacterId = "character-123",
            SkinId = "new-skin-456"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(character.ActiveSkinId, Is.EqualTo("new-skin-456"));
        _mockCharacterRepo.Verify(r => r.UpdateAsync(It.Is<Character>(c => c.ActiveSkinId == "new-skin-456")), Times.Once);
    }

    [Test]
    public void EquipSkinCommand_ShouldThrowException_WhenCharacterNotFound()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Character?)null);

        var handler = new EquipSkinCommandHandler(
            _mockUnitOfWork.Object,
            _mockBus.Object,
            _mockEquipSkinLogger.Object
        );

        var command = new EquipSkinCommand
        {
            CharacterId = "non-existent",
            SkinId = "skin-123"
        };

        // Act & Assert
        Assert.ThrowsAsync<Exception>(async () => await handler.Handle(command, CancellationToken.None));
        _mockCharacterRepo.Verify(r => r.UpdateAsync(It.IsAny<Character>()), Times.Never);
    }

    #endregion

    #region GetCharacterByIdQuery Tests

    [Test]
    public async Task GetCharacterById_ShouldReturnCharacter_WhenExists()
    {
        // Arrange
        var expectedCharacter = new Character
        {
            Id = "character-123",
            Name = "TestHero",
            Class = Classes.Warrior,
            UserId = "user-123",
            ActiveSkinId = "skin-456"
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync("character-123"))
            .ReturnsAsync(expectedCharacter);

        var handler = new GetCharacterByIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetCharacterLogger.Object
        );

        var query = new GetCharacterByIdQuery("character-123");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo("character-123"));
            Assert.That(result.Name, Is.EqualTo("TestHero"));
            Assert.That(result.Class, Is.EqualTo(Classes.Warrior));
            Assert.That(result.UserId, Is.EqualTo("user-123"));
        });
    }

    [Test]
    public void GetCharacterById_ShouldThrowKeyNotFoundException_WhenNotExists()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Character?)null);

        var handler = new GetCharacterByIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetCharacterLogger.Object
        );

        var query = new GetCharacterByIdQuery("non-existent-id");

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.Handle(query, CancellationToken.None));
    }

    #endregion

    #region GetEquippedSkinIdQuery Tests

    [Test]
    public async Task GetEquippedSkinId_ShouldReturnSkinId_WhenCharacterExists()
    {
        // Arrange
        var character = new Character
        {
            Id = "character-123",
            Name = "Hero",
            ActiveSkinId = "skin-789"
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync("character-123"))
            .ReturnsAsync(character);

        var handler = new GetEquippedSkinIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetSkinLogger.Object
        );

        var query = new GetEquippedSkinIdQuery("character-123");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo("skin-789"));
    }

    [Test]
    public async Task GetEquippedSkinId_ShouldReturnEmptyString_WhenNoSkinEquipped()
    {
        // Arrange
        var character = new Character
        {
            Id = "character-123",
            Name = "Hero",
            ActiveSkinId = string.Empty
        };

        _mockCharacterRepo.Setup(r => r.GetByIdAsync("character-123"))
            .ReturnsAsync(character);

        var handler = new GetEquippedSkinIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetSkinLogger.Object
        );

        var query = new GetEquippedSkinIdQuery("character-123");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetEquippedSkinId_ShouldThrowKeyNotFoundException_WhenCharacterNotExists()
    {
        // Arrange
        _mockCharacterRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Character?)null);

        var handler = new GetEquippedSkinIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetSkinLogger.Object
        );

        var query = new GetEquippedSkinIdQuery("non-existent-id");

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.Handle(query, CancellationToken.None));
    }

    #endregion
}


