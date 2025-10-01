using DungeonCrawler_Game_Service.Application.Services;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using Moq;

namespace DungeonCrawler_Game_Service.Application.Testing
{
    [TestFixture]
    public class DungeonServiceTests
    {
        [Test]
        public async Task GenerateDungeonAsync_ShouldCreateDungeonWithLevelsAndRooms()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Dungeon>>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(uow => uow.GetRepository<Dungeon>())
                .Returns(mockRepo.Object);

            var service = new DungeonService(mockUnitOfWork.Object);

            // Act
            var dungeon = await service.GenerateDungeonAsync();

            // Assert
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.Levels.Count, Is.InRange(15, 20)); // nombre d'étages

            // Vérifie les salles de tous les étages sauf le dernier
            foreach (var level in dungeon.Levels.Take(dungeon.Levels.Count - 1))
            {
                Assert.That(level.Rooms.Count, Is.InRange(1, 3));
            }

            // dernier étage = boss
            Assert.That(dungeon.Levels.Last().Rooms.Count, Is.EqualTo(1));
            Assert.That(dungeon.Levels.Last().Rooms.First().Type, Is.EqualTo(RoomType.Boss));


            // Vérifie que AddAsync a été appelé
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Dungeon>()), Times.Once);
        }
        
        [Test]
        public async Task GenerateDungeonAsync_ShouldSetNextRoomIdCorrectly()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Dungeon>>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(uow => uow.GetRepository<Dungeon>())
                .Returns(mockRepo.Object);

            var service = new DungeonService(mockUnitOfWork.Object);

            // Act
            var dungeon = await service.GenerateDungeonAsync();

            // Assert
            Assert.That(dungeon, Is.Not.Null);

            // Vérifie toutes les salles sauf celles du dernier étage
            var allLevelsExceptLast = dungeon.Levels.Take(dungeon.Levels.Count - 1);
            foreach (var level in allLevelsExceptLast)
            {
                foreach (var room in level.Rooms)
                {
                    Assert.That(room.NextRoomId, Is.Not.Null.And.Not.Empty, 
                        $"Room {room.Id} du niveau {level.Number} doit avoir un NextRoomId défini");
                }
            }

            // Vérifie que le dernier étage n'a pas de NextRoomId
            var lastLevel = dungeon.Levels.Last();
            foreach (var room in lastLevel.Rooms)
            {
                Assert.That(room.NextRoomId, Is.Null, 
                    $"Room {room.Id} du dernier niveau ne doit pas avoir de NextRoomId");
            }
        }
    }
}