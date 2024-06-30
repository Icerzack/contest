namespace Camelot.Api.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class IsShared : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "ShareMode",
        table: "Collections");

    migrationBuilder.AddColumn<bool>(
        name: "IsShared",
        table: "Collections",
        type: "boolean",
        nullable: false,
        defaultValue: false);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropColumn(
        name: "IsShared",
        table: "Collections");

    migrationBuilder.AddColumn<string>(
        name: "ShareMode",
        table: "Collections",
        type: "text",
        nullable: true);
  }
}
