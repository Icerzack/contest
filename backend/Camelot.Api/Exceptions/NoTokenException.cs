namespace Camelot.Api.Exceptions;

using System;

public class NoTokenException : UnauthorizedException
{
  public NoTokenException() { }
  public NoTokenException(string message) : base(message) { }
  public NoTokenException(string message, Exception inner) : base(message, inner) { }
}
