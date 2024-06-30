namespace Camelot.Api.Service;

using System.Threading.Tasks;

using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;
using Camelot.Api.Models;
using Camelot.Api.Utils;
using Microsoft.Extensions.Logging;

public class UserService : BaseService
{
  private readonly Password passwordUtil;

  public UserService(AppDbContext context, ILoggerFactory factory) : base(context, factory) => this.passwordUtil = new Password();

  public async Task<UserDTO> CreateUser(RegisterModel user)
  {
    var user_id = await this.userRepo.CreateUser(new User
    {
      Username = user.Username,
      Password = this.passwordUtil.HashPassword(user.Password, out var salt),
      Salt = salt
    });
    return await this.userRepo.GetUser(user_id);
  }

  public async Task<int> CheckUser(string username, string password)
  {
    var p = await this.userRepo.DoesUsernameExist(username);
    if (!p)
    { throw new UnauthorizedException("There no user with such name & password"); }
    p = p && this.passwordUtil.VerifyPassword(
          password,
          await this.userRepo.GetHashPassword(username),
          await this.userRepo.GetSalt(username));
    if (!p)
    { throw new UnauthorizedException("There no user with such name & password"); }

    return await this.userRepo.GetUserId(username);
  }

  public async Task<bool> DoesUsernameExist(string username) => await this.userRepo.DoesUsernameExist(username);

  public async Task<UserDTO> About(int userId) => await this.userRepo.GetUser(userId);
  public async Task<UserDTO> About(string username) => await this.userRepo.GetUser(await this.userRepo.GetUserIdByUsername(username));
  public async Task<OwnerDTO> AboutMe(int userId) => await this.userRepo.AboutMe(userId);
  public async Task<UserDTO> UpdateUser(int userId, UserModel model)
  {
    await this.userRepo.UpdateUser(
            userId: userId,
            username: model.Username,
            passwordHash: this.passwordUtil.HashPassword(model.Password, out var salt),
            salt: salt);
    return await this.userRepo.GetUser(userId);
  }
}
