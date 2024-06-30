namespace Camelot.Api.Controllers;

using System;
using System.Threading.Tasks;

using Camelot.Api.Dto;
using Camelot.Api.Exceptions;
using Camelot.Api.Models;
using Camelot.Api.Service;
using Camelot.Api.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[Route("api/v1/users")]
[ApiController]
public class UserController : ControllerBase
{
  private readonly UserService service;
  private readonly ILogger logger;
  private static readonly Action<ILogger, string, Exception> LoggerError =
    LoggerMessage.Define<string>(LogLevel.Error, eventId:
    new EventId(id: 0, name: "ERROR"), formatString: "{Message}");
  public UserController(UserService userService, ILoggerFactory factory)
  {
    this.service = userService;
    this.logger = factory.CreateLogger<UserController>();
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login(UserModel model)
  {
    try
    {
      var user_id = await this.service.CheckUser(model.Username, model.Password);
      var tokenString = Token.GenerateToken(user_id.ToString(System.Globalization.CultureInfo.CurrentCulture));
      if (this.Response.Headers.ContainsKey("X-Auth-Token"))
      { this.Response.Headers.Remove("X-Auth-Token"); }
      this.Response.Headers.Add("X-Auth-Token", tokenString);
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

  [HttpPut("")]
  public async Task<IActionResult> UpdateUser(UserModel model)
  {
    try
    {
      var user_id = await Token.GetUserId(this.Request, this.service);

      if (user_id == 0)
      { return this.Unauthorized(new ResponseDTO { Message = $"the is no token" }); }
      await this.service.UpdateUser(user_id, model);
      return await this.About(user_id);
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

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterModel user)
  {
    try
    {
      if (await this.service.DoesUsernameExist(user.Username))
      { return this.BadRequest(new ResponseDTO { Message = "User already exists!" }); }

      if (Token.CheckRegisterKey(this.Request))
      {
        await this.service.CreateUser(user);
        return this.Ok();
      }
      else
      { return this.BadRequest(new ResponseDTO { Message = "Wrong register key" }); }
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

  [HttpGet("about/id/{id}")]
  public async Task<IActionResult> About(int id)
  {
    try
    {
      if (await Token.GetUserId(this.Request, this.service) != 0)
      {
        var user = await this.service.About(id);
        return new JsonResult(user);
      }
      else
      { return this.Unauthorized(new ResponseDTO { Message = $"the is no token" }); }
    }
    catch (Exception e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", e);
      return this.BadRequest(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
  }


  [HttpGet("about/name/{name}")]
  public async Task<IActionResult> About(string name)
  {
    try
    {
      if (await Token.GetUserId(this.Request, this.service) != 0)
      {
        var user = await this.service.About(name);
        return new JsonResult(user);
      }
      else
      { return this.Unauthorized(new ResponseDTO { Message = $"the is no token" }); }
    }
    catch (Exception e)
    {
      LoggerError(this.logger, $"{e.GetType()}: {e.Message}", e);
      return this.BadRequest(new ResponseDTO { Message = $"{e.GetType()}: {e.Message}" });
    }
  }

  [HttpGet("about/me")]
  public async Task<IActionResult> AboutMe()
  {
    try
    {
      var userId = await Token.GetUserId(this.Request, this.service);
      return new JsonResult(await this.service.AboutMe(userId));
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

