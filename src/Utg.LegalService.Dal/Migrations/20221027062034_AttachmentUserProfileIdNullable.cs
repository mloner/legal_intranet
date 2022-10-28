using Microsoft.EntityFrameworkCore.Migrations;

namespace Utg.LegalService.Dal.Migrations
{
    public partial class AttachmentUserProfileIdNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                schema: "public",
                table: "TaskAttachments",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserProfileId",
                schema: "public",
                table: "TaskAttachments");
        }
    }
}
