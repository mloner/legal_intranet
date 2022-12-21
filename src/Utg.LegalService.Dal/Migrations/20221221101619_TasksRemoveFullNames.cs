using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utg.LegalService.Dal.Migrations
{
    public partial class TasksRemoveFullNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorFullName",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PerformerFullName",
                schema: "public",
                table: "Tasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorFullName",
                schema: "public",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformerFullName",
                schema: "public",
                table: "Tasks",
                type: "text",
                nullable: true);
        }
    }
}
