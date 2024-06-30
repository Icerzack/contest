namespace Camelot.Api.Repository;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Camelot.Api.Data;
using Camelot.Api.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
public class User2CollRepository
{
  private readonly AppDbContext context;
  private readonly ILogger logger;
  private static readonly Action<ILogger, string, Exception> LoggerINFO =
      LoggerMessage.Define<string>(LogLevel.Information, eventId:
      new EventId(id: 0, name: "INFO"), formatString: "{Message}");

  public User2CollRepository(AppDbContext context, ILoggerFactory factory)
  {
    this.context = context;
    this.logger = factory.CreateLogger<User2CollRepository>();
  }

  public async Task CreateSharing(int userId, int collectionId, string shareMode)
  {
    this.context.User2Coll.Add(new User2Collection
    {
      UserId = userId,
      CollectionId = collectionId,
      ShareMode = shareMode
    });
    await this.context.SaveChangesAsync();
  }

  public async Task CloseSharing(int collectionId)
  {
    var relation = await this.context.User2Coll
              .Where(r => r.CollectionId == collectionId)
              .Where(r => r.ShareMode != "onwer")
              .ToListAsync();
    this.context.User2Coll.RemoveRange(relation);// is not async
    await this.context.SaveChangesAsync();
  }

  public async Task DeleteRelation(int userId, int collectionId, string shareMode)
  {
    var relation = await this.context.User2Coll
        .Where(u => u.UserId == userId)
        .Where(u => u.CollectionId == collectionId)
        .Where(u => u.ShareMode == shareMode)
        .FirstOrDefaultAsync()
     ?? throw new ObjectNotFoundException($" Nothing to delete: No relation between user {userId} and collection {collectionId}");

    this.context.User2Coll.Remove(relation);
    await this.context.SaveChangesAsync();
  }

  public async Task<User2Collection> GetRelation(int userId, int collectionId) => await this.context.User2Coll
          .Where(u => u.UserId == userId)
          .Where(u => u.CollectionId == collectionId)
          .FirstOrDefaultAsync();
  public async Task DeleteCollectionsRelation(int collectionId)
  {
    var relations = await this.context.User2Coll
          .Where(x => x.CollectionId == collectionId)
          .ToListAsync();
    this.context.User2Coll.RemoveRange(relations);

    await this.context.SaveChangesAsync();
  }
  public async Task UpdateSharingMode(int collectionId, int passiveUser, string shareMode)
  {
    var relation = await this.context.User2Coll
      .Where(u => u.CollectionId == collectionId)
      .Where(u => u.UserId == passiveUser)
      .FirstOrDefaultAsync();
    LoggerINFO(this.logger, $"previous share mode was {relation.ShareMode}, new is:{shareMode}", default!);
    relation.ShareMode = shareMode;
    await this.context.SaveChangesAsync();
  }

  internal async Task<List<string>> GetRoleRelations(int collectionId, string role) => await this.context.User2Coll
          .Where(u => u.CollectionId == collectionId)
          .Where(u => u.ShareMode == role)
          .Select(u => this.context.Users
              .Where(x => x.UserId == u.UserId)
              .Select(x => x.Username)
              .FirstOrDefault())
          .ToListAsync();
}
