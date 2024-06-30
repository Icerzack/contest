namespace Camelot.Api.Repository;

using System.Linq;
using System.Threading.Tasks;
using Camelot.Api.Data;
using Camelot.Api.Dto;
using Camelot.Api.Exceptions;

using Microsoft.EntityFrameworkCore;

public class UserRepository
{
  private readonly AppDbContext context;
  public UserRepository(AppDbContext context) => this.context = context;
  public async Task<bool> DoesUserIdExist(int userId) => await this.context.Users
          .Where(c => c.UserId == userId)
          .FirstOrDefaultAsync() is not null;

  public async Task<string> GetHashPassword(string username) => await this.context.Users
          .Where(u => u.Username == username)
          .Select(u => u.Password)
          .FirstOrDefaultAsync();

  public async Task<bool> DoesUsernameExist(string username) => await this.context.Users
          .Where(u => u.Username == username)
          .FirstOrDefaultAsync() is not null;

  public async Task<int> GetUserId(string username) => await this.context.Users
          .Where(u => u.Username == username)
          .Select(u => u.UserId)
          .FirstOrDefaultAsync();
  public async Task<int> GetUserIdByUsername(string username) => await this.context.Users
          .Where(u => u.Username == username)
          .Select(u => u.UserId)
          .FirstOrDefaultAsync();

  public async Task<int> CreateUser(User user)
  {
    this.context.Users.Add(user);
    await this.context.SaveChangesAsync();
    return user.UserId;
  }

  public async Task<UserDTO> GetUser(int userId) => new UserDTO
  {
    Id = userId,
    Name = await this.context.Users
        .Where(u => u.UserId == userId)
        .Select(u => u.Username)
        .FirstOrDefaultAsync() ?? throw new ObjectNotFoundException($"There is no user with id={userId}")

  };
  public async Task<OwnerDTO> AboutMe(int userId) => new OwnerDTO
  {
    Id = userId,
    Name = await this.context.Users
        .Where(u => u.UserId == userId)
        .Select(u => u.Username)
        .FirstOrDefaultAsync() ?? throw new ObjectNotFoundException($"There is no user with id={userId}"),
    Roles = await this.context.User2Coll
        .Where(u => u.UserId == userId)
        .Select(x => new OwnerDTO.Pair
        {
          CollectionId = x.CollectionId,
          Role = x.ShareMode
        })
        .ToArrayAsync()
  };
  public async Task<byte[]> GetSalt(string username) => await this.context.Users
          .Where(u => u.Username == username)
          .Select(u => u.Salt)
          .FirstOrDefaultAsync();
  public async Task UpdateUser(int userId, string username, string passwordHash, byte[] salt)
  {
    var user = await this.context.Users.FindAsync(userId);
    user.Username = username;
    user.Password = passwordHash;
    user.Salt = salt;
    await this.context.SaveChangesAsync();
  }
}
