using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class Transfare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificates_of_analysis_supplier_samples_SupplierSampleId",
                table: "certificates_of_analysis");

            migrationBuilder.DropForeignKey(
                name: "FK_contract_reviews_materials_MaterialId",
                table: "contract_reviews");

            migrationBuilder.DropTable(
                name: "supplier_samples");

            migrationBuilder.DropCheckConstraint(
                name: "ck_certificates_of_analysis_rejection",
                table: "certificates_of_analysis");

            migrationBuilder.DropColumn(
                name: "quantity_in",
                table: "uoms");

            migrationBuilder.DropColumn(
                name: "quantity_out",
                table: "uoms");

            migrationBuilder.RenameColumn(
                name: "quantity_work_order",
                table: "uoms",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "MaterialId",
                table: "contract_reviews",
                newName: "RFQId");

            migrationBuilder.RenameIndex(
                name: "IX_contract_reviews_MaterialId",
                table: "contract_reviews",
                newName: "IX_contract_reviews_RFQId");

            migrationBuilder.RenameColumn(
                name: "SupplierSampleId",
                table: "certificates_of_analysis",
                newName: "SampleId");

            migrationBuilder.RenameIndex(
                name: "IX_certificates_of_analysis_SupplierSampleId",
                table: "certificates_of_analysis",
                newName: "IX_certificates_of_analysis_SampleId");

            migrationBuilder.CreateSequence(
                name: "memo_rw_in_number_seq");

            migrationBuilder.CreateSequence(
                name: "uom_material_seq");

            migrationBuilder.CreateSequence(
                name: "uom_product_seq");

            migrationBuilder.AlterColumn<int>(
                name: "MaterialId",
                table: "uoms",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "uoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentUomId",
                table: "uoms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "uoms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShelfId",
                table: "uoms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "kind",
                table: "uoms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FlowId",
                table: "material_types",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "flows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "memo_rw_in",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "NEXT VALUE FOR memo_rw_in_number_seq"),
                    memo_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memo_rw_in", x => x.Id);
                    table.ForeignKey(
                        name: "FK_memo_rw_in_purchase_orders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "purchase_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memo_rw_in_workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rfqs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnnualPlanID = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rfqs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rfqs_annual_plans_AnnualPlanID",
                        column: x => x.AnnualPlanID,
                        principalTable: "annual_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rfqs_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "samples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    sample_type = table.Column<int>(type: "int", nullable: false),
                    sample_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractReviewId = table.Column<int>(type: "int", nullable: true),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_samples", x => x.Id);
                    table.CheckConstraint("ck_samples_link", "([sample_type] = 1 AND [ContractReviewId] IS NOT NULL AND [UomId] IS NULL) OR ([sample_type] = 2 AND [UomId] IS NOT NULL AND [ContractReviewId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_samples_contract_reviews_ContractReviewId",
                        column: x => x.ContractReviewId,
                        principalTable: "contract_reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_samples_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    kind = table.Column<int>(type: "int", nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: true),
                    zone = table.Column<int>(type: "int", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.Id);
                    table.CheckConstraint("ck_locations_inventory_zone", "([kind] = 1 AND [InventoryId] IS NOT NULL AND [zone] IS NOT NULL) OR ([kind] = 2 AND [InventoryId] IS NULL AND [zone] IS NULL)");
                    table.ForeignKey(
                        name: "FK_locations_inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memo_rw_details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemoId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    tear = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memo_rw_details", x => x.Id);
                    table.CheckConstraint("ck_memo_rw_details_tear", "[tear] >= 0");
                    table.ForeignKey(
                        name: "FK_memo_rw_details_memo_rw_in_MemoId",
                        column: x => x.MemoId,
                        principalTable: "memo_rw_in",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_memo_rw_details_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "flow_steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlowId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_flow_steps_flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flow_steps_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shelves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    number = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shelves", x => x.Id);
                    table.UniqueConstraint("ak_shelves_id_location", x => new { x.Id, x.LocationId });
                    table.ForeignKey(
                        name: "FK_shelves_locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transfer_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromLocationId = table.Column<int>(type: "int", nullable: false),
                    ToLocationId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    executed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_orders", x => x.Id);
                    table.CheckConstraint("ck_transfer_orders_locations", "[FromLocationId] <> [ToLocationId]");
                    table.ForeignKey(
                        name: "FK_transfer_orders_locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_orders_locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_orders_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_moves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    FromLocationId = table.Column<int>(type: "int", nullable: false),
                    ToLocationId = table.Column<int>(type: "int", nullable: false),
                    FromShelfId = table.Column<int>(type: "int", nullable: true),
                    ToShelfId = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    move_type = table.Column<int>(type: "int", nullable: false),
                    source_type = table.Column<int>(type: "int", nullable: false),
                    source_id = table.Column<int>(type: "int", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    move_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_moves", x => x.Id);
                    table.CheckConstraint("ck_stock_moves_quantity", "[quantity] > 0");
                    table.ForeignKey(
                        name: "FK_stock_moves_locations_FromLocationId",
                        column: x => x.FromLocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_moves_locations_ToLocationId",
                        column: x => x.ToLocationId,
                        principalTable: "locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_moves_shelves_FromShelfId",
                        column: x => x.FromShelfId,
                        principalTable: "shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_moves_shelves_ToShelfId",
                        column: x => x.ToShelfId,
                        principalTable: "shelves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_moves_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preparing_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProtocolId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    TransferOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preparing_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preparing_orders_protocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "protocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_preparing_orders_transfer_orders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "transfer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "release_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    order_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManufacturingOrderId = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    TransferOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_release_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_release_orders_transfer_orders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "transfer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transfer_order_lines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferOrderId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    is_scanned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_order_lines", x => x.Id);
                    table.CheckConstraint("ck_transfer_order_lines_quantity", "[quantity] > 0");
                    table.ForeignKey(
                        name: "FK_transfer_order_lines_transfer_orders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "transfer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transfer_order_lines_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "flows",
                columns: new[] { "Id", "is_active", "name_ar", "name_en" },
                values: new object[,]
                {
                    { 1, true, "عضوي", "Organic" },
                    { 2, true, "كيميائي", "Chemical" },
                    { 3, true, "تعبئة", "Filling" }
                });

            migrationBuilder.InsertData(
                table: "inventories",
                columns: new[] { "Id", "code", "is_active", "name_ar", "name_en", "sort_order" },
                values: new object[,]
                {
                    { 1, "I1", true, "الاستلام", "Receiving", 1 },
                    { 2, "I2", true, "التحضير", "Preparing", 2 },
                    { 3, "FIL", true, "التعبئة", "Filling", 3 },
                    { 4, "PRD", true, "المنتجات المنتجة", "Products produced", 4 },
                    { 5, "PTS", true, "المنتجات الجاهزة", "Products ready to sell", 5 }
                });

            migrationBuilder.InsertData(
                table: "locations",
                columns: new[] { "Id", "code", "InventoryId", "is_active", "kind", "name_ar", "name_en", "sort_order", "zone" },
                values: new object[,]
                {
                    { 901, "VIRT-CONSUMED", null, true, 2, "مستهلك", "Consumed", 16, null },
                    { 902, "VIRT-SCRAP", null, true, 2, "تالف", "Scrap", 17, null },
                    { 903, "VIRT-SUPPLIER", null, true, 2, "مرتجع للمورد", "Supplier returns", 18, null },
                    { 904, "VIRT-PROD", null, true, 2, "أرضية الإنتاج", "Production floor", 19, null },
                    { 11, "I1-QUAR", 1, true, 1, "حجر الاستلام", "Receiving quarantine", 1, 1 },
                    { 12, "I1-ACC", 1, true, 1, "مقبول الاستلام", "Receiving accepted", 2, 2 },
                    { 13, "I1-REJ", 1, true, 1, "مرفوض الاستلام", "Receiving rejected", 3, 3 },
                    { 21, "I2-QUAR", 2, true, 1, "حجر التحضير", "Preparing quarantine", 4, 1 },
                    { 22, "I2-ACC", 2, true, 1, "مقبول التحضير", "Preparing accepted", 5, 2 },
                    { 23, "I2-REJ", 2, true, 1, "مرفوض التحضير", "Preparing rejected", 6, 3 },
                    { 31, "FIL-QUAR", 3, true, 1, "حجر التعبئة", "Filling quarantine", 7, 1 },
                    { 32, "FIL-ACC", 3, true, 1, "مقبول التعبئة", "Filling accepted", 8, 2 },
                    { 33, "FIL-REJ", 3, true, 1, "مرفوض التعبئة", "Filling rejected", 9, 3 },
                    { 41, "PRD-QUAR", 4, true, 1, "حجر المنتجات المنتجة", "Products produced quarantine", 10, 1 },
                    { 42, "PRD-ACC", 4, true, 1, "مقبول المنتجات المنتجة", "Products produced accepted", 11, 2 },
                    { 43, "PRD-REJ", 4, true, 1, "مرفوض المنتجات المنتجة", "Products produced rejected", 12, 3 },
                    { 51, "PTS-QUAR", 5, true, 1, "حجر الجاهز", "Ready to sell quarantine", 13, 1 },
                    { 52, "PTS-ACC", 5, true, 1, "مقبول الجاهز", "Ready to sell accepted", 14, 2 },
                    { 53, "PTS-REJ", 5, true, 1, "مرفوض الجاهز", "Ready to sell rejected", 15, 3 }
                });

            migrationBuilder.InsertData(
                table: "flow_steps",
                columns: new[] { "Id", "FlowId", "LocationId", "order_index" },
                values: new object[,]
                {
                    { 1, 1, 11, 1 },
                    { 2, 1, 21, 2 },
                    { 3, 1, 41, 3 },
                    { 4, 1, 51, 4 },
                    { 5, 2, 21, 1 },
                    { 6, 2, 41, 2 },
                    { 7, 2, 51, 3 },
                    { 8, 3, 31, 1 },
                    { 9, 3, 41, 2 },
                    { 10, 3, 51, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_uoms_location_material",
                table: "uoms",
                columns: new[] { "LocationId", "MaterialId" });

            migrationBuilder.CreateIndex(
                name: "ix_uoms_location_product",
                table: "uoms",
                columns: new[] { "LocationId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "ix_uoms_parent_uom",
                table: "uoms",
                column: "ParentUomId");

            migrationBuilder.CreateIndex(
                name: "IX_uoms_ProductId",
                table: "uoms",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_uoms_ShelfId_LocationId",
                table: "uoms",
                columns: new[] { "ShelfId", "LocationId" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_uoms_kind_link",
                table: "uoms",
                sql: "([kind] = 1 AND [MaterialId] IS NOT NULL AND [ProductId] IS NULL) OR ([kind] = 2 AND [ProductId] IS NOT NULL AND [MaterialId] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_uoms_quantity",
                table: "uoms",
                sql: "[quantity] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_material_types_FlowId",
                table: "material_types",
                column: "FlowId");

            migrationBuilder.AddCheckConstraint(
                name: "ck_certificates_of_analysis_rejection",
                table: "certificates_of_analysis",
                sql: "([result] = 0 AND [rejection_reason] IS NULL) OR ([result] = 1 AND [rejection_reason] IS NULL) OR ([result] = 2 AND [rejection_reason] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_flow_steps_flow_order",
                table: "flow_steps",
                columns: new[] { "FlowId", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flow_steps_LocationId",
                table: "flow_steps",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "ix_flows_name_ar",
                table: "flows",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flows_name_en",
                table: "flows",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inventories_code",
                table: "inventories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_locations_code",
                table: "locations",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_locations_inventory_zone",
                table: "locations",
                columns: new[] { "InventoryId", "zone" },
                unique: true,
                filter: "[InventoryId] IS NOT NULL AND [zone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_memo_rw_details_MemoId",
                table: "memo_rw_details",
                column: "MemoId");

            migrationBuilder.CreateIndex(
                name: "ix_memo_rw_details_uom",
                table: "memo_rw_details",
                column: "UomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_memo_rw_in_number",
                table: "memo_rw_in",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_memo_rw_in_PurchaseOrderId",
                table: "memo_rw_in",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_memo_rw_in_WorkerId",
                table: "memo_rw_in",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "ix_preparing_orders_number",
                table: "preparing_orders",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_preparing_orders_ProtocolId",
                table: "preparing_orders",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "ix_preparing_orders_transfer_order",
                table: "preparing_orders",
                column: "TransferOrderId",
                unique: true,
                filter: "[TransferOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_release_orders_number",
                table: "release_orders",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_release_orders_transfer_order",
                table: "release_orders",
                column: "TransferOrderId",
                unique: true,
                filter: "[TransferOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_rfqs_AnnualPlanID",
                table: "rfqs",
                column: "AnnualPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_rfqs_MaterialId",
                table: "rfqs",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "ix_rfqs_number",
                table: "rfqs",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_samples_contract_review",
                table: "samples",
                column: "ContractReviewId");

            migrationBuilder.CreateIndex(
                name: "ix_samples_number",
                table: "samples",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_samples_uom",
                table: "samples",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "ix_shelves_id_location",
                table: "shelves",
                columns: new[] { "Id", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shelves_location_number",
                table: "shelves",
                columns: new[] { "LocationId", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_moves_FromLocationId",
                table: "stock_moves",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_moves_FromShelfId",
                table: "stock_moves",
                column: "FromShelfId");

            migrationBuilder.CreateIndex(
                name: "ix_stock_moves_source",
                table: "stock_moves",
                columns: new[] { "source_type", "source_id" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_moves_ToLocationId",
                table: "stock_moves",
                column: "ToLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_moves_ToShelfId",
                table: "stock_moves",
                column: "ToShelfId");

            migrationBuilder.CreateIndex(
                name: "ix_stock_moves_uom_date",
                table: "stock_moves",
                columns: new[] { "UomId", "move_date" });

            migrationBuilder.CreateIndex(
                name: "ix_transfer_order_lines_order_uom",
                table: "transfer_order_lines",
                columns: new[] { "TransferOrderId", "UomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transfer_order_lines_uom",
                table: "transfer_order_lines",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_orders_FromLocationId",
                table: "transfer_orders",
                column: "FromLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_orders_MaterialId",
                table: "transfer_orders",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "ix_transfer_orders_status",
                table: "transfer_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_orders_ToLocationId",
                table: "transfer_orders",
                column: "ToLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_of_analysis_samples_SampleId",
                table: "certificates_of_analysis",
                column: "SampleId",
                principalTable: "samples",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contract_reviews_rfqs_RFQId",
                table: "contract_reviews",
                column: "RFQId",
                principalTable: "rfqs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_material_types_flows_FlowId",
                table: "material_types",
                column: "FlowId",
                principalTable: "flows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_uoms_locations_LocationId",
                table: "uoms",
                column: "LocationId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_uoms_products_ProductId",
                table: "uoms",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_uoms_shelves_ShelfId_LocationId",
                table: "uoms",
                columns: new[] { "ShelfId", "LocationId" },
                principalTable: "shelves",
                principalColumns: new[] { "Id", "LocationId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_uoms_uoms_ParentUomId",
                table: "uoms",
                column: "ParentUomId",
                principalTable: "uoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_certificates_of_analysis_samples_SampleId",
                table: "certificates_of_analysis");

            migrationBuilder.DropForeignKey(
                name: "FK_contract_reviews_rfqs_RFQId",
                table: "contract_reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_material_types_flows_FlowId",
                table: "material_types");

            migrationBuilder.DropForeignKey(
                name: "FK_uoms_locations_LocationId",
                table: "uoms");

            migrationBuilder.DropForeignKey(
                name: "FK_uoms_products_ProductId",
                table: "uoms");

            migrationBuilder.DropForeignKey(
                name: "FK_uoms_shelves_ShelfId_LocationId",
                table: "uoms");

            migrationBuilder.DropForeignKey(
                name: "FK_uoms_uoms_ParentUomId",
                table: "uoms");

            migrationBuilder.DropTable(
                name: "flow_steps");

            migrationBuilder.DropTable(
                name: "memo_rw_details");

            migrationBuilder.DropTable(
                name: "preparing_orders");

            migrationBuilder.DropTable(
                name: "release_orders");

            migrationBuilder.DropTable(
                name: "rfqs");

            migrationBuilder.DropTable(
                name: "samples");

            migrationBuilder.DropTable(
                name: "stock_moves");

            migrationBuilder.DropTable(
                name: "transfer_order_lines");

            migrationBuilder.DropTable(
                name: "flows");

            migrationBuilder.DropTable(
                name: "memo_rw_in");

            migrationBuilder.DropTable(
                name: "shelves");

            migrationBuilder.DropTable(
                name: "transfer_orders");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "inventories");

            migrationBuilder.DropIndex(
                name: "ix_uoms_location_material",
                table: "uoms");

            migrationBuilder.DropIndex(
                name: "ix_uoms_location_product",
                table: "uoms");

            migrationBuilder.DropIndex(
                name: "ix_uoms_parent_uom",
                table: "uoms");

            migrationBuilder.DropIndex(
                name: "IX_uoms_ProductId",
                table: "uoms");

            migrationBuilder.DropIndex(
                name: "IX_uoms_ShelfId_LocationId",
                table: "uoms");

            migrationBuilder.DropCheckConstraint(
                name: "ck_uoms_kind_link",
                table: "uoms");

            migrationBuilder.DropCheckConstraint(
                name: "ck_uoms_quantity",
                table: "uoms");

            migrationBuilder.DropIndex(
                name: "IX_material_types_FlowId",
                table: "material_types");

            migrationBuilder.DropCheckConstraint(
                name: "ck_certificates_of_analysis_rejection",
                table: "certificates_of_analysis");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "uoms");

            migrationBuilder.DropColumn(
                name: "ParentUomId",
                table: "uoms");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "uoms");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "uoms");

            migrationBuilder.DropColumn(
                name: "kind",
                table: "uoms");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "material_types");

            migrationBuilder.DropSequence(
                name: "memo_rw_in_number_seq");

            migrationBuilder.DropSequence(
                name: "uom_material_seq");

            migrationBuilder.DropSequence(
                name: "uom_product_seq");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "uoms",
                newName: "quantity_work_order");

            migrationBuilder.RenameColumn(
                name: "RFQId",
                table: "contract_reviews",
                newName: "MaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_contract_reviews_RFQId",
                table: "contract_reviews",
                newName: "IX_contract_reviews_MaterialId");

            migrationBuilder.RenameColumn(
                name: "SampleId",
                table: "certificates_of_analysis",
                newName: "SupplierSampleId");

            migrationBuilder.RenameIndex(
                name: "IX_certificates_of_analysis_SampleId",
                table: "certificates_of_analysis",
                newName: "IX_certificates_of_analysis_SupplierSampleId");

            migrationBuilder.AlterColumn<int>(
                name: "MaterialId",
                table: "uoms",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity_in",
                table: "uoms",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "quantity_out",
                table: "uoms",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "supplier_samples",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractReviewId = table.Column<int>(type: "int", nullable: false),
                    delivery_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.AddCheckConstraint(
                name: "ck_certificates_of_analysis_rejection",
                table: "certificates_of_analysis",
                sql: "([result] = 1 AND [rejection_reason] IS NULL) OR ([result] = 2 AND [rejection_reason] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_samples_ContractReviewId",
                table: "supplier_samples",
                column: "ContractReviewId");

            migrationBuilder.CreateIndex(
                name: "ix_supplier_samples_number",
                table: "supplier_samples",
                column: "number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_certificates_of_analysis_supplier_samples_SupplierSampleId",
                table: "certificates_of_analysis",
                column: "SupplierSampleId",
                principalTable: "supplier_samples",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contract_reviews_materials_MaterialId",
                table: "contract_reviews",
                column: "MaterialId",
                principalTable: "materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
