using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleDeGasto.API.Migrations
{
    /// <inheritdoc />
    public partial class fixingIntallmentCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_InstallmentPlans_InstallmentCount",
                table: "InstallmentPlans");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InstallmentPlans_InstallmentCount",
                table: "InstallmentPlans",
                sql: "\"InstallmentCount\" BETWEEN 1 AND 360");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_InstallmentPlans_InstallmentCount",
                table: "InstallmentPlans");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InstallmentPlans_InstallmentCount",
                table: "InstallmentPlans",
                sql: "\"InstallmentCount\" BETWEEN 2 AND 360");
        }
    }
}
