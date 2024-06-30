namespace Camelot.Api.Utils;

using System;
using System.Security.Cryptography;
using System.Text;

public class Password
{
  private const int KeySize = 64;
  private const int Iterations = 350000;
  private readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

  public string HashPassword(string password, out byte[] salt)
  {
    salt = RandomNumberGenerator.GetBytes(KeySize);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        Encoding.UTF8.GetBytes(password),
        salt,
        Iterations,
        this.hashAlgorithm,
        KeySize);
    return Convert.ToHexString(hash);
  }
  public bool VerifyPassword(string password, string hash, byte[] salt)
  {
    var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, this.hashAlgorithm, KeySize);
    return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hash));
  }
}
