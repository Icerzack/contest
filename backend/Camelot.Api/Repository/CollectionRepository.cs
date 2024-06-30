namespace Camelot.Api.Repository;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Camelot.Api.Dao;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;

using Microsoft.EntityFrameworkCore;

public class CollectionRepository
{
  private readonly AppDbContext context;
  public CollectionRepository(AppDbContext context) => this.context = context;

  public async Task<int> CreateCollection(Collection collection)
  {
    this.context.Add(collection);
    await this.context.SaveChangesAsync();
    return collection.CollectionId;
  }

  public async Task UpdateIsSharedField(int collectionId)
  {
    var collection = await this.context.Collections.FindAsync(collectionId);
    var relations = await this.context.User2Coll
        .Where(u => u.CollectionId == collectionId)
        .Where(u => u.UserId != collection.UserId).ToListAsync();
    collection.IsShared = relations.Count != 0;
    await this.context.SaveChangesAsync();
  }

  public async Task<CollectionDTO> GetCollection(int userId, int collectionId)
  => await this.context.User2Coll
        .Where(c => c.CollectionId == collectionId)
        .Join(this.context.Collections, c => c.CollectionId, u2c => u2c.CollectionId,
            (u2c, c) => new
            {
              c.CollectionId,
              c.Name,
              c.UserId,
              c.CollectionCreationDate,
              c.IsShared
            }
        )
        .Select(c => new CollectionDTO
        {
          Id = c.CollectionId,
          Name = c.Name,
          AuthorId = c.UserId,
          Role = this.context.User2Coll
              .Where(x => x.UserId == userId)
              .Where(x => x.CollectionId == c.CollectionId)
              .Select(x => x.ShareMode)
              .FirstOrDefault(),
          CreationDate = c.CollectionCreationDate,
          IsShared = c.IsShared,
          Boards = this.context.Coll2Board
              .Where(x => c.CollectionId == x.CollectionId)
              .Join(this.context.Boards, b => b.BoardId, c2b => c2b.BoardId,
                  (c2b, b) => new { b.BoardId, b.Name, b.UserId, b.Picture, b.Elements, b.BoardCreationDate, b.AppState, c2b.CollectionId }
              ).Select(x => new BoardDTO
              {
                Name = x.Name,
                Id = x.BoardId,
                CollectionId = x.CollectionId,
                IsShared = c.IsShared,
                // Elements = x.Elements,
                AuthorId = x.UserId,
                Picture = x.Picture,
                CreationDate = x.BoardCreationDate,
                AppState = x.AppState
              }).ToList()
        }).FirstOrDefaultAsync();
  public async Task<CollectionsDTO> GetAllCollections(int userId)
   => new CollectionsDTO
   {
     Collections = await this.context.User2Coll
        .Where(c => c.UserId == userId)
        .Join(this.context.Collections, c => c.CollectionId, u2c => u2c.CollectionId,
            (u2c, c) => new
            { c.CollectionId, c.Name, c.UserId, c.CollectionCreationDate, c.IsShared, u2c.ShareMode }
        )
        .Select(c => new CollectionDTO
        {
          Id = c.CollectionId,
          Name = c.Name,
          AuthorId = c.UserId,
          Role = c.ShareMode,
          CreationDate = c.CollectionCreationDate,
          IsShared = c.IsShared,
          Boards = this.context.Coll2Board
              .Where(x => c.CollectionId == x.CollectionId)
              .Join(this.context.Boards, b => b.BoardId, c2b => c2b.BoardId,
                  (c2b, b) => new { b.BoardId, b.Name, b.UserId, b.Picture, b.Elements, b.BoardCreationDate, b.AppState, c2b.CollectionId }
              ).Select(x => new BoardDTO
              {
                Name = x.Name,
                Id = x.BoardId,
                CollectionId = x.CollectionId,
                IsShared = c.IsShared,
                // Elements = x.Elements,
                AuthorId = x.UserId,
                Picture = x.Picture,
                CreationDate = x.BoardCreationDate,
                AppState = x.AppState
              }).ToList()
        }).ToListAsync()
   };

  public async Task UpdateCollection(int collectionId, CollectionDAO collectionDAO)
  {
    var collection = await this.context.Collections
      .Where(b => b.CollectionId == collectionId)
      .SingleOrDefaultAsync() ?? throw new ObjectNotFoundException($"Unable to find the collection by id={collectionId}");

    //update collection
    collection.Name = collectionDAO.Name;
    await this.context.SaveChangesAsync();
  }

  public async Task DeleteCollection(int collectionId)
  {
    var collection = await this.context.Collections
        .Where(c => c.CollectionId == collectionId)
        .SingleOrDefaultAsync();
    this.context.Collections.Remove(collection);// remove collection
    await this.context.SaveChangesAsync();
  }

  public async Task<bool> IsShared(int collectionId) => await this.context.User2Coll
        .Where(c => c.CollectionId == collectionId)
        .Join(this.context.Collections, c => c.CollectionId, u2c => u2c.CollectionId,
            (u2c, c) => new
            {
              c.CollectionId,
              c.Name,
              c.UserId,
              c.CollectionCreationDate,
              c.IsShared
            }
        )
        .Select(c => c.IsShared).FirstOrDefaultAsync();
  internal async Task<bool> CollectionExist(int collectionId) => await this.context.Collections
        .Where(c => c.CollectionId == collectionId)
        .FirstAsync() is not null;
}

public class CollectionsDTO
{
  public List<CollectionDTO> Collections { get; set; }
}
