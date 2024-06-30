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
[Route("api/v1/collections")]
public class CollectionController : ControllerBase
{

  private readonly CollectionService service;
  private readonly ILogger logger;
  private static readonly Action<ILogger, string, Exception> LoggerError =
    LoggerMessage.Define<string>(LogLevel.Error, eventId:
    new EventId(id: 0, name: "ERROR"), formatString: "{Message}");
  public CollectionController(CollectionService service, ILoggerFactory factory)
  {
    this.service = service;
    this.logger = factory.CreateLogger<CollectionController>();
  }

  [HttpGet("all")]
  public async Task<IActionResult> GetCollectionsByUserId()
  {
    try
    {
      var collections = await this.service.GetAllCollections(await Token.GetUserId(this.Request, this.service));
      if (collections is null)
      { return this.NotFound(); }

      return new JsonResult(collections);
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


  [HttpGet("{collectionId}")]
  public async Task<IActionResult> GetCollectionById(int collectionId)
  {
    try
    {
      var collection = await this.service.GetCollection(await Token.GetUserId(this.Request, this.service), collectionId);
      if (collection is null)
      { return this.NotFound(); }

      return new JsonResult(collection);
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


  [HttpDelete("{collectionId}")]
  public async Task<IActionResult> DeleteCollection(int collectionId)
  {
    try
    {
      await this.service.DeleteCollection(await Token.GetUserId(this.Request, this.service), collectionId);
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
  [HttpPut("{collectionId}")]
  public async Task<IActionResult> UpdateCollection(int collectionId, CollectionDAO collectionDAO)
  {
    try
    {
      var collection = await this.service.UpdateCollection(collectionId, collectionDAO, await Token.GetUserId(this.Request, this.service));
      return new JsonResult(collection);
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

  [HttpPost("")]
  public async Task<IActionResult> CreateCollection(CollectionDAO createCollection)
  {
    try
    {
      var collection = await this.service.CreateCollection(createCollection, await Token.GetUserId(this.Request, this.service));
      return new JsonResult(collection);
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

  [HttpPut("{collectionId}/{boardId}")]
  public async Task<IActionResult> AddBaord2Collection(int collectionId, int boardId)
  {
    try
    {
      await this.service.AddBoard2Collection(await Token.GetUserId(this.Request, this.service), collectionId, boardId);
      return this.Ok();
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

  [HttpDelete("{collectionId}/{boardId}")]
  public async Task<IActionResult> DeleteBaordFromCollection(int collectionId, int boardId)
  {
    try
    {
      await this.service.DeleteBoardFromCollection(await Token.GetUserId(this.Request, this.service), collectionId, boardId);
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

  //////////////////

  [HttpDelete("share/{collectionId}")]
  public async Task<IActionResult> DeleteSharing(int collectionId, ShareDao shareDao)
  {
    try
    {
      await this.service.DeleteSharing(await Token.GetUserId(this.Request, this.service), collectionId, shareDao);
      return this.Ok();
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

  [HttpPut("share/{collectionId}")]
  public async Task<IActionResult> ChangeSharingMode(int collectionId, ShareDao cs)
  {
    try
    {
      await this.service.UpdateSharingMode(await Token.GetUserId(this.Request, this.service), collectionId, cs);
      return this.Ok();
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

  [HttpGet("share/{collectionId}")]
  public async Task<IActionResult> GetSharingMode(int collectionId)
  {
    try
    {
      return new JsonResult(await this.service.GetSharingMode(await Token.GetUserId(this.Request, this.service), collectionId));
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
