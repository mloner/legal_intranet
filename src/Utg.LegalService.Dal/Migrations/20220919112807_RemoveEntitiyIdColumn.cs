using Microsoft.EntityFrameworkCore.Migrations;

namespace Utg.LegalService.Dal.Migrations
{
    public partial class RemoveEntitiyIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "public",
                table: "TaskAttachments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                schema: "public",
                table: "TaskAttachments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
