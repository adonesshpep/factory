using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class ContractReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contract_reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expected_delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    connection_method = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contract_reviews_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contract_reviews_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contract_reviews_MaterialId",
                table: "contract_reviews",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "ix_contract_reviews_number",
                table: "contract_reviews",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contract_reviews_SupplierId",
                table: "contract_reviews",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contract_reviews");
        }
    }
}
