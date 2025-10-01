using DefaultNamespace;
using DungeonCrawler_Game_Service.Application.Services;
using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using DungeonCrawler_Game_Service.Infrastructure.Repositories;
using Mongo2Go;
using MongoDB.Driver;
using Moq;

namespace DungeonCrawler_Game_Service.Application.IntegrationTests
{

    [TestFixture]
    public class CharacterServiceIntegrationTests
    {
        private MongoDbRunner _mongoRunner;
        private IMongoDatabase _database;

        // Dépendances injectées
        private IRepository<Character> _characterRepository;
        private IUnitOfWork _unitOfWork;
        private CharacterService _characterService;

        [SetUp]
        public void Setup()
        {
            // Lance un MongoDB in-memory
            _mongoRunner = MongoDbRunner.Start();
            var client = new MongoClient(_mongoRunner.ConnectionString);
            _database = client.GetDatabase("TestCharatcerDb");

            // Création d'un repository concret
            _characterRepository = new MongoRepository<Character>(_database, "Characters");

            // UnitOfWork factice qui injecte le repository
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.GetRepository<Character>())
                .Returns(_characterRepository);
            _unitOfWork = unitOfWorkMock.Object;

            // CharacterService reçoit l'instance injectée
            _characterService = new CharacterService(_unitOfWork);
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
            var character = await _characterService.CreateCharacterAsync("Hero", Classes.Warrior, "user-123");

            Assert.That(character, Is.Not.Null);
            Assert.That(character.Name, Is.EqualTo("Hero"));
            Assert.That(character.Class, Is.EqualTo(Classes.Warrior));
            Assert.That(character.UserId, Is.EqualTo("user-123"));

            var storedCharacter = await _characterRepository.GetByIdAsync(character.Id);
            Assert.That(storedCharacter, Is.Not.Null);
            Assert.That(storedCharacter.Id, Is.EqualTo(character.Id));
            Assert.That(storedCharacter.Name, Is.EqualTo(character.Name));
            Assert.That(storedCharacter.UserId, Is.EqualTo(character.UserId));
        }
    }
}