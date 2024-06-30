namespace Camelot.Api.Tests.Controllers;

using Camelot.Api.Controllers;
using Camelot.Api.Dao;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Models;
using Camelot.Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1707 // Identifiers should not contain underscores

[TestClass]
public class BoardControllerTest
{
  private readonly UserController userController;
  private readonly BoardService boardService;
  private readonly BoardController boardController;

  public BoardControllerTest()
  {
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");
    Environment.SetEnvironmentVariable("REGISTER_KEY", "");
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_board_db");
    var db = new AppDbContext(dbOptionsBuilder.Options);
    var logger = new LoggerFactory();

    var userService = new UserService(db, logger);
    this.userController = new UserController(userService, logger)
    {
      ControllerContext = new ControllerContext
      { HttpContext = new DefaultHttpContext() }

    };

    this.boardService = new BoardService(db, logger);
    this.boardController = new BoardController(this.boardService, logger)
    {
      ControllerContext = new ControllerContext
      { HttpContext = new DefaultHttpContext() }

    };
  }

  [TestMethod]
  public async Task CreateBoard_InputNoJwtHeaderNoBoard_ResultUnauthorized()
  {
    // arrange
    var boardDAO = new BoardDAO { };
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    this.boardController.ControllerContext.HttpContext.Request.Headers.Remove("X-Auth-Token");

    // act
    var response = await this.boardController.CreateBoard(boardDAO);
    // assert
    Assert.IsNotNull(response, "Expected UnauthorizedObjectResult");
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }

  [TestMethod]
  public async Task CreateBoard_InputWrongJwtHeader_ResultUnauthorized()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = "abracadabra";
    var boardDAO = new BoardDAO { };

    // act
    var response = await this.boardController.CreateBoard(boardDAO);
    // assert
    Assert.IsNotNull(response, "Expected UnauthorizedObjectResult");
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }
  [TestMethod]
  public async Task CreateBoard_InputBoardDAO_ResultBoard()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var boardDAO = new BoardDAO()
    { Name = "al", Picture = "pass", Elements = "elements", AppState = "str" };

    // act
    var boardCreateResponse = await this.boardController.CreateBoard(boardDAO) as JsonResult;
    Assert.IsNotNull(boardCreateResponse);
    var board = boardCreateResponse.Value as BoardDTO;
    Assert.IsNotNull(board);

    // assert
    Assert.IsNotNull(board);
    var expected_result = await this.boardController.GetBoardById(board.Id) as JsonResult;
    Assert.IsNotNull(expected_result);
    Assert.IsTrue(Equals(expected_result.Value as BoardDTO, board));
  }

  [TestMethod]
  public async Task GetBoardById_InputWrongJwtHeader_ResultUnauthorized()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = "abracadabra";
    // act
    var response = await this.boardController.GetBoardById(1);
    // assert
    Assert.IsNotNull(response, "Expected UnauthorizedObjectResult");
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }

  [TestMethod]
  public async Task GetBoardById_InputAnonymUserGetPrivateBoard_ResultBadRequest()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var tokenAl = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = tokenAl;
    var userId = this.userController.AboutMe().Id;

    var boardDAO = new BoardDAO()
    { Name = "al", Picture = "pass", Elements = "elements", AppState = "str" };
    var board = await this.boardService.CreateBoard(boardDAO, userId);
    Assert.IsNotNull(board);

    this.boardController.ControllerContext.HttpContext.Request.Headers.Remove("X-Auth-Token");
    // act
    var response = await this.boardController.GetBoardById(board.Id);
    Console.WriteLine($"{response}");
    // assert
    Assert.IsNotNull(response, response.ToString());
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
  }

  [TestMethod]
  public async Task GetBoardById_InputOwnerGetPrivateBoard_ResultBoard()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "alex", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "alex", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;
    var userId = this.userController.AboutMe().Id;

    var boardCreateResponse = await this.boardController.CreateBoard(new BoardDAO()
    { Name = "alead", Picture = "adfasd", Elements = "asdf", AppState = "str" }) as JsonResult;
    Assert.IsNotNull(boardCreateResponse);
    var board = boardCreateResponse.Value as BoardDTO;
    Assert.IsNotNull(board);
    var expected_result = await this.boardService.GetBoard(userId, board.Id);

    // act
    var taskResponse = this.boardController.GetBoardById(board.Id);
    var response = ((JsonResult)taskResponse.Result).Value as BoardDTO;
    // assert
    Assert.IsNotNull(expected_result);
    Assert.IsNotNull(response);
    Assert.IsTrue(Equals(expected_result, response));
  }

  [TestMethod]
  public async Task GetBoardById_InputNotAllowdUserGetBoard_ResultBadRequest()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var tokenAl = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = tokenAl;

    var boardCreateResponse = await this.boardController.CreateBoard(new BoardDAO()
    { Name = "al", Picture = "pass", Elements = "elements", AppState = "str" }) as JsonResult;
    Assert.IsNotNull(boardCreateResponse);
    var board = boardCreateResponse.Value as BoardDTO;
    Assert.IsNotNull(board);

    await this.userController.Register(new RegisterModel { Username = "vas", Password = "pass" });
    await this.userController.Login(new UserModel { Username = "vas", Password = "pass" });
    var tokenVas = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = tokenVas;
    Assert.AreNotEqual(tokenAl, tokenVas);
    // act
    var response = await this.boardController.GetBoardById(board.Id);
    // assert
    Assert.IsNotNull(response, response.ToString());
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult), response.ToString());
  }


  [TestMethod]
  public async Task DeleteBoard_InputOwnerDeleteBoard_ResultNoContent()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var boardCreateResponse = await this.boardController.CreateBoard(new BoardDAO()
    { Name = "al", Picture = "pass", Elements = "elements", AppState = "str" }) as JsonResult;
    Assert.IsNotNull(boardCreateResponse);
    var board = boardCreateResponse.Value as BoardDTO;
    Assert.IsNotNull(board);

    // act
    var response = await this.boardController.DeleteBoard(board.Id);

    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(NoContentResult));
  }

  [TestMethod]
  public async Task DeleteBoard_InputNotOwnerDeleteBoard_ResultBadRequest()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    await this.userController.Register(new RegisterModel { Username = "vas", Password = "pass" });

    //create board by AL
    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var boardCreateResponse = await this.boardController.CreateBoard(new BoardDAO()
    { Name = "al", Picture = "pass", Elements = "elements", AppState = "str" }) as JsonResult;
    Assert.IsNotNull(boardCreateResponse);
    var board = boardCreateResponse.Value as BoardDTO;
    Assert.IsNotNull(board);

    // Now LogIn as VAS
    await this.userController.Login(new UserModel { Username = "vas", Password = "pass" });
    token = this.userController.Response.Headers["X-Auth-Token"];
    this.boardController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;
    // act
    var response = await this.boardController.DeleteBoard(board.Id);

    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
  }



#pragma warning restore CA1707 // Identifiers should not contain underscores
  private static bool Equals(BoardDTO? fir, BoardDTO? sec) => fir is not null && sec is not null &&
                fir.Name == sec.Name &&
                fir.Id == sec.Id &&
                fir.CollectionId == sec.CollectionId &&
                fir.IsShared == sec.IsShared &&
                fir.Elements == sec.Elements &&
                fir.AuthorId == sec.AuthorId &&
                fir.Picture == sec.Picture &&
                fir.CreationDate == sec.CreationDate &&
                fir.AppState == sec.AppState;
}
#pragma warning restore CA1707 // Identifiers should not contain underscores
