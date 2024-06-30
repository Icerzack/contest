namespace Camelot.Api.Utils;

using System.Linq;
using System.Threading.Tasks;

using Camelot.Api.Data;
using Camelot.Api.Exceptions;

using Microsoft.EntityFrameworkCore;

public class BoardAccess
{
  private readonly AppDbContext context;
  public BoardAccess(AppDbContext context) => this.context = context;

  public async Task<bool> GetAccess(int userId, int boardId)
  {
    var collection_id = await this.context.Coll2Board
            .Where(b => b.BoardId == boardId)
            .Select(x => x.CollectionId).FirstOrDefaultAsync();

    var userAccess = await this.context.User2Coll// is shared to user
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collection_id)
        .FirstOrDefaultAsync() is not null || // or user IS an Author
            await this.context.Boards
        .Where(x => x.BoardId == boardId)
        .Where(x => x.UserId == userId)
        .FirstOrDefaultAsync() is not null;
    if (userAccess)
    {
      return true;
    }
    else
    {
      throw new UserAccessException($"User user_id={userId} has no access to GET board {boardId}");
    }
  }

  public async Task<bool> DeleteAccess(int userId, int boardId)
  {
    var collection_id = await this.context.Coll2Board
         .Where(b => b.BoardId == boardId)
         .Select(x => x.CollectionId).FirstOrDefaultAsync();

    var userAccess = await this.context.User2Coll// is shared to user with moderator or owner rigths
      .Where(u => u.UserId == userId)
      .Where(u => u.CollectionId == collection_id)
      .Where(u => u.ShareMode == "moderator" || u.ShareMode == "owner")
      .FirstOrDefaultAsync() is not null || // or user IS an Author
            await this.context.Boards
        .Where(x => x.BoardId == boardId)
        .Where(x => x.UserId == userId)
        .FirstOrDefaultAsync() is not null;
    if (userAccess)
    {
      return true;
    }
    else
    {
      throw new UserAccessException($"User user_id={userId} has no access to DELETE board {boardId}");
    }
  }

  public async Task<bool> UpdateAccess(int userId, int boardId)
  {
    var collection_id = await this.context.Coll2Board
          .Where(c => c.BoardId == boardId)
          .Select(c => c.CollectionId).FirstOrDefaultAsync();

    var userAccess = await this.context.User2Coll
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collection_id)
        .Where(u => u.ShareMode == "moderator" || u.ShareMode == "owner" || u.ShareMode == "editor")
        .FirstOrDefaultAsync() is not null || // or user IS an Author
            await this.context.Boards
        .Where(x => x.BoardId == boardId)
        .Where(x => x.UserId == userId)
        .FirstOrDefaultAsync() is not null;
    if (userAccess)
    {
      return true;
    }
    else
    {
      throw new UserAccessException($"User user_id={userId} has no access to EDIT board {boardId}");
    }
  }

}
