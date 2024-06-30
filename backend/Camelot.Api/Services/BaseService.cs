namespace Camelot.Api.Service;

using System.Threading.Tasks;
using Camelot.Api.Data;
using Camelot.Api.Repository;
using Camelot.Api.Utils;
using Microsoft.Extensions.Logging;

public class BaseService
{
#pragma warning disable IDE1006 // Naming Styles
  internal readonly BoardRepository boardRepo;
  internal readonly CollectionRepository collRepo;
  internal readonly UserRepository userRepo;

  internal readonly Coll2BoardRepository coll2boardRepo;
  internal readonly User2CollRepository user2collRepo;

  internal readonly BoardAccess boardAccess;
  internal readonly ShareAccess shareAccess;
  internal readonly CollectionAccess collectionAccess;
#pragma warning restore IDE1006 // Naming Styles

  public BaseService(AppDbContext context, ILoggerFactory factory)
  {
    this.boardRepo = new BoardRepository(context);
    this.collRepo = new CollectionRepository(context);
    this.userRepo = new UserRepository(context);

    this.coll2boardRepo = new Coll2BoardRepository(context, factory);
    this.user2collRepo = new User2CollRepository(context, factory);

    this.boardAccess = new BoardAccess(context);
    this.collectionAccess = new CollectionAccess(context);
    this.shareAccess = new ShareAccess(context);
  }
  internal async Task<bool> DoesUserExist(int userId) => await this.userRepo.DoesUserIdExist(userId);

}
