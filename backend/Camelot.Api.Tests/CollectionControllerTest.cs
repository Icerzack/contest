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
public class CollectionControllerTest
{
  private readonly UserController userController;
  private readonly CollectionService collectionService;
  private readonly CollectionController controller;

  public CollectionControllerTest()
  {
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");
    Environment.SetEnvironmentVariable("REGISTER_KEY", "");
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_collection_db");
    var db = new AppDbContext(dbOptionsBuilder.Options);
    var logger = new LoggerFactory();

    var userService = new UserService(db, logger);
    this.userController = new UserController(userService, logger)
    {
      ControllerContext = new ControllerContext
      { HttpContext = new DefaultHttpContext() }

    };

    this.collectionService = new CollectionService(db, logger);
    this.controller = new CollectionController(this.collectionService, logger)
    {
      ControllerContext = new ControllerContext
      { HttpContext = new DefaultHttpContext() }

    };
  }


  [TestMethod]
  public async Task GetCollection_InputNoJwtHeaderNoCollection_ResultUnauthorized()
  {
    // arrange

    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    this.controller.ControllerContext.HttpContext.Request.Headers.Remove("X-Auth-Token");

    var collectionDAO = new CollectionDAO { };

    // act
    var response = await this.controller.CreateCollection(collectionDAO);

    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }

  [TestMethod]
  public async Task CreateCollection_InputWrongJwtHeader_ResultUnauthorized()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = "abracadabra";
    var collectionDAO = new CollectionDAO { };

    // act
    var response = await this.controller.CreateCollection(collectionDAO);
    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }
  [TestMethod]
  public async Task CreateCollection_InputCollectionDAO_ResultCollection()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var collectionDAO = new CollectionDAO() { Name = "al" };
    // act
    var createCollectionRespose = await this.controller.CreateCollection(collectionDAO) as JsonResult;
    Assert.IsNotNull(createCollectionRespose);
    var collection = createCollectionRespose.Value as CollectionDTO;
    Assert.IsNotNull(collection);
    // assert
    var expected_result = await this.controller.GetCollectionById(collection.Id) as JsonResult;
    Assert.IsNotNull(expected_result);
    Assert.IsTrue(Equals(expected_result.Value as CollectionDTO, collection));
  }

  [TestMethod]
  public async Task GetCollectionById_InputWrongJwtHeader_ResultUnauthorized()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = "abracadabra";
    var collectionId = 1;
    // act
    var response = await this.controller.GetCollectionById(collectionId);
    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(UnauthorizedObjectResult));
  }

  [TestMethod]
  public async Task GetCollectionById_InputAnonymUserGetPrivateCollection_ResultBadRequest()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var collectionDAO = new CollectionDAO() { Name = "al" };

    var collectionCreateResponse = await this.controller.CreateCollection(collectionDAO) as JsonResult;
    Assert.IsNotNull(collectionCreateResponse);
    var collection = collectionCreateResponse.Value as CollectionDTO;
    Assert.IsNotNull(collection);

    this.controller.ControllerContext.HttpContext.Request.Headers.Remove("X-Auth-Token");
    // act

    var response = await this.controller.GetCollectionById(collection.Id);
    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
  }

  [TestMethod]
  public async Task GetCollectionById_InputOwnerGetPrivateCollection_ResultCollection()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var collectionDAO = new CollectionDAO() { Name = "al" };

    var collectionCreateResponse = await this.controller.CreateCollection(collectionDAO) as JsonResult;
    Assert.IsNotNull(collectionCreateResponse);
    var collection = collectionCreateResponse.Value as CollectionDTO;
    Assert.IsNotNull(collection);

    // act
    var response = await this.controller.GetCollectionById(collection.Id) as JsonResult;
    Assert.IsNotNull(response);
    var responseColleciton = response.Value as CollectionDTO;
    // assert
    Assert.IsNotNull(responseColleciton);
    Console.WriteLine(responseColleciton.Boards);
    Console.WriteLine(collection.Boards);
    Assert.AreEqual(collection.Id, responseColleciton.Id);
  }

  [TestMethod]
  public async Task GetCollectionById_InputNotAllowdUserGetCollection_ResultBadRequest()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    await this.userController.Register(new RegisterModel { Username = "vas", Password = "pass" });


    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var collectionDAO = new CollectionDAO() { Name = "al" };

    var collectionCreateResponse = await this.controller.CreateCollection(collectionDAO) as JsonResult;
    Assert.IsNotNull(collectionCreateResponse);
    var collection = collectionCreateResponse.Value as CollectionDTO;
    Assert.IsNotNull(collection);

    await this.userController.Login(new UserModel { Username = "vas", Password = "pass" });
    token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    // act
    var response = await this.controller.GetCollectionById(collection.Id);

    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
  }


  [TestMethod]
  public async Task DeleteCollection_InputOwnerDeleteCollection_ResultNoContent()
  {
    // arrange

    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });

    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var collectionDAO = new CollectionDAO() { Name = "al" };

    var collectionCreateResponse = await this.controller.CreateCollection(collectionDAO) as JsonResult;
    Assert.IsNotNull(collectionCreateResponse);
    var collection = collectionCreateResponse.Value as CollectionDTO;
    Assert.IsNotNull(collection);

    // act
    var response = await this.controller.DeleteCollection(collection.Id);

    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(NoContentResult));
  }

  [TestMethod]
  public async Task DeleteCollection_InputNotOwnerDeleteCollection_ResultBadRequest()
  {
    // arrange
    await this.userController.Register(new RegisterModel { Username = "al", Password = "pass" });
    await this.userController.Register(new RegisterModel { Username = "vas", Password = "pass" });


    await this.userController.Login(new UserModel { Username = "al", Password = "pass" });
    var token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    var collectionDAO = new CollectionDAO() { Name = "al" };

    var collectionCreateResponse = await this.controller.CreateCollection(collectionDAO) as JsonResult;
    Assert.IsNotNull(collectionCreateResponse);
    var collection = collectionCreateResponse.Value as CollectionDTO;
    Assert.IsNotNull(collection);

    await this.userController.Login(new UserModel { Username = "vas", Password = "pass" });
    token = this.userController.Response.Headers["X-Auth-Token"];
    this.controller.ControllerContext.HttpContext.Request.Headers["X-Auth-Token"] = token;

    // act
    var response = await this.controller.DeleteCollection(collection.Id);

    // assert
    Assert.IsNotNull(response);
    Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
  }


#pragma warning restore CA1707 // Identifiers should not contain underscores
  private static bool Equals(CollectionDTO? fir, CollectionDTO sec) => fir is not null &&
                fir.Name == sec.Name &&
                fir.Id == sec.Id &&
                fir.IsShared == sec.IsShared &&
                fir.AuthorId == sec.AuthorId;
}
