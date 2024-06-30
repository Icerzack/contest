namespace Camelot.Api.Data;

#pragma warning disable CA1711 // it's naming of the table 
public class User2Collection
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public int CollectionId { get; set; }
  public string ShareMode { get; set; }

}
