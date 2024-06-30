namespace Camelot.Api.Migrations;

using System;

using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Boards",
        columns: table => new
        {
          BoardId = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          UserId = table.Column<int>(type: "integer", nullable: false),
          Name = table.Column<string>(type: "text", nullable: true),
          Elements = table.Column<string>(type: "text", nullable: true),
          Picture = table.Column<string>(type: "text", nullable: true),
          AppState = table.Column<string>(type: "text", nullable: true),
          BoardCreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_Boards", x => x.BoardId));

    migrationBuilder.CreateTable(
        name: "Coll2Board",
        columns: table => new
        {
          BoardId = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          CollectionId = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_Coll2Board", x => x.BoardId));

    migrationBuilder.CreateTable(
        name: "Collections",
        columns: table => new
        {
          CollectionId = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          UserId = table.Column<int>(type: "integer", nullable: false),
          CollectionCreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          Name = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table => table.PrimaryKey("PK_Collections", x => x.CollectionId));

    migrationBuilder.CreateTable(
        name: "User2Coll",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          UserId = table.Column<int>(type: "integer", nullable: false),
          CollectionId = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_User2Coll", x => x.Id));

    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          UserId = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          Username = table.Column<string>(type: "text", nullable: true),
          Password = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table => table.PrimaryKey("PK_Users", x => x.UserId));
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Boards");

    migrationBuilder.DropTable(
        name: "Coll2Board");

    migrationBuilder.DropTable(
        name: "Collections");

    migrationBuilder.DropTable(
        name: "User2Coll");

    migrationBuilder.DropTable(
        name: "Users");
  }
}
