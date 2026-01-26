using DungeonCrawler_Game_Service.Application.Features.EnterRoom.Queries;
using DungeonCrawler_Game_Service.Application.Features.LinkRooms.Commands;
using DungeonCrawler_Game_Service.Application.Features.NextRooms.Queries;
using DungeonCrawler_Game_Service.Application.Features.ProceduralDungeons.Commands;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace DungeonCrawler_Game_Service.Application.Testing;

[TestFixture]
public class DungeonUnitTests
{
    private Mock<IRepository<Dungeon>> _mockDungeonRepo;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<GenerateDungeonCommandHandler>> _mockGenerateLogger;
    private Mock<ILogger<LinkRoomsCommandHandler>> _mockLinkRoomsLogger;
    private Mock<ILogger<EnterRoomQueryHandler>> _mockEnterRoomLogger;
    private Mock<ILogger<GetNextRoomsQueryHandler>> _mockGetNextRoomsLogger;

    [SetUp]
    public void SetUp()
    {
        _mockDungeonRepo = new Mock<IRepository<Dungeon>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockGenerateLogger = new Mock<ILogger<GenerateDungeonCommandHandler>>();
        _mockLinkRoomsLogger = new Mock<ILogger<LinkRoomsCommandHandler>>();
        _mockEnterRoomLogger = new Mock<ILogger<EnterRoomQueryHandler>>();
        _mockGetNextRoomsLogger = new Mock<ILogger<GetNextRoomsQueryHandler>>();

        _mockUnitOfWork.Setup(u => u.GetRepository<Dungeon>()).Returns(_mockDungeonRepo.Object);
    }

    #region GenerateDungeonCommand Tests

