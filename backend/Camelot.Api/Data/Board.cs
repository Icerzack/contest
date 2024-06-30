namespace Camelot.Api.Data;

using System;

public class Board
{

  public int BoardId { get; set; }
  public int UserId { get; set; }

  public string Name { get; set; }

  public string Elements { get; set; }

  public string Picture { get; set; }
  public string AppState { get; set; }
  public DateTime BoardCreationDate { get; set; }

}
