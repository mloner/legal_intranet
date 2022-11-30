﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utg.LegalService.Dal.Migrations
{
    public partial class CommentsAddCreatedUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                schema: "public",
                table: "TaskComments",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                schema: "public",
                table: "TaskComments",
                type: "timestamp without time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                schema: "public",
                table: "TaskComments");

            migrationBuilder.DropColumn(
                name: "Updated",
                schema: "public",
                table: "TaskComments");
        }
    }
}
