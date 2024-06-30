namespace Camelot.Api.Utils;

using System.Linq;
using System.Threading.Tasks;

using Camelot.Api.Data;
using Camelot.Api.Exceptions;

using Microsoft.EntityFrameworkCore;

public class CollectionAccess
{
  private readonly AppDbContext context;
  public CollectionAccess(AppDbContext context) => this.context = context;

  public async Task<bool> GetAccess(int userId, int collectionId)
  {
    var userAccess = await this.context.User2Coll// is shared to user == everybody, who know about board
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collectionId)
        .FirstOrDefaultAsync() is not null;
    if (userAccess)
    {
      return true;
    }
    else
    {
      throw new UserAccessException($"User user_id={userId} has no access to GET collection {collectionId}");
    }
  }

  public async Task<bool> DeleteAccess(int userId, int collectionId)
  {
    var userAccess = await this.context.User2Coll// only owner can
     .Where(u => u.UserId == userId)
     .Where(u => u.CollectionId == collectionId)
     .Where(u => u.ShareMode == "owner")
     .FirstOrDefaultAsync() is not null;
    if (userAccess)
    {
      return true;
    }
    else
    {
      throw new UserAccessException($"User user_id={userId} has no access to DELETE collection {collectionId}");
    }
  }

  public async Task<bool> UpdateAccess(int userId, int collectionId)
  {
    var userAccess = await this.context.User2Coll// only owner can
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collectionId)
        .Where(u => u.ShareMode == "owner")
        .FirstOrDefaultAsync() is not null;
    if (userAccess)
    {
      return true;
    }
    else
    {
      throw new UserAccessException($"User user_id={userId} has no access to EDIT collection {collectionId}");
    }
  }

  public async Task<bool> AddDelBoardAccess(int userId, int collectionId)
  {
    var userAccess = await this.context.User2Coll
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collectionId)
        .Where(u => u.ShareMode == "moderator" || u.ShareMode == "owner")
        .FirstOrDefaultAsync() is not null;
    if (userAccess)
    { return true; }
    else
    { throw new UserAccessException($"User user_id={userId} has no access to Add or Delete boards to/from collection {collectionId}"); }
  }
}

