namespace Camelot.Api.Dto;

using System.Collections.Generic;

public class OwnerDTO
{
  public int Id { get; set; }
  public string Name { get; set; }
  public IEnumerable<Pair> Roles { get; set; }

  public class Pair
  {
    public int CollectionId { get; set; }
    public string Role { get; set; }
  }
}
