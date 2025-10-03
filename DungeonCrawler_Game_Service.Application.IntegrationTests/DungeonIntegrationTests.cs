using DungeonCrawler_Game_Service.Application.Features.Dungeons.Commands;
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
    public class DungeonIntegrationTests
    {
        private MongoDbRunner _mongoRunner;
        private IMongoDatabase _database;

        // Dépendances injectées
        private IRepository<Dungeon> _dungeonRepository;
        private IUnitOfWork _unitOfWork;

        /// <summary>
        /// Setup avant chaque test pour initialiser une base de données MongoDB en mémoire.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Démarre une instance MongoDB en mémoire
            _mongoRunner = MongoDbRunner.Start();
            var client = new MongoClient(_mongoRunner.ConnectionString);
            _database = client.GetDatabase("TestDungeonDb");

            // Initialise les dépôts et le unit of work
            _dungeonRepository = new GenericRepository<Dungeon>(_database, "Dungeons");
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.GetRepository<Dungeon>())
                .Returns(_dungeonRepository);
            _unitOfWork = unitOfWorkMock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _mongoRunner.Dispose();
        }

        /// <summary>
        /// Teste la génération d'un donjon et sa persistance dans la base de données.
        /// </summary>
        [Test]
        public async Task GenerateDungeonAsync_ShouldPersistDungeonInDatabase()
        {
            //Arrange
            var handler = new GenerateDungeonCommandHandler(_unitOfWork);
            
            //Act
            var dungeon = await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
            
            //Assert
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.Levels.Count, Is.InRange(15, 20));

            var storedDungeon = await _dungeonRepository.GetByIdAsync(dungeon.Id);
            Assert.That(storedDungeon, Is.Not.Null);
            Assert.That(storedDungeon.Id, Is.EqualTo(dungeon.Id));
            Assert.That(storedDungeon.Levels.Count, Is.EqualTo(dungeon.Levels.Count));
        }

        /// <summary>
        /// Teste que le dernier niveau du donjon contient une salle de boss.
        /// </summary>
        [Test]
        public async Task GenerateDungeonAsync_ShouldHaveBossRoomAtLastLevel_InDatabase()
        {
            //Arrange
            var handler = new GenerateDungeonCommandHandler(_unitOfWork);
            
            //Act
            var dungeon = await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
            
            //Assert
            var storedDungeon = await _dungeonRepository.GetByIdAsync(dungeon.Id);

            Assert.That(storedDungeon.Levels.Last().Rooms.Count, Is.EqualTo(1));
            Assert.That(storedDungeon.Levels.Last().Rooms.First().Type, Is.EqualTo(RoomType.Boss));
        }

        /// <summary>
        /// Teste que chaque salle d'un niveau est correctement liée à une salle du niveau suivant.
        /// </summary>
        [Test]
        public async Task GenerateDungeonAsync_ShouldLinkRoomsToNextLevel()
        {
            //Arrange
            var handler = new GenerateDungeonCommandHandler(_unitOfWork);
            
            //Act
            var dungeon = await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
            
            //Assert
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

        /// <summary>
        /// Teste la récupération de tous les donjons stockés dans la base de données.
        /// </summary>
        [Test]
        public async Task GetAllAsync_ShouldReturnStoredDungeons()
        {
            //Arrange
            var handler = new GenerateDungeonCommandHandler(_unitOfWork);
            
            //Act
            await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
            await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
            
            //Assert

            var allDungeons = await _dungeonRepository.GetAllAsync();
            Assert.That(allDungeons.Count(), Is.GreaterThanOrEqualTo(2));
        }

        /// <summary>
        /// Teste la recherche d'un donjon par son ID.
        /// </summary>
        [Test]
        public async Task FindAsync_ShouldReturnDungeonById()
        {
            //Arrange
            var handler = new GenerateDungeonCommandHandler(_unitOfWork);
            
            //Act
            var dungeon = await handler.Handle(new GenerateDungeonCommand(), CancellationToken.None);
            
            //Assert
            var found = await _dungeonRepository.FindAsync(d => d.Id == dungeon.Id);

            Assert.That(found.FirstOrDefault(), Is.Not.Null);
            Assert.That(found.First().Id, Is.EqualTo(dungeon.Id));
        }
    }
}