using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseSetting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionString",
                table: "DbSettings");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DbSettings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DbSettings");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionString",
                table: "DbSettings",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
