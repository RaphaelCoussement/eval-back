using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Features.Characters.Commands;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using Mongo2Go;
using MongoDB.Driver;
using Moq;

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

        /// <summary>
        /// Configuration avant chaque test, incluant le démarrage d'une instance MongoDB in-memory
        /// et la création d'un repository concret pour les personnages.
        /// </summary>
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

        /// <summary>
        ///  Verification de la bonne création d'un personnage + persistance en base de données
        /// </summary>
        [Test]
        public async Task CreateCharacterAsync_ShouldPersistCharacterInDatabase()
        {
            //Arrange
            var handler = new CreateCharacterCommandHandler(_unitOfWork);
            var command = new CreateCharacterCommand{ Name = "Hero", ClassCode = 1, UserId = "68de2d850d723cd498ac3a0c" };
            
            //Act
            var character = await handler.Handle(command, CancellationToken.None);
            var storedCharacter = await _characterRepository.GetByIdAsync(character.Id);
            
            //Assert
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