namespace Camelot.Api.Utils;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Camelot.Api.Exceptions;
using Camelot.Api.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

public class Token
{
  public static async Task<int> GetUserId(HttpRequest request, BaseService service)
  {
    try
    {
      request.Headers.TryGetValue("X-Auth-Token", out var jwt);
      var user_id = ParseUserId(jwt);
      if (user_id < 0 || !await service.DoesUserExist(user_id))
      { throw new UnauthorizedException(); }
      return user_id;
    }
    catch (NoTokenException) { return 0; }
  }
  public static bool CheckRegisterKey(HttpRequest request)
  {
    request.Headers.TryGetValue("X-Register-Key", out var key);
    return key == Environment.GetEnvironmentVariable("REGISTER_KEY") // they are equel
     || Environment.GetEnvironmentVariable("REGISTER_KEY") == ""; // or ENV var was not set
  }
  public static bool CheckSocketToken(HttpRequest request)
  {
    var key = Environment.GetEnvironmentVariable("SOCKET_KEY");
    request.Headers.TryGetValue("X-Socket-Token", out var token);
    if (token.IsNullOrEmpty())
    { throw new NoTokenException("There was no token named \"X-Socket-Token\" to parse"); }
    if (key != token)
    { throw new InvalidTokenException($"Wrong \"X-Socket-Token\""); }
    return true;
  }

  public static string GenerateToken(string userId)
  {
    var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"));
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(new Claim[] { new("userId", userId) }),
      Expires = DateTime.UtcNow.AddHours(1),
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    var res = tokenHandler.WriteToken(token);
    return res;
  }
  private static int ParseUserId(string jwt)
  {
    if (IsTokenAlive(jwt))
    {
      var handler = new JwtSecurityTokenHandler();
      var token = handler.ReadJwtToken(jwt);
      var userId = token.Payload["userId"].ToString();
      return int.Parse(userId, System.Globalization.CultureInfo.CurrentCulture);
    }
    throw new NotAliveTokenException("Not Alive Token");
  }
  private static bool IsTokenAlive(string jwt)
  {
    try
    {
      var handler = new JwtSecurityTokenHandler();
      var token = handler.ReadJwtToken(jwt);
      return TimeSpan.Compare(token.ValidTo - DateTime.UtcNow, new TimeSpan(0, 0, 0)) > 0;
    }
    catch (ArgumentNullException)
    { throw new NoTokenException("There was no token named \"X-Auth-Token\" to parse"); }
    catch (Exception)
    { throw new InvalidTokenException(); }
  }

}
