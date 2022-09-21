using Microsoft.EntityFrameworkCore.Migrations;

namespace Utg.LegalService.Dal.Migrations
{
    public partial class DropUserProfileIdColimns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorUserProfileId",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PerformerUserProfileId",
                schema: "public",
                table: "Tasks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorUserProfileId",
                schema: "public",
                table: "Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformerUserProfileId",
                schema: "public",
                table: "Tasks",
                type: "text",
                nullable: true);
        }
    }
}
