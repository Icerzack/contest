namespace Camelot.Api.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class ShareModes : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.AddColumn<string>(
        name: "ShareMode",
        table: "User2Coll",
        type: "text",
        nullable: true);

    migrationBuilder.AddColumn<string>(
        name: "ShareMode",
        table: "Collections",
        type: "text",
        nullable: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "ShareMode",
        table: "User2Coll");

    migrationBuilder.DropColumn(
        name: "ShareMode",
        table: "Collections");
  }
}
