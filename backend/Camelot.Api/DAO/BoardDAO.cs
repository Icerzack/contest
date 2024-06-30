namespace Camelot.Api.Dao;

using System.ComponentModel.DataAnnotations;

using Camelot.Api.Data;

public class BoardDAO
{
  [Required, StringLength(50)]
  public string Name { get; set; }

  [Required]
  public string Elements { get; set; }

  [Required]
  public string Picture { get; set; }

  [Required]
  public string AppState { get; set; }

  public Board ToBoard(int authorId) => new()
  {
    Name = this.Name,
    Elements = this.Elements,
    Picture = this.Picture,
    AppState = this.AppState,
    UserId = authorId,
    BoardCreationDate = System.DateTime.UtcNow
  };
}
