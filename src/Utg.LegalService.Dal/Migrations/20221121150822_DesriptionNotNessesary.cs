using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utg.LegalService.Dal.Migrations
{
    public partial class DesriptionNotNessesary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "public",
                table: "Tasks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "public",
                table: "Tasks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
