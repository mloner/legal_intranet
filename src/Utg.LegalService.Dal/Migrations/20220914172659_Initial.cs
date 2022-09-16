using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Utg.LegalService.Dal.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Tasks",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AuthorUserProfileId = table.Column<string>(type: "text", nullable: true),
                    AuthorFullName = table.Column<string>(type: "text", nullable: true),
                    CreationDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PerformerUserProfileId = table.Column<string>(type: "text", nullable: true),
                    PerformerFullName = table.Column<string>(type: "text", nullable: true),
                    DeadlineDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangeDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks",
                schema: "public");
        }
    }
}
