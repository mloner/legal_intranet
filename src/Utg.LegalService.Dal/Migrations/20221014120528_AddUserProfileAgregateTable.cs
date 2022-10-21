using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Utg.LegalService.Dal.Migrations
{
    public partial class AddUserProfileAgregateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserProfileAgregates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserProfileId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TabN = table.Column<string>(type: "text", nullable: true),
                    DismissalDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CompanyId = table.Column<int>(type: "integer", nullable: true),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentName = table.Column<string>(type: "text", nullable: true),
                    PositionId = table.Column<int>(type: "integer", nullable: true),
                    PositionName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    ManagerPositionId = table.Column<int>(type: "integer", nullable: true),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    HeadUserProfileId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileAgregates", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfileAgregates",
                schema: "public");
        }
    }
}
