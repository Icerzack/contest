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
[Route("api/v2/boards")]
public class BoardControllerV2 : ControllerBase
{
  private readonly BoardService service;
  private readonly ILogger logger;
  private static readonly Action<ILogger, string, Exception> LoggerError =
    LoggerMessage.Define<string>(LogLevel.Error, eventId:
    new EventId(id: 0, name: "ERROR"), formatString: "{Message}");

  public BoardControllerV2(BoardService service, ILoggerFactory factory)
  {
    this.service = service;
    this.logger = factory.CreateLogger<BoardController>();

  }


  [HttpPut("{boardId}")]
  public async Task<IActionResult> UpdateBoard(int boardId, BoardDAO cbc)
  {
    try
    {
      Token.CheckSocketToken(this.Request);
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
