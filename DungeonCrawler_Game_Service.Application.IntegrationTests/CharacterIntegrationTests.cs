using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using DungeonCrawlerAssembly; // Ensure classes like 'Classes' are visible
using Microsoft.Extensions.Logging; // Required for Logger
using Rebus.Bus; // Required for IBus
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace DungeonCrawler_Game_Service.Application.IntegrationTests
{
    [TestFixture]
    public class CharacterIntegrationTests
    {
        private MongoDbRunner _mongoRunner;
        private IMongoDatabase _database;

        // Dépendances injectées
        private GenericRepository<Character> _characterRepository;
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            // Lance un MongoDB in-memory
            _mongoRunner = MongoDbRunner.Start();
            var client = new MongoClient(_mongoRunner.ConnectionString);
            _database = client.GetDatabase("TestCharatcerDb");

            // Création d'un repository concret
            _characterRepository = new GenericRepository<Character>(_database, "Characters");

            // UnitOfWork factice qui injecte le repository
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.GetRepository<Character>())
                .Returns(_characterRepository);
            _unitOfWork = unitOfWorkMock.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _mongoRunner.Dispose();
        }

        [Test]
        public async Task CreateCharacterAsync_ShouldPersistCharacterInDatabase()
        {
            // Arrange
            
            // 1. Mock the Bus (We don't want to actually publish to a queue in this test)
            var mockBus = new Mock<IBus>();
            // Important: Setup Publish to return a completed task, otherwise 'await' in the handler will throw NullReference
            mockBus.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<IDictionary<string, string>>()))
                   .Returns(Task.CompletedTask);

            // 2. Mock the Logger (We don't need to see logs in the test output)
            var mockLogger = new Mock<ILogger<CreateCharacterCommandHandler>>();

            // 3. Instantiate Handler with the Real UnitOfWork + Mocked Bus/Logger
            var handler = new CreateCharacterCommandHandler(_unitOfWork, mockBus.Object, mockLogger.Object);
            
            var command = new CreateCharacterCommand{ Name = "Hero", ClassCode = 1, UserId = "68de2d850d723cd498ac3a0c" };
            
            // Act
            var character = await handler.Handle(command, CancellationToken.None);
            
            // Check the Actual Database (using the repository)
            var storedCharacter = await _characterRepository.GetByIdAsync(character.Id);
            
            // Assert
            Assert.That(storedCharacter, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(character, Is.Not.Null);
                Assert.That(character.Name, Is.EqualTo("Hero"));
                Assert.That(character.Class, Is.EqualTo(Classes.Warrior));
                Assert.That(character.UserId, Is.EqualTo("68de2d850d723cd498ac3a0c"));
                
                Assert.That(storedCharacter.Id, Is.EqualTo(character.Id));
                Assert.That(storedCharacter.Name, Is.EqualTo(character.Name));
                Assert.That(storedCharacter.UserId, Is.EqualTo(character.UserId));
            });
        }
    }
}