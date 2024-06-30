namespace Camelot.Api.Controllers;

using System;
using System.Threading.Tasks;

using Camelot.Api.Dao;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;
using Camelot.Api.Service;
using Camelot.Api.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/v1/boards")]
public class BoardController : ControllerBase
{
  private readonly BoardService service;
  private readonly ILogger logger;
  private static readonly Action<ILogger, string, Exception> LoggerError =
    LoggerMessage.Define<string>(LogLevel.Error, eventId:
    new EventId(id: 0, name: "ERROR"), formatString: "{Message}");

  public BoardController(BoardService service, ILoggerFactory factory) : base()
  {
    this.service = service;
    this.logger = factory.CreateLogger<BoardController>();
  }

  [HttpPost("")]
  public async Task<IActionResult> CreateBoard(BoardDAO boardDAO)
  {
    try
    {
      var board = await this.service.CreateBoard(boardDAO, await Token.GetUserId(this.Request, this.service));
      return new JsonResult(board);
    }
    catch (UnauthorizedException e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", default!);
      return this.Unauthorized(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
    catch (Exception e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", e);
      return this.BadRequest(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
  }

  [HttpDelete("{boardId}")]
  public async Task<IActionResult> DeleteBoard(int boardId)
  {
    try
    {
      var userId = await Token.GetUserId(this.Request, this.service);
      await this.service.DeleteBoard(userId, boardId);
      return this.NoContent();
    }
    catch (UnauthorizedException e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", default!);
      return this.Unauthorized(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
    catch (Exception e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", e);
      return this.BadRequest(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
  }

  [HttpGet("{boardId}")]
  public async Task<IActionResult> GetBoardById(int boardId)
  {
    try
    {
      var board = await this.service.GetBoard(await Token.GetUserId(this.Request, this.service), boardId);
      if (board is null)
      {
        return this.NotFound();
      }

      return new JsonResult(board);
    }
    catch (UnauthorizedException e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", default!);
      return this.Unauthorized(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
    catch (Exception e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", e);
      return this.BadRequest(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
  }

  [HttpPut("{boardId}")]
  public async Task<IActionResult> UpdateBoard(int boardId, BoardDAO cbc)
  {
    try
    {
      var board = await this.service.UpdateBoard(boardId, cbc, await Token.GetUserId(this.Request, this.service));
      return new JsonResult(board);
    }
    catch (UnauthorizedException e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", default!);
      return this.Unauthorized(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
    catch (Exception e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", e);
      return this.BadRequest(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
  }

}
