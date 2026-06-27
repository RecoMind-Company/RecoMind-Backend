using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePlanIdColumnToModuleId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "Quests",
                newName: "ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_Quests_PlanId",
                table: "Quests",
                newName: "IX_Quests_ModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "Quests",
                newName: "PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_Quests_ModuleId",
                table: "Quests",
                newName: "IX_Quests_PlanId");
        }
    }
}
