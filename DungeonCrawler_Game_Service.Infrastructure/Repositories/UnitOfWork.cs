using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MongoDB.Driver;

namespace DungeonCrawler_Game_Service.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IMongoDatabase _database;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(IMongoDatabase database)
    {
        _database = database;
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (_repositories.TryGetValue(type, out var repository))
            return (IRepository<TEntity>)repository;

        var collectionName = typeof(TEntity).Name + "s";
        var repo = new GenericRepository<TEntity>(_database, collectionName);
        _repositories[type] = repo;
        return repo;
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Libération des ressources managées
            foreach (var repo in _repositories.Values)
            {
                if (repo is IDisposable disposableRepo)
                    disposableRepo.Dispose();
            }
            _repositories.Clear();
        }

        // Libération des ressources non managées si nécessaire

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

// Finaliseur au cas où des ressources non managées seraient ajoutées
    ~UnitOfWork()
    {
        Dispose(false);
    }


}