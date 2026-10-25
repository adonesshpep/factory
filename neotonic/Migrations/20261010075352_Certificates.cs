using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class Certificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "certificates_of_analysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    analysis_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierSampleId = table.Column<int>(type: "int", nullable: false),
                    physical_appearance = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    moisture_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    bacterial_test = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    fungal_test = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    microscopic_appearance = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    color = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    odor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    result = table.Column<int>(type: "int", nullable: false),
                    rejection_reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificates_of_analysis", x => x.Id);
                    table.CheckConstraint("ck_certificates_of_analysis_moisture", "[moisture_percentage] >= 0 AND [moisture_percentage] <= 100");
                    table.CheckConstraint("ck_certificates_of_analysis_rejection", "([result] = 1 AND [rejection_reason] IS NULL) OR ([result] = 2 AND [rejection_reason] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_certificates_of_analysis_supplier_samples_SupplierSampleId",
                        column: x => x.SupplierSampleId,
                        principalTable: "supplier_samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_certificates_of_analysis_number",
                table: "certificates_of_analysis",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_certificates_of_analysis_SupplierSampleId",
                table: "certificates_of_analysis",
                column: "SupplierSampleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certificates_of_analysis");
        }
    }
}
