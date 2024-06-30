namespace Camelot.Api.Dao;

using System.ComponentModel.DataAnnotations;

using Camelot.Api.Data;

public class CollectionDAO
{
  [Required, StringLength(50)]
  public string Name { get; set; }

  public Collection ToCollection(int authorId) => new()
  {
    Name = this.Name,
    UserId = authorId,
    CollectionCreationDate = System.DateTime.UtcNow,
    IsShared = false
  };
}
