namespace Camelot.Api.Dto;

using System.Collections.Generic;

public class ShareDTO
{
  public List<string> Reader { get; set; }
  public List<string> Editor { get; set; }
  public List<string> Moderator { get; set; }
  public bool Anonym { get; set; }
}
