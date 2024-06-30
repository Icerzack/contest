namespace Camelot.Api.Models;

using System.ComponentModel.DataAnnotations;

using Camelot.Api.Data;

public class RegisterModel
{
  [Required, StringLength(50)]
  public string Username { get; set; }

  [Required, StringLength(50)]
  public string Password { get; set; }

  public User ToUser() => new()
  {
    Username = this.Username,
    Password = this.Password
  };
}