    [Test]
    public async Task GenerateDungeon_ShouldCreateDungeonWithMultipleLevels()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.AddAsync(It.IsAny<Dungeon>())).Returns(Task.CompletedTask);

        var handler = new GenerateDungeonCommandHandler(
            _mockUnitOfWork.Object,
            _mockGenerateLogger.Object
        );

        var command = new GenerateDungeonCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Levels, Is.Not.Empty);
            Assert.That(result.Levels.Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(result.Levels.Count, Is.LessThanOrEqualTo(15));
            Assert.That(result.Seed, Is.Not.Empty);
        });

        _mockDungeonRepo.Verify(r => r.AddAsync(It.IsAny<Dungeon>()), Times.Once);
    }

    [Test]
    public async Task GenerateDungeon_ShouldHaveEntranceRoomOnFirstLevel()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.AddAsync(It.IsAny<Dungeon>())).Returns(Task.CompletedTask);

        var handler = new GenerateDungeonCommandHandler(
            _mockUnitOfWork.Object,
            _mockGenerateLogger.Object
        );

        var command = new GenerateDungeonCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var firstLevel = result.Levels.First();
        Assert.Multiple(() =>
        {
            Assert.That(firstLevel.Number, Is.EqualTo(1));
            Assert.That(firstLevel.Rooms, Has.Count.EqualTo(1));
            Assert.That(firstLevel.Rooms.First().Type, Is.EqualTo(RoomType.Entrance));
        });
    }

    [Test]
    public async Task GenerateDungeon_ShouldHaveBossRoomOnLastLevel()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.AddAsync(It.IsAny<Dungeon>())).Returns(Task.CompletedTask);

        var handler = new GenerateDungeonCommandHandler(
            _mockUnitOfWork.Object,
            _mockGenerateLogger.Object
        );

        var command = new GenerateDungeonCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var lastLevel = result.Levels.Last();
        Assert.Multiple(() =>
        {
            Assert.That(lastLevel.Rooms, Has.Count.EqualTo(1));
            Assert.That(lastLevel.Rooms.First().Type, Is.EqualTo(RoomType.BossRoom));
            Assert.That(lastLevel.Rooms.First(), Is.InstanceOf<BossRoom>());
        });
    }

    [Test]
    public async Task GenerateDungeon_ShouldCreateUniqueSeedForEachDungeon()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.AddAsync(It.IsAny<Dungeon>())).Returns(Task.CompletedTask);

        var handler = new GenerateDungeonCommandHandler(
            _mockUnitOfWork.Object,
            _mockGenerateLogger.Object
        );

        // Act
        var dungeon1 = await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
        var dungeon2 = await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);

        // Assert - Les seeds devraient être différents (probabilité très élevée)
        Assert.That(dungeon1.Seed, Is.Not.EqualTo(dungeon2.Seed));
    }

    #endregion

    #region LinkRoomsCommand Tests

    [Test]
    public async Task LinkRooms_ShouldCreateNewLink_WhenNotExists()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 1,
                    Rooms = new List<Room>
                    {
                        new CombatRoom { Id = "room-1", Type = RoomType.CombatRoom },
                        new CombatRoom { Id = "room-2", Type = RoomType.CombatRoom }
                    }
                }
            },
            Links = new List<RoomLink>()
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);
        _mockDungeonRepo.Setup(r => r.UpdateAsync(It.IsAny<Dungeon>()))
            .Returns(Task.CompletedTask);

        var handler = new LinkRoomsCommandHandler(
            _mockUnitOfWork.Object,
            _mockLinkRoomsLogger.Object
        );

        var command = new LinkRoomsCommand("dungeon-123", "room-1", "room-2");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Links, Has.Count.EqualTo(1));
            Assert.That(result.Links.First().FromRoomId, Is.EqualTo("room-1"));
            Assert.That(result.Links.First().ToRoomId, Is.EqualTo("room-2"));
        });

        _mockDungeonRepo.Verify(r => r.UpdateAsync(It.IsAny<Dungeon>()), Times.Once);
    }

    [Test]
    public async Task LinkRooms_ShouldNotCreateDuplicate_WhenLinkAlreadyExists()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Links = new List<RoomLink>
            {
                new RoomLink { FromRoomId = "room-1", ToRoomId = "room-2" }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new LinkRoomsCommandHandler(
            _mockUnitOfWork.Object,
            _mockLinkRoomsLogger.Object
        );

        var command = new LinkRoomsCommand("dungeon-123", "room-1", "room-2");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result.Links, Has.Count.EqualTo(1));
        _mockDungeonRepo.Verify(r => r.UpdateAsync(It.IsAny<Dungeon>()), Times.Never);
    }

    [Test]
    public void LinkRooms_ShouldThrowKeyNotFoundException_WhenDungeonNotExists()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Dungeon?)null);

        var handler = new LinkRoomsCommandHandler(
            _mockUnitOfWork.Object,
            _mockLinkRoomsLogger.Object
        );

        var command = new LinkRoomsCommand("non-existent", "room-1", "room-2");

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region EnterRoomQuery Tests

    [Test]
    public async Task EnterRoom_ShouldReturnRoomProgress_WhenRoomExists()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 1,
                    Rooms = new List<Room>
                    {
                        new CombatRoom { Id = "room-1", MonsterNb = 3, Type = RoomType.CombatRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new EnterRoomQueryHandler(
            _mockUnitOfWork.Object,
            _mockEnterRoomLogger.Object
        );

        var query = new EnterRoomQuery("dungeon-123", "room-1");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.RoomId, Is.EqualTo("room-1"));
            Assert.That(result.Level, Is.EqualTo(1));
            Assert.That(result.Events, Is.Not.Empty);
        });
    }

    [Test]
    public async Task EnterRoom_ShouldReturnCombatEvent_WhenEnteringCombatRoom()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 2,
                    Rooms = new List<Room>
                    {
                        new CombatRoom { Id = "combat-room", MonsterNb = 5, Type = RoomType.CombatRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new EnterRoomQueryHandler(
            _mockUnitOfWork.Object,
            _mockEnterRoomLogger.Object
        );

        var query = new EnterRoomQuery("dungeon-123", "combat-room");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Events, Does.Contain("Vous affrontez 5 monstre(s) !"));
            Assert.That(result.Level, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task EnterRoom_ShouldReturnTreasureEvent_WhenEnteringTreasureRoom()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 3,
                    Rooms = new List<Room>
                    {
                        new TreasureRoom { Id = "treasure-room", Piece = 100, Type = RoomType.TreasureRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new EnterRoomQueryHandler(
            _mockUnitOfWork.Object,
            _mockEnterRoomLogger.Object
        );

        var query = new EnterRoomQuery("dungeon-123", "treasure-room");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Events, Does.Contain("Vous trouvez 100 pièces d'or !"));
    }

    [Test]
    public async Task EnterRoom_ShouldReturnTrapEvent_WhenEnteringTrapRoom()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 4,
                    Rooms = new List<Room>
                    {
                        new TrapRoom { Id = "trap-room", Damage = 25, Type = RoomType.TrapRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new EnterRoomQueryHandler(
            _mockUnitOfWork.Object,
            _mockEnterRoomLogger.Object
        );

        var query = new EnterRoomQuery("dungeon-123", "trap-room");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Events, Does.Contain("Vous tombez dans un piège ! Dégâts : 25"));
    }

    [Test]
    public async Task EnterRoom_ShouldReturnBossEvent_WhenEnteringBossRoom()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 10,
                    Rooms = new List<Room>
                    {
                        new BossRoom { Id = "boss-room", NameBoss = "Dragon Final", Type = RoomType.BossRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new EnterRoomQueryHandler(
            _mockUnitOfWork.Object,
            _mockEnterRoomLogger.Object
        );

        var query = new EnterRoomQuery("dungeon-123", "boss-room");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result.Events, Does.Contain("Vous rencontrez le Dragon Final !"));
    }

    [Test]
    public void EnterRoom_ShouldThrowKeyNotFoundException_WhenDungeonNotExists()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Dungeon?)null);

        var handler = new EnterRoomQueryHandler(
            _mockUnitOfWork.Object,
            _mockEnterRoomLogger.Object
        );

        var query = new EnterRoomQuery("non-existent", "room-1");

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.Handle(query, CancellationToken.None));
    }

    #endregion

    #region GetNextRoomsQuery Tests

    [Test]
    public async Task GetNextRooms_ShouldReturnRoomsFromNextLevel()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 1,
                    Rooms = new List<Room>
                    {
                        new CombatRoom { Id = "room-1", Type = RoomType.CombatRoom }
                    }
                },
                new Level
                {
                    Number = 2,
                    Rooms = new List<Room>
                    {
                        new CombatRoom { Id = "room-2", Type = RoomType.CombatRoom },
                        new TreasureRoom { Id = "room-3", Type = RoomType.TreasureRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new GetNextRoomsQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetNextRoomsLogger.Object
        );

        var query = new GetNextRoomsQuery("dungeon-123", "room-1");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(r => r.Id), Does.Contain("room-2"));
            Assert.That(result.Select(r => r.Id), Does.Contain("room-3"));
        });
    }

    [Test]
    public async Task GetNextRooms_ShouldReturnEmptyList_WhenOnLastLevel()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 1,
                    Rooms = new List<Room>
                    {
                        new BossRoom { Id = "boss-room", Type = RoomType.BossRoom, NameBoss = "Final Boss" }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new GetNextRoomsQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetNextRoomsLogger.Object
        );

        var query = new GetNextRoomsQuery("dungeon-123", "boss-room");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetNextRooms_ShouldReturnEmptyList_WhenRoomNotFound()
    {
        // Arrange
        var dungeon = new Dungeon
        {
            Id = "dungeon-123",
            Levels = new List<Level>
            {
                new Level
                {
                    Number = 1,
                    Rooms = new List<Room>
                    {
                        new CombatRoom { Id = "room-1", Type = RoomType.CombatRoom }
                    }
                }
            }
        };

        _mockDungeonRepo.Setup(r => r.GetByIdAsync("dungeon-123"))
            .ReturnsAsync(dungeon);

        var handler = new GetNextRoomsQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetNextRoomsLogger.Object
        );

        var query = new GetNextRoomsQuery("dungeon-123", "non-existent-room");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetNextRooms_ShouldThrowKeyNotFoundException_WhenDungeonNotExists()
    {
        // Arrange
        _mockDungeonRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Dungeon?)null);

        var handler = new GetNextRoomsQueryHandler(
            _mockUnitOfWork.Object,
            _mockGetNextRoomsLogger.Object
        );

        var query = new GetNextRoomsQuery("non-existent", "room-1");

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(async () => await handler.Handle(query, CancellationToken.None));
    }

    #endregion
}

