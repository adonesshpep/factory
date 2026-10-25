using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class SupplierSamples : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "supplier_samples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ContractReviewId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_samples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_samples_contract_reviews_ContractReviewId",
                        column: x => x.ContractReviewId,
                        principalTable: "contract_reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_samples_ContractReviewId",
                table: "supplier_samples",
                column: "ContractReviewId");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_samples_number",
                table: "supplier_samples",
                column: "number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_samples");
        }
    }
}
