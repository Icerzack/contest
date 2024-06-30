namespace Camelot.Api.Dto;
using System;
using System.Collections.Generic;

public class CollectionDTO
{
  public int Id { get; set; }
  public DateTime CreationDate { get; set; }
  public int AuthorId { get; set; }
  public string Role { get; set; }
  public string Name { get; set; }
  public bool IsShared { get; set; }
  public IEnumerable<BoardDTO> Boards { get; set; }
}
