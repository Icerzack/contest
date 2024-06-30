namespace Camelot.Api.Dao;

using System;
using System.Collections.Generic;

public class ShareDao
{
  public List<string> Reader { get; set; }
  public List<string> Editor { get; set; }
  public List<string> Moderator { get; set; }
  public bool Anonym { get; set; }

  public List<Tuple<string, string>> ToPairList()
  { // list<tuple<username, sharingMode>>
    var result = new List<Tuple<string, string>>();
    foreach (var reader in this.Reader)
    {
      if (reader is not null)
      { result.Add(new Tuple<string, string>(reader, "reader")); }
    }
    foreach (var editor in this.Editor)
    {
      if (editor is not null)
      { result.Add(new Tuple<string, string>(editor, "editor")); }
    }
    foreach (var moderator in this.Moderator)
    {
      if (moderator is not null)
      { result.Add(new Tuple<string, string>(moderator, "moderator")); }
    }
    // if (this.Anonym)
    // { result.Add(new Tuple<string, string>(null, "anonym")); }
    return result;
  }
}
