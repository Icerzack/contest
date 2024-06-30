namespace Camelot.Api.Data;

public class User
{
  public int UserId { get; set; }
  public string Username { get; set; }
  public string Password { get; set; }
  public byte[] Salt { get; set; }
}
