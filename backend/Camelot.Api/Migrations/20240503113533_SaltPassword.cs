
namespace Camelot.Api.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class SaltPassword : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<byte[]>(
        name: "Salt",
        table: "Users",
        type: "bytea",
        nullable: true);

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
        name: "Salt",
        table: "Users");
}
