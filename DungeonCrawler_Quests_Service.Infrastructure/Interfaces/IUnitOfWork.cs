namespace DungeonCrawler_Quests_Service.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    
}