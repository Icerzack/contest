namespace Camelot.Api.Service;

using System;
using System.Threading.Tasks;

using Camelot.Api.Dao;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;
using Camelot.Api.Repository;

using Microsoft.Extensions.Logging;

public class CollectionService : BaseService
{
  private readonly ILogger logger;
  private static readonly Action<ILogger, string, Exception> LoggerWARN =
        LoggerMessage.Define<string>(LogLevel.Information, eventId:
        new EventId(id: 0, name: "WARN"), formatString: "{Message}");

  public CollectionService(AppDbContext context, ILoggerFactory factory)
  : base(context, factory) => this.logger = factory.CreateLogger<CollectionService>();

  public async Task<CollectionDTO> CreateCollection(CollectionDAO collectionDAO, int authorId)
  {
    if (authorId == 0)
    {
      throw new UnauthorizedException();
    }
    var collectionId = await this.collRepo.CreateCollection(collectionDAO.ToCollection(authorId));
    await this.user2collRepo.CreateSharing(authorId, collectionId, "owner");
    return await this.collRepo.GetCollection(authorId, collectionId);
  }

  public async Task AddBoard2Collection(int userId, int collectionId, int boardId)
  {
    if (!await this.collRepo.CollectionExist(collectionId))
    { return; }
    await this.collectionAccess.AddDelBoardAccess(userId, collectionId);
    await this.boardAccess.UpdateAccess(userId, boardId);
    await this.coll2boardRepo.AddRelation(collectionId, boardId);
  }

  public async Task<CollectionDTO> GetCollection(int userId, int collectionId)
  {
    await this.collectionAccess.GetAccess(userId, collectionId);
    return await this.collRepo.GetCollection(userId, collectionId);
  }
  public async Task<CollectionsDTO> GetAllCollections(int userId) => await this.collRepo.GetAllCollections(userId);

  public async Task<CollectionDTO> UpdateCollection(int collectionId, CollectionDAO collectionDAO, int userId)
  {
    await this.collectionAccess.UpdateAccess(userId, collectionId);
    await this.collRepo.UpdateCollection(collectionId, collectionDAO);
    return await this.collRepo.GetCollection(userId, collectionId);
  }


  public async Task DeleteBoardFromCollection(int userId, int collectionId, int boardId)
  {
    if (!await this.collRepo.CollectionExist(collectionId))
    { return; }
    await this.collectionAccess.AddDelBoardAccess(userId, collectionId);
    await this.boardAccess.DeleteAccess(userId, boardId);

    await this.coll2boardRepo.DeleteBoardsRelation(boardId);
  }

  public async Task DeleteCollection(int userId, int collectionId)
  {
    await this.collectionAccess.DeleteAccess(userId, collectionId);
    // await _boardRepo.DeleteBoardsByCollection(collection_id);//remove boards
    await this.coll2boardRepo.DeleteCollectionsRelation(collectionId);// remove relations
    await this.user2collRepo.DeleteCollectionsRelation(collectionId);
    await this.collRepo.DeleteCollection(collectionId);// remove collection
  }

  public async Task DeleteSharing(int userId, int collectionId, ShareDao shareDao)
  {
    if (!await this.collRepo.CollectionExist(collectionId))
    { return; }
    //check all access firts
    foreach (var passiveUser in shareDao.ToPairList())
    {
      var passiveUserId = await this.userRepo.GetUserIdByUsername(passiveUser.Item1);
      if (passiveUserId == 0 && passiveUser.Item2 != "anonym")
      { throw new ObjectNotFoundException($"User '{passiveUser.Item1}' does not exist"); }
      var previousSharemode = await this.user2collRepo.GetRelation(passiveUserId, collectionId);
      if (previousSharemode != null && previousSharemode.ShareMode != "owner")
      {
        await this.shareAccess.DeleteAccess(userId, collectionId);
        await this.user2collRepo.DeleteRelation(passiveUserId, collectionId, passiveUser.Item2);
      }
    }
    await this.collRepo.UpdateIsSharedField(collectionId);
  }

  public async Task UpdateSharingMode(int userId, int collectionId, ShareDao shareDao)
  {
    if (!await this.collRepo.CollectionExist(collectionId))
    { return; }
    foreach (var passiveUser in shareDao.ToPairList())
    {

      var passiveUserId = await this.userRepo.GetUserIdByUsername(passiveUser.Item1);
      if (passiveUserId == 0 && passiveUser.Item2 != "anonym")
      { throw new ObjectNotFoundException($"User '{passiveUser.Item1}' does not exist"); }
      var previousSharemode = await this.user2collRepo.GetRelation(passiveUserId, collectionId);

      if (previousSharemode is null)
      {
        LoggerWARN(this.logger, $"There is nothing to change, relation for user {passiveUserId} will be created", default!);
        await this.shareAccess.CreateAccess(userId, collectionId, passiveUser.Item2);
        await this.user2collRepo.CreateSharing(passiveUserId, collectionId, passiveUser.Item2);
      }
      else if (previousSharemode.ShareMode != "owner" &&
            previousSharemode.ShareMode != passiveUser.Item2)
      {
        await this.shareAccess.UpdateAccess(userId, collectionId, passiveUserId);
        await this.user2collRepo.UpdateSharingMode(collectionId, passiveUserId, passiveUser.Item2);
      }

    }
    if (shareDao.Anonym)
    {
      LoggerWARN(this.logger, $"Turn on anonym-mode for collection {collectionId}", default!);
      if (await this.user2collRepo.GetRelation(0, collectionId) is null)
      { await this.user2collRepo.CreateSharing(0, collectionId, "anonym"); }
    }
    else
    {
      LoggerWARN(this.logger, $"Turn off anonym-mode for collection {collectionId}", default!);
      if (await this.user2collRepo.GetRelation(0, collectionId) is not null)
      { await this.user2collRepo.DeleteRelation(0, collectionId, "anonym"); }
    }
    await this.collRepo.UpdateIsSharedField(collectionId);
  }

  public async Task<ShareDTO> GetSharingMode(int userId, int collectionId)
  {
    await this.shareAccess.GetAccess(userId, collectionId);
    return new ShareDTO
    {
      Reader = await this.user2collRepo.GetRoleRelations(collectionId, "reader"),
      Moderator = await this.user2collRepo.GetRoleRelations(collectionId, "moderator"),
      Editor = await this.user2collRepo.GetRoleRelations(collectionId, "editor"),
      Anonym = (await this.user2collRepo.GetRoleRelations(collectionId, "anonym")).Count > 0
    };

  }
}
