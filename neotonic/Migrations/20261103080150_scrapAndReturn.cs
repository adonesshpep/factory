using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class scrapAndReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "scrap_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    TransferOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scrap_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scrap_orders_transfer_orders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "transfer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "supplier_return_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    TransferOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_return_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_return_orders_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_supplier_return_orders_transfer_orders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "transfer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scrap_orders_number",
                table: "scrap_orders",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scrap_orders_transfer_order",
                table: "scrap_orders",
                column: "TransferOrderId",
                unique: true,
                filter: "[TransferOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_return_orders_number",
                table: "supplier_return_orders",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_return_orders_SupplierId",
                table: "supplier_return_orders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_return_orders_transfer_order",
                table: "supplier_return_orders",
                column: "TransferOrderId",
                unique: true,
                filter: "[TransferOrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scrap_orders");

            migrationBuilder.DropTable(
                name: "supplier_return_orders");
        }
    }
}
