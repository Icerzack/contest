namespace Camelot.Api.Data;

using System.ComponentModel.DataAnnotations;

public class Collection2Board
{
  [Key]
  public int BoardId { get; set; }
  public int CollectionId { get; set; }

}
