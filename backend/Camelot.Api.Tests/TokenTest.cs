namespace Camelot.Api.Tests.Utils;

using System.IdentityModel.Tokens.Jwt;
using Camelot.Api.Data;
using Camelot.Api.Exceptions;
using Camelot.Api.Service;
using Camelot.Api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1707 // Identifiers should not contain underscores

[TestClass]
public class TokenTest
{
  [TestMethod]
  public void GetUserId_NoInputJwtHeader_Result0()
  {
    var httpContext = new DefaultHttpContext();

    var logger = new LoggerFactory();
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_token_db");

    // arrange

    var db = new AppDbContext(dbOptionsBuilder.Options);
    db.Set<User>().Add(new User()
    { UserId = 1, Username = "al", Password = "pass" });
    db.SaveChangesAsync();

    var service = new BaseService(db, logger);
    var request = httpContext.Request;

    // act
    var userId = Token.GetUserId(request, service).Result;

    // assert
    Assert.AreEqual(0, userId);
  }

  [TestMethod]
  public void GetUserId_InputGeneratedJWT_ResultUserId1()
  {
    var httpContext = new DefaultHttpContext();
    var logger = new LoggerFactory();
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_db");
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");

    // arrange
    var db = new AppDbContext(dbOptionsBuilder.Options);
    db.Set<User>().Add(new User()
    { UserId = 1, Username = "al", Password = "pass" });
    db.SaveChangesAsync();
    var userId = "1";

    var service = new BaseService(db, logger);

    var token = Token.GenerateToken(userId);
    httpContext.Request.Headers["X-Auth-Token"] = token;
    var request = httpContext.Request;

    //act
    var task = Token.GetUserId(request, service);
    var result = task.Result;
    //assert
    Assert.AreEqual(1, result);
  }

  [TestMethod]
  public void GetUserId_InputNotExistedUser_ResultUnauthorizedException()
  {
    var httpContext = new DefaultHttpContext();
    var logger = new LoggerFactory();
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_db");
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");

    // arrange
    var db = new AppDbContext(dbOptionsBuilder.Options);
    db.Set<User>().Add(new User()
    { UserId = 2, Username = "vas", Password = "pass" });
    db.SaveChangesAsync();

    var service = new BaseService(db, logger);
    httpContext.Request.Headers["X-Auth-Token"] = "eyJhbGciolJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwibmJmIjoxNzE1MDg1MTcyLCJleHAiOjE3MTUwODg3NzIsImlhdCI6MTcxNTA4NTE3Mn0sdC4VSR2fvn1wcs0Pj1yQ2Nx_oLCeDpkhCISfud2mzk4";
    var request = httpContext.Request;

    try // act
    {
      _ = Token.GetUserId(request, service);
    }
    catch (Exception e)// assert
    {
      Assert.AreEqual(new UnauthorizedException().GetType(), e.GetType());
    }
  }

  [TestMethod]
  public void GetUserId_InputIncorrectJWT_ResultInvalitTokenException()
  {
    var httpContext = new DefaultHttpContext();
    var logger = new LoggerFactory();
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_db");
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");

    // arrange
    var db = new AppDbContext(dbOptionsBuilder.Options);
    db.Set<User>().Add(new User()
    { UserId = 1, Username = "al", Password = "pass" });
    db.SaveChangesAsync();

    var service = new BaseService(db, logger);
    httpContext.Request.Headers["X-Auth-Token"] = "eyJhbGciolJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwibmJmIjoxNzE1MDg1MTcyLCJleHAiOjE3MTUwODg3NzIsImlhdCI6MTcxNTA4NTE3Mn0sdC4VSR2fvn1wcs0Pj1yQ2Nx_oLCeDpkhCISfud2mzk4";
    var request = httpContext.Request;

    try // act
    {
      _ = Token.GetUserId(request, service);
    }
    catch (Exception e)// assert
    {
      Assert.AreEqual(new InvalidTokenException().GetType(), e.GetType());
    }
  }

  [TestMethod]
  public void GetUserId_InputNotAliveJWT_ResultNoTokenException()
  {
    var httpContext = new DefaultHttpContext();
    var logger = new LoggerFactory();
    var dbOptionsBuilder = new DbContextOptionsBuilder().UseInMemoryDatabase("test_db");
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");

    // arrange

    var db = new AppDbContext(dbOptionsBuilder.Options);
    db.Set<User>().Add(new User()
    { UserId = 1, Username = "al", Password = "pass" });
    db.SaveChangesAsync();

    var service = new BaseService(db, logger);
    httpContext.Request.Headers["X-Auth-Token"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiIxIiwibmJmIjoxNzE1MDg1MTcyLCJleHAiOjE3MTUwODg3NzIsImlhdCI6MTcxNTA4NTE3Mn0.eC4VSR2fvn1wcs0Pj1yQ2Nx_oLCeDpkhCISfud2mzk4";
    var request = httpContext.Request;

    try // act
    {
      _ = Token.GetUserId(request, service);
    }
    catch (Exception e) // assert
    {
      Assert.AreEqual(new NotAliveTokenException().GetType(), e.GetType(), e.Message);
    }

  }


  [TestMethod]
  public void GenerateToken_InputUserId1_ResultAnyString()
  {
    var userId = "1";
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");

    var token = Token.GenerateToken(userId);

    Assert.AreEqual("".GetType(), token.GetType());
  }

  [TestMethod]
  public void GenerateToken_InputUserId1_ResultJWToken()
  {
    var userId = "1";
    Environment.SetEnvironmentVariable("JWT_KEY", "This is my very Secret Key from diploma camilot api");
    var handler = new JwtSecurityTokenHandler();

    //code
    var token = Token.GenerateToken(userId);
    //decode
    var decodedToken = handler.ReadJwtToken(token);
    var decodedUserId = decodedToken.Payload["userId"].ToString();

    //assert
    Assert.AreEqual(userId, decodedUserId);
  }


  [TestMethod]
  public void CheckSocketToken_NoInputSockedHeader_ResultNoTokenException()
  {
    Environment.SetEnvironmentVariable("SOCKET_KEY", "This is my very Secret Key from diploma camilot api");
    var httpContext = new DefaultHttpContext();


    var request = httpContext.Request;
    try
    { Token.CheckSocketToken(request); }
    catch (Exception e)
    { Assert.AreEqual(new NoTokenException().GetType(), e.GetType()); }

  }

  [TestMethod]
  public void CheckSocketToken_InputWrongSockedHeader_ResultInvalidTokenException()
  {
    Environment.SetEnvironmentVariable("SOCKET_KEY", "This is my very Secret Key from diploma camilot api");
    var httpContext = new DefaultHttpContext();

    httpContext.Request.Headers["X-Socket-Token"] = "abracadabra";
    var request = httpContext.Request;

    try
    {
      _ = Token.CheckSocketToken(request);
    }
    catch (Exception e)
    { Assert.AreEqual(new InvalidTokenException().GetType(), e.GetType()); }
  }

  [TestMethod]
  public void CheckSocketToken_InputSockedHeader_ResultTrue()
  {
    Environment.SetEnvironmentVariable("SOCKET_KEY", "This is my very Secret Key from diploma camilot api");
    var httpContext = new DefaultHttpContext();

    httpContext.Request.Headers["X-Socket-Token"] = "This is my very Secret Key from diploma camilot api";
    var request = httpContext.Request;

    var result = Token.CheckSocketToken(request);

    Assert.IsTrue(result);

  }
}
#pragma warning restore CA1707 // Identifiers should not contain underscores
