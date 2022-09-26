using Microsoft.EntityFrameworkCore.Migrations;

namespace Utg.LegalService.Dal.Migrations
{
    public partial class AddTaskIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAttachments_Tasks_TaskId",
                schema: "public",
                table: "TaskAttachments");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                schema: "public",
                table: "TaskAttachments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAttachments_Tasks_TaskId",
                schema: "public",
                table: "TaskAttachments",
                column: "TaskId",
                principalSchema: "public",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskAttachments_Tasks_TaskId",
                schema: "public",
                table: "TaskAttachments");

            migrationBuilder.AlterColumn<int>(
                name: "TaskId",
                schema: "public",
                table: "TaskAttachments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAttachments_Tasks_TaskId",
                schema: "public",
                table: "TaskAttachments",
                column: "TaskId",
                principalSchema: "public",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
