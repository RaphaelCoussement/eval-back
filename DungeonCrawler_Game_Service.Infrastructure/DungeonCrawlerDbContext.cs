using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DungeonCrawler_Game_Service.Infrastructure;

public class DungeonCrawlerDbContext : IdentityDbContext
{
    public DungeonCrawlerDbContext()
    {
        
    }
    
    public DungeonCrawlerDbContext(DbContextOptions<DungeonCrawlerDbContext> options) : base(options)
    {
    }
    
    // === DbSet principaux ===
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("");
    }
}