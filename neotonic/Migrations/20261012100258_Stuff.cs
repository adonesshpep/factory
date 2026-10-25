using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class Stuff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractReviewId = table.Column<int>(type: "int", nullable: false),
                    CertificateOfAnalysisId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_certificates_of_analysis_CertificateOfAnalysisId",
                        column: x => x.CertificateOfAnalysisId,
                        principalTable: "certificates_of_analysis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_contract_reviews_ContractReviewId",
                        column: x => x.ContractReviewId,
                        principalTable: "contract_reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "uoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    lot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    box_number = table.Column<int>(type: "int", nullable: false),
                    print_count = table.Column<int>(type: "int", nullable: false),
                    quantity_in = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    quantity_out = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    quantity_work_order = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uoms_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<long>(type: "bigint", nullable: false),
                    work_order_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    batch_size = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    lot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    production_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "memos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<long>(type: "bigint", nullable: false),
                    memo_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    type_id = table.Column<int>(type: "int", nullable: false),
                    invoice_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    invoice_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memos_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memos_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_order_details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_details_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_order_details_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "memo_details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemoId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    split_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    lot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    factory_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    country_origin = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    production_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memo_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memo_details_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memo_details_memos_MemoId",
                        column: x => x.MemoId,
                        principalTable: "memos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_order_detail_uoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_detail_uoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_detail_uoms_uoms_UOMId",
                        column: x => x.UOMId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_order_detail_uoms_work_order_details_WorkOrderDetailId",
                        column: x => x.WorkOrderDetailId,
                        principalTable: "work_order_details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "memo_detail_uoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemoDetailId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memo_detail_uoms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memo_detail_uoms_memo_details_MemoDetailId",
                        column: x => x.MemoDetailId,
                        principalTable: "memo_details",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_memo_detail_uoms_uoms_UOMId",
                        column: x => x.UOMId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_memo_detail_uoms_detail_uom",
                table: "memo_detail_uoms",
                columns: new[] { "MemoDetailId", "UOMId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_memo_detail_uoms_UOMId",
                table: "memo_detail_uoms",
                column: "UOMId");

            migrationBuilder.CreateIndex(
                name: "IX_memo_details_MaterialId",
                table: "memo_details",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "ix_memo_details_memo_material",
                table: "memo_details",
                columns: new[] { "MemoId", "MaterialId" });

            migrationBuilder.CreateIndex(
                name: "ix_memos_number",
                table: "memos",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_memos_SupplierId",
                table: "memos",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_memos_WorkOrderId",
                table: "memos",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_CertificateOfAnalysisId",
                table: "purchase_orders",
                column: "CertificateOfAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_ContractReviewId",
                table: "purchase_orders",
                column: "ContractReviewId");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_orders_number",
                table: "purchase_orders",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_uoms_barcode",
                table: "uoms",
                column: "barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_uoms_MaterialId",
                table: "uoms",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "ix_uoms_number",
                table: "uoms",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_work_order_detail_uoms_detail_uom",
                table: "work_order_detail_uoms",
                columns: new[] { "WorkOrderDetailId", "UOMId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_order_detail_uoms_UOMId",
                table: "work_order_detail_uoms",
                column: "UOMId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_details_MaterialId",
                table: "work_order_details",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_details_WorkOrderId",
                table: "work_order_details",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "ix_work_orders_number",
                table: "work_orders",
                column: "number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "memo_detail_uoms");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropTable(
                name: "work_order_detail_uoms");

            migrationBuilder.DropTable(
                name: "memo_details");

            migrationBuilder.DropTable(
                name: "uoms");

            migrationBuilder.DropTable(
                name: "work_order_details");

            migrationBuilder.DropTable(
                name: "memos");

            migrationBuilder.DropTable(
                name: "work_orders");
        }
    }
}
