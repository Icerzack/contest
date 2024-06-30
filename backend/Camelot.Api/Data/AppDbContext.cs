namespace Camelot.Api.Data;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
  // public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  //test-only
  // [assembly: InternalsVisibleTo("Camelot.Api.Tests")]
  public AppDbContext(DbContextOptions options) : base(options) { }

  public DbSet<User> Users { get; set; }
  public DbSet<Collection> Collections { get; set; }
  public DbSet<Board> Boards { get; set; }

  public DbSet<Collection2Board> Coll2Board { get; set; }
  public DbSet<User2Collection> User2Coll { get; set; }

}
