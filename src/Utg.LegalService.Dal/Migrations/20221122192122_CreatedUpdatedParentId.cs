using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utg.LegalService.Dal.Migrations
{
    public partial class CreatedUpdatedParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DismissalDate",
                schema: "public",
                table: "UserProfileAgregates",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastChangeDateTime",
                schema: "public",
                table: "Tasks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeadlineDateTime",
                schema: "public",
                table: "Tasks",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDateTime",
                schema: "public",
                table: "Tasks",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                schema: "public",
                table: "Tasks",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ParentTaskId",
                schema: "public",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "public",
                table: "Tasks",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                schema: "public",
                table: "TaskComments",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                schema: "public",
                table: "TaskAttachments",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "public",
                table: "TaskAttachments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ParentTaskId",
                schema: "public",
                table: "Tasks",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Tasks_ParentTaskId",
                schema: "public",
                table: "Tasks",
                column: "ParentTaskId",
                principalSchema: "public",
                principalTable: "Tasks",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Tasks_ParentTaskId",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ParentTaskId",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "public",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Created",
                schema: "public",
                table: "TaskAttachments");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "public",
                table: "TaskAttachments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DismissalDate",
                schema: "public",
                table: "UserProfileAgregates",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastChangeDateTime",
                schema: "public",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeadlineDateTime",
                schema: "public",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDateTime",
                schema: "public",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                schema: "public",
                table: "TaskComments",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
