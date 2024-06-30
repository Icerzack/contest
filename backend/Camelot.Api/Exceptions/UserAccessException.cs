namespace Camelot.Api.Exceptions;

using System;

public class UserAccessException : Exception
{
  public UserAccessException() { }
  public UserAccessException(string message) : base(message) { }
  public UserAccessException(string message, Exception inner) : base(message, inner) { }
}
