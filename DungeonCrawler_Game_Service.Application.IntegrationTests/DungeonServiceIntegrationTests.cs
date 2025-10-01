using DungeonCrawler_Game_Service.Application.Services;
using DungeonCrawler_Game_Service.Domain.Entities;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace DungeonCrawler_Game_Service.Application.IntegrationTests
{
    [TestFixture]
    public class DungeonServiceIntegrationTests
    {
        private MongoDbRunner _mongoRunner;
        private IMongoDatabase _database;

        // Dépendances injectées
        private IRepository<Dungeon> _dungeonRepository;
        private IUnitOfWork _unitOfWork;
        private DungeonService _dungeonService;

        [SetUp]
        public void Setup()
        {
            // Lance un MongoDB in-memory
            _mongoRunner = MongoDbRunner.Start();
            var client = new MongoClient(_mongoRunner.ConnectionString);
            _database = client.GetDatabase("TestDungeonDb");

            // Création d'un repository concret
            _dungeonRepository = new MongoRepository<Dungeon>(_database, "Dungeons");

            // UnitOfWork factice qui injecte le repository
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.GetRepository<Dungeon>())
                          .Returns(_dungeonRepository);
            _unitOfWork = unitOfWorkMock.Object;

            // DungeonService reçoit l'instance injectée
            _dungeonService = new DungeonService(_unitOfWork);
        }

        [TearDown]
        public void TearDown()
        {
            _mongoRunner.Dispose();
        }

        [Test]
        public async Task GenerateDungeonAsync_ShouldPersistDungeonInDatabase()
        {
            var dungeon = await _dungeonService.GenerateDungeonAsync();

            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.Levels.Count, Is.InRange(15, 20));

            var storedDungeon = await _dungeonRepository.GetByIdAsync(dungeon.Id);
            Assert.That(storedDungeon, Is.Not.Null);
            Assert.That(storedDungeon.Id, Is.EqualTo(dungeon.Id));
            Assert.That(storedDungeon.Levels.Count, Is.EqualTo(dungeon.Levels.Count));
        }

        [Test]
        public async Task GenerateDungeonAsync_ShouldHaveBossRoomAtLastLevel_InDatabase()
        {
            var dungeon = await _dungeonService.GenerateDungeonAsync();
            var storedDungeon = await _dungeonRepository.GetByIdAsync(dungeon.Id);

            Assert.That(storedDungeon.Levels.Last().Rooms.Count, Is.EqualTo(1));
            Assert.That(storedDungeon.Levels.Last().Rooms.First().Type, Is.EqualTo(RoomType.Boss));
        }

        [Test]
        public async Task GenerateDungeonAsync_ShouldLinkRoomsToNextLevel()
        {
            var dungeon = await _dungeonService.GenerateDungeonAsync();
            var storedDungeon = await _dungeonRepository.GetByIdAsync(dungeon.Id);

            for (int i = 0; i < storedDungeon.Levels.Count - 1; i++)
            {
                var currentLevel = storedDungeon.Levels[i];
                var nextLevel = storedDungeon.Levels[i + 1];

                foreach (var room in currentLevel.Rooms)
                {
                    Assert.That(nextLevel.Rooms.Any(r => r.Id == room.NextRoomId),
                        $"Room {room.Id} does not link correctly to next level {i + 2}");
                }
            }
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnStoredDungeons()
        {
            await _dungeonService.GenerateDungeonAsync();
            await _dungeonService.GenerateDungeonAsync();

            var allDungeons = await _dungeonRepository.GetAllAsync();
            Assert.That(allDungeons.Count(), Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public async Task FindAsync_ShouldReturnDungeonById()
        {
            var dungeon = await _dungeonService.GenerateDungeonAsync();

            var found = await _dungeonRepository.FindAsync(d => d.Id == dungeon.Id);

            Assert.That(found.FirstOrDefault(), Is.Not.Null);
            Assert.That(found.First().Id, Is.EqualTo(dungeon.Id));
        }
    }
}