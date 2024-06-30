namespace Camelot.Api.Service;

using System.Threading.Tasks;

using Camelot.Api.Dao;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;
using Microsoft.Extensions.Logging;

public class BoardService : BaseService
{
  public BoardService(AppDbContext context, ILoggerFactory factory) : base(context, factory)
  { }

  public async Task<BoardDTO> CreateBoard(BoardDAO input, int authorId)
  {
    if (authorId == 0)
    { throw new UnauthorizedException(); }

    var board = input.ToBoard(authorId);
    var id = await this.boardRepo.CreateBoard(board);
    return await this.boardRepo.GetBoard(id);
  }

  public async Task<BoardDTO> GetBoard(int userId, int boardId)
  {
    await this.boardAccess.GetAccess(userId, boardId);
    return await this.boardRepo.GetBoard(boardId);
  }

  public async Task DeleteBoard(int userId, int boardId)
  {
    await this.boardAccess.DeleteAccess(userId, boardId);
    await this.boardRepo.DeleteBoard(boardId);
    await this.coll2boardRepo.DeleteBoardsRelation(boardId);
  }

  public async Task<BoardDTO> UpdateBoard(int boardId, BoardDAO boardDAO, int userId)
  {
    await this.boardAccess.UpdateAccess(userId, boardId);
    await this.boardRepo.UpdateBoard(boardId, boardDAO);
    return await this.boardRepo.GetBoard(boardId); // get updated
  }
}
