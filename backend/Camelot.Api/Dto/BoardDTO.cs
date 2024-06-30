namespace Camelot.Api.Dto;
using System;

public class BoardDTO
{
  public int Id { get; set; }
  public DateTime CreationDate { get; set; }
  public int AuthorId { get; set; }
  public int CollectionId { get; set; }
  public bool IsShared { get; set; }
  public string Name { get; set; }
  public string Elements { get; set; }
  public string Picture { get; set; }
  public string AppState { get; set; }
}
