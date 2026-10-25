using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class RestrictionsOnAnnDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_annual_plan_details_AnnualPlanId",
                table: "annual_plan_details");

            migrationBuilder.CreateIndex(
                name: "ix_annual_plan_details_plan_material",
                table: "annual_plan_details",
                columns: new[] { "AnnualPlanId", "MaterialId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_annual_plan_details_plan_material",
                table: "annual_plan_details");

            migrationBuilder.CreateIndex(
                name: "IX_annual_plan_details_AnnualPlanId",
                table: "annual_plan_details",
                column: "AnnualPlanId");
        }
    }
}
