namespace Camelot.Api.Exceptions;

using System;

public class NotAliveTokenException : UnauthorizedException
{
  public NotAliveTokenException() { }
  public NotAliveTokenException(string message) : base(message) { }
  public NotAliveTokenException(string message, Exception inner) : base(message, inner) { }
}
