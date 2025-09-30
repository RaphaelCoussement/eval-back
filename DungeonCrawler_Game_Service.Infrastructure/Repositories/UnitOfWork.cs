using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MongoDB.Driver;

namespace DungeonCrawler_Game_Service.Infrastructure.Repositories;

public class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoDatabase _database;
    private readonly Dictionary<Type, object> _repositories = new();

    public MongoUnitOfWork(IMongoDatabase database)
    {
        _database = database;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (_repositories.ContainsKey(type))
            return (IRepository<TEntity>)_repositories[type];

        var collectionName = typeof(TEntity).Name + "s";
        var repo = new MongoRepository<TEntity>(_database, collectionName);
        _repositories[type] = repo;
        return repo;
    }

    public void Dispose() { }
}