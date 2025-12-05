using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSqlQueryColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Response",
                table: "ChatMessages",
                newName: "Response_Sql_Query");

            migrationBuilder.AddColumn<string>(
                name: "Response_Answer",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Response_Answer",
                table: "ChatMessages");

            migrationBuilder.RenameColumn(
                name: "Response_Sql_Query",
                table: "ChatMessages",
                newName: "Response");
        }
    }
}
