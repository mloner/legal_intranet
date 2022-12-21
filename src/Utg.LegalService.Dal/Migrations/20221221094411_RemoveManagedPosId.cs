using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utg.LegalService.Dal.Migrations
{
    public partial class RemoveManagedPosId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadUserProfileId",
                schema: "public",
                table: "UserProfileAgregates");

            migrationBuilder.DropColumn(
                name: "ManagerPositionId",
                schema: "public",
                table: "UserProfileAgregates");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfileAgregates_UserProfileId",
                schema: "public",
                table: "UserProfileAgregates",
                column: "UserProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfileAgregates_UserProfileId",
                schema: "public",
                table: "UserProfileAgregates");

            migrationBuilder.AddColumn<int>(
                name: "HeadUserProfileId",
                schema: "public",
                table: "UserProfileAgregates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManagerPositionId",
                schema: "public",
                table: "UserProfileAgregates",
                type: "integer",
                nullable: true);
        }
    }
}
