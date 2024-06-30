namespace Camelot.Api.Exceptions;

using System;

public class ObjectNotFoundException : Exception
{
  public ObjectNotFoundException() { }
  public ObjectNotFoundException(string message) : base(message) { }
  public ObjectNotFoundException(string message, Exception inner) : base(message, inner) { }
}
