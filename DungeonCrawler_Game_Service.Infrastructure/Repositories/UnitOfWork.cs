using DungeonCrawler_Game_Service.Infrastructure.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace DungeonCrawler_Game_Service.Infrastructure.Repositories;

public class UnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
{
    private readonly DbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(TDbContext context, IMapper mapper)
    {
        _context = context;
        _repositories = new();
    }

    public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        if (_repositories.ContainsKey(type))
            return (IRepository<TEntity>)_repositories[type];

        var repositoryInstance = new GenericRepository<TEntity>(_context);
        _repositories[type] = repositoryInstance;

        return repositoryInstance;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}