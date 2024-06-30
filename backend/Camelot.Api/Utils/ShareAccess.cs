namespace Camelot.Api.Utils;

using System.Linq;
using System.Threading.Tasks;

using Camelot.Api.Data;
using Camelot.Api.Exceptions;

using Microsoft.EntityFrameworkCore;

public class ShareAccess
{
  private readonly AppDbContext context;
  public ShareAccess(AppDbContext context) => this.context = context;

  public async Task<bool> CreateAccess(int userId, int collectionId, string shareMode)
  {
    var access = await this.context.User2Coll
           .Where(u => u.UserId == userId)
           .Where(u => u.CollectionId == collectionId)
           .Where(u => u.ShareMode == "owner" || u.ShareMode == "moderator")
           .FirstOrDefaultAsync();
    var resutl = access is not null;
    if (resutl)
    {
      resutl = (access.ShareMode == "owner") ||
       ((access.ShareMode is "moderator") && (shareMode is "editor" or "reader"));
    }
    if (resutl)
    { return true; }
    throw new UserAccessException($"User has no access to collection {collectionId} or is not able to share");
  }

  public async Task<bool> GetAccess(int userId, int collectionId)
  {
    var access = await this.context.User2Coll
           .Where(u => u.UserId == userId)
           .Where(u => u.CollectionId == collectionId)
           .Where(u => u.ShareMode == "owner" || u.ShareMode == "moderator")
           .FirstOrDefaultAsync();
    if (access is not null)
    { return true; }
    throw new UserAccessException($"User has no access to collection {collectionId} or is not able to share");
  }

  public async Task<bool> DeleteAccess(int userId, int collectionId)
  {
    var access = await this.context.User2Coll
     .Where(u => u.UserId == userId)
     .Where(u => u.CollectionId == collectionId)
     .Where(u => u.ShareMode == "owner")
     .FirstOrDefaultAsync();

    if (access is not null && (access.ShareMode == "owner"))
    { return true; }
    throw new UserAccessException($"User {userId} has no access to DELETE sharing for the user or collection");

  }

  public async Task<bool> UpdateAccess(int userId, int collectionId, int passiveUser)
  {
    var access = await this.context.User2Coll
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collectionId)
        .Where(u => u.ShareMode == "owner" || u.ShareMode == "moderator")
        .FirstOrDefaultAsync();
    var sharedUserAccess = await this.context.User2Coll
      .Where(u => u.UserId == passiveUser)
      .Where(u => u.CollectionId == collectionId)
      .FirstOrDefaultAsync();
    var resutl = access is not null && sharedUserAccess is not null;
    if (resutl)
    {
      resutl = (access.ShareMode == "owner") ||
       ((access.ShareMode is "moderator") && (sharedUserAccess.ShareMode is "editor" or "reader"));
    }
    if (resutl)
    { return true; }

    throw new UserAccessException($"User {userId} has no access to EDIT sharing mode for the user or collection ");

  }

}
