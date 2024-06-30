namespace Camelot.Api.Repository;

using System.Linq;
using System.Threading.Tasks;

using Camelot.Api.Dao;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;

using Microsoft.EntityFrameworkCore;

public class BoardRepository
{
  private readonly AppDbContext context;

  public BoardRepository(AppDbContext context) => this.context = context;

  public async Task<int> CreateBoard(Board board)
  {
    this.context.Add(board);
    await this.context.SaveChangesAsync();
    return board.BoardId;
  }

  public async Task<BoardDTO> GetBoard(int boardId) => await this.context.Boards
            .Where(x => x.BoardId == boardId)
           // .Join(this.context.Boards, b => b.BoardId, c2b => c2b.BoardId,
           //     (c2b, b) => new { b.BoardId, b.Name, b.UserId, b.Picture, b.Elements, b.BoardCreationDate, b.AppState, c2b.CollectionId }
           // )
           .Select(x => new BoardDTO
           {
             Name = x.Name,
             Id = x.BoardId,
             CollectionId = this.context.Coll2Board
               .Where(b => b.BoardId == boardId)
               .Select(x => x.CollectionId).FirstOrDefault(),
             IsShared = this.context.Collections
               .Where(c => c.CollectionId == this.context.Coll2Board
                    .Where(b => b.BoardId == boardId)
                    .Select(x => x.CollectionId).FirstOrDefault())
               .Select(x => x.IsShared).FirstOrDefault(),
             Elements = x.Elements,
             AuthorId = x.UserId,
             Picture = x.Picture,
             CreationDate = x.BoardCreationDate,
             AppState = x.AppState
           }).FirstOrDefaultAsync();

  public async Task DeleteBoardsByCollection(int collectionId)
  {
    var boards = await this.context.Coll2Board
              .Where(x => x.CollectionId == collectionId)
              .Join(this.context.Boards, b => b.BoardId, c2b => c2b.BoardId,
                (c2b, b) => new { b.BoardId, b.Name, b.UserId, b.Picture, b.Elements, b.BoardCreationDate, b.AppState }
              ).Select(b => new Board
              {
                BoardId = b.BoardId,
                Name = b.Name,
                Elements = b.Elements,
                UserId = b.UserId,
                Picture = b.Picture,
                BoardCreationDate = b.BoardCreationDate,
                AppState = b.AppState
              })
              .ToListAsync();
    this.context.Boards.RemoveRange(boards);
  }

  public async Task DeleteBoard(int boardId)
  {
    var board = await this.context.Boards
        .Where(b => b.BoardId == boardId)
        .SingleOrDefaultAsync();
    this.context.Boards.Remove(board);
    await this.context.SaveChangesAsync();
  }

  public async Task UpdateBoard(int boardId, BoardDAO boardDAO)
  {
    var board = await this.context.Boards
              .Where(b => b.BoardId == boardId)
              .SingleOrDefaultAsync() ?? throw new ObjectNotFoundException($"Unable to find the board by id={boardId}");

    //update board
    board.Name = boardDAO.Name;
    board.Elements = boardDAO.Elements;
    board.AppState = boardDAO.AppState;
    board.Picture = boardDAO.Picture;

    await this.context.SaveChangesAsync();
  }

}
