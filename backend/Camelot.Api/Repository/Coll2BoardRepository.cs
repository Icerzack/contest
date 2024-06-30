namespace Camelot.Api.Repository;

using System.Linq;
using System.Threading.Tasks;
using Camelot.Api.Data;
using Camelot.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class Coll2BoardRepository
{
  private readonly AppDbContext context;

  public ILogger Logger { get; }
  public Coll2BoardRepository(AppDbContext context, ILoggerFactory factory)
  {
    this.context = context;
    this.Logger = factory.CreateLogger<Coll2BoardRepository>();
  }

  public async Task AddRelation(int collectionId, int boardId)
  {

    var isAlreadyAdded = await this.context.Coll2Board
        .Where(e => e.BoardId == boardId)
        .FirstOrDefaultAsync() != null;
    if (isAlreadyAdded)
    {
      throw new DuplicateException("Board is already added to collection");
    }

    this.context.Coll2Board.Add(new Collection2Board
    {
      BoardId = boardId,
      CollectionId = collectionId
    });
    await this.context.SaveChangesAsync();
  }

  public async Task DeleteBoardsRelation(int boardId)
  {
    var relations = await this.context.Coll2Board
          .Where(x => x.BoardId == boardId)
          .ToListAsync();
    this.context.Coll2Board.RemoveRange(relations);

    await this.context.SaveChangesAsync();
  }
  public async Task DeleteCollectionsRelation(int collectionId)
  {
    var relations = await this.context.Coll2Board
          .Where(x => x.CollectionId == collectionId)
          .ToListAsync();
    this.context.Coll2Board.RemoveRange(relations);

    await this.context.SaveChangesAsync();
  }

}
