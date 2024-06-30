namespace Camelot.Api.Exceptions;

using System;

public class InvalidTokenException : UnauthorizedException
{
  public InvalidTokenException() { }
  public InvalidTokenException(string message) : base(message) { }
  public InvalidTokenException(string message, Exception inner) : base(message, inner) { }
}
