namespace Camelot.Api.Tests.Controllers;

using System.Threading.Tasks;
using Camelot.Api.Controllers;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Models;
using Camelot.Api.Service;
using Camelot.Api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1707 // Identifiers should not contain underscores

[TestClass]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class UserControllerTest
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
  private readonly LoggerFactory logger = new();
  private readonly AppDbContext db;
  private readonly UserService userService;
  private readonly UserController userController;
  public UserControllerTest()
  {
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");
    Environment.SetEnvironmentVariable("REGISTER_KEY", "");

    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_user_db");
    this.db = new AppDbContext(dbOptionsBuilder.Options);

    this.userService = new UserService(this.db, this.logger);
    this.userController = new UserController(this.userService, this.logger)
    {
      ControllerContext = new ControllerContext
      { HttpContext = new DefaultHttpContext() }

    };
  }

  [TestMethod]
  public async Task Login_InputNouserModel_ResultUnauthorized()
  {
    // arrange

    var userModel = new UserModel { };

    // act
    var response = await this.userController.Login(userModel);
    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }

  [TestMethod]
  public async Task Login_InputWronguserModel_ResultUnauthorized()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    var userModel = new UserModel { Username = "wasd", Password = "ssap" };

    // act
    var response = await this.userController.Login(userModel);
    // assert
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }
  [TestMethod]
  public async Task Login_InputuserModel_ResultUserIdFromJWT()
  {
    // arrange
    var registerModel = new RegisterModel { Username = "ale", Password = "passe" };
    await this.userController.Register(registerModel);
    var userModel = new UserModel { Username = "ale", Password = "passe" };

    // act
    await this.userController.Login(userModel);
    var responseToken = this.userController.Response.Headers["X-Auth-Token"];
    this.userController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = responseToken;
    var userId = await Token.GetUserId(this.userController.Request, this.userService);
    var expectedUserId = await this.db.Users
        .Where(u => u.Username == "ale")
        .Select(u => u.UserId).FirstAsync();
    // assert
    Assert.IsNotNull(responseToken);
    Assert.IsNotNull(userId);
    Assert.IsNotNull(expectedUserId);
    Assert.AreEqual(expectedUserId, userId);
  }

  [TestMethod]
  public async Task Register_InputRegisterModel_ResultOk()
  {
    // arrange
    var registerModel = new RegisterModel { Username = "al2", Password = "pass2" };

    // act
    var response = await this.userController.Register(registerModel);
    var user = this.db.Users.Where(u => u.Username == registerModel.Username).FirstOrDefault();
    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(OkResult));
    Assert.IsNotNull(user);
  }

  [TestMethod]
  public async Task Register_InputAlredyRegistedUser_ResultBadRequestAsync()
  {
    // arrange
    var registerModel = new RegisterModel { Username = "al", Password = "pass" };
    await this.userController.Register(registerModel);

    // act
    var taskResponse = await this.userController.Register(registerModel);
    var response = taskResponse as BadRequestObjectResult;
    // assert
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
  }

  [TestMethod]
  public async Task AboutMe_InputOwner_ResultOwnerDTO()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });

    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.userController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    // act
    var response = await this.userController.AboutMe() as JsonResult;
    Assert.IsNotNull(response);
    var owner = response.Value as OwnerDTO;
    // assert
    Assert.IsNotNull(response, response.ToString());
    Assert.IsInstanceOfType(owner, typeof(OwnerDTO));
  }

  [TestMethod]
  public async Task AboutId_InputAnonym_ResultUnauthorized()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    // act
    var response = await this.userController.About(1);
    // assert
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }
  [TestMethod]
  public async Task AboutId_SingInUser_ResultUserDTO()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "ali", Password = "pass" });
    await this.userController.Login(new UserModel { Username = "ali", Password = "pass" });

    var jwtoken = this.userController.Response.Headers["X-Auth-Token"];
    this.userController.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = jwtoken;
    var userId = this.userController.AboutMe().Id;
    // act
    var taskResponse = await this.userController.About(userId);
    var response = ((JsonResult)taskResponse).Value as UserDTO;
    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(UserDTO));
    Assert.AreEqual(userId, response.Id);
  }

#pragma warning restore CA1707 // Identifiers should not contain underscores
}
