namespace Camelot.Api.Exceptions;

using System;

public class NotSharedException : Exception
{
  public NotSharedException() { }
  public NotSharedException(string message) : base(message) { }
  public NotSharedException(string message, Exception inner) : base(message, inner) { }
}
