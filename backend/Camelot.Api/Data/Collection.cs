namespace Camelot.Api.Data;

using System;

#pragma warning disable CA1711 // it is naming of the table
public class Collection
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
  public int CollectionId { get; set; }
  public int UserId { get; set; }
  public DateTime CollectionCreationDate { get; set; }
  public string Name { get; set; }

  public bool IsShared { get; set; }

}
