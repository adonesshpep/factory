using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class PreparingOrderRemade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_samples_link",
                table: "samples");

            migrationBuilder.DropCheckConstraint(
                name: "ck_locations_inventory_zone",
                table: "locations");

            migrationBuilder.AddColumn<int>(
                name: "VirtualLocationId",
                table: "steps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "preparing_orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "drying_reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreparingOrderId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    StockMoveId = table.Column<int>(type: "int", nullable: false),
                    before_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    after_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    loss_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    humidity_before_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    humidity_after_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    drying_kind = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    temperature = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    SampleId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drying_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_drying_reports_preparing_orders_PreparingOrderId",
                        column: x => x.PreparingOrderId,
                        principalTable: "preparing_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drying_reports_samples_SampleId",
                        column: x => x.SampleId,
                        principalTable: "samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drying_reports_stock_moves_StockMoveId",
                        column: x => x.StockMoveId,
                        principalTable: "stock_moves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drying_reports_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_drying_reports_workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preparing_order_transfer_links",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreparingOrderId = table.Column<int>(type: "int", nullable: false),
                    TransferOrderId = table.Column<int>(type: "int", nullable: false),
                    step_order_index = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preparing_order_transfer_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_preparing_order_transfer_links_preparing_orders_PreparingOrderId",
                        column: x => x.PreparingOrderId,
                        principalTable: "preparing_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_preparing_order_transfer_links_transfer_orders_TransferOrderId",
                        column: x => x.TransferOrderId,
                        principalTable: "transfer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purification_reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreparingOrderId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    StockMoveId = table.Column<int>(type: "int", nullable: false),
                    before_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    after_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    loss_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    theoretical_impurity_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    practical_impurity_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purification_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purification_reports_preparing_orders_PreparingOrderId",
                        column: x => x.PreparingOrderId,
                        principalTable: "preparing_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purification_reports_stock_moves_StockMoveId",
                        column: x => x.StockMoveId,
                        principalTable: "stock_moves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purification_reports_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purification_reports_workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sterilization_reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreparingOrderId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    StockMoveId = table.Column<int>(type: "int", nullable: false),
                    before_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    after_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    loss_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    duration = table.Column<int>(type: "int", nullable: false),
                    wavelength = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    thickness = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    speed = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    temperature = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    humidity_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    SampleId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sterilization_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sterilization_reports_preparing_orders_PreparingOrderId",
                        column: x => x.PreparingOrderId,
                        principalTable: "preparing_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sterilization_reports_samples_SampleId",
                        column: x => x.SampleId,
                        principalTable: "samples",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sterilization_reports_stock_moves_StockMoveId",
                        column: x => x.StockMoveId,
                        principalTable: "stock_moves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sterilization_reports_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sterilization_reports_workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "washing_reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreparingOrderId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    StockMoveId = table.Column<int>(type: "int", nullable: false),
                    before_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    after_weight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    loss_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    impurities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    temperature = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    humidity_percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    user_id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_washing_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_washing_reports_preparing_orders_PreparingOrderId",
                        column: x => x.PreparingOrderId,
                        principalTable: "preparing_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_washing_reports_stock_moves_StockMoveId",
                        column: x => x.StockMoveId,
                        principalTable: "stock_moves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_washing_reports_uoms_UomId",
                        column: x => x.UomId,
                        principalTable: "uoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_washing_reports_workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "locations",
                columns: new[] { "Id", "code", "InventoryId", "is_active", "kind", "name_ar", "name_en", "sort_order", "zone" },
                values: new object[,]
                {
                    { 911, "V-WASH", null, true, 3, "الغسل", "Washing", 11, null },
                    { 912, "V-PURIF", null, true, 3, "التنقية", "Purification", 12, null },
                    { 913, "V-STER", null, true, 3, "التعقيم", "Sterilization", 13, null },
                    { 914, "V-DRY", null, true, 3, "التجفيف", "Drying", 14, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_steps_VirtualLocationId",
                table: "steps",
                column: "VirtualLocationId");

            migrationBuilder.AddCheckConstraint(
                name: "ck_samples_link_delivery",
                table: "samples",
                sql: "sample_type <> 2 OR (UomId IS NOT NULL AND ContractReviewId IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_samples_link_preparing",
                table: "samples",
                sql: "sample_type <> 3 OR (UomId IS NOT NULL AND ContractReviewId IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_samples_link_supplier",
                table: "samples",
                sql: "sample_type <> 1 OR (ContractReviewId IS NOT NULL AND UomId IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_locations_inventory_zone",
                table: "locations",
                sql: "([kind] = 1 AND [InventoryId] IS NOT NULL AND [zone] IS NOT NULL) OR ([kind] = 2 AND [InventoryId] IS NULL AND [zone] IS NULL) OR ([kind] = 3 AND [InventoryId] IS NULL AND [zone] IS NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_drying_reports_prep_order",
                table: "drying_reports",
                column: "PreparingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_drying_reports_SampleId",
                table: "drying_reports",
                column: "SampleId");

            migrationBuilder.CreateIndex(
                name: "IX_drying_reports_StockMoveId",
                table: "drying_reports",
                column: "StockMoveId");

            migrationBuilder.CreateIndex(
                name: "ix_drying_reports_uom",
                table: "drying_reports",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_drying_reports_WorkerId",
                table: "drying_reports",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "ix_prep_link_order_step",
                table: "preparing_order_transfer_links",
                columns: new[] { "PreparingOrderId", "step_order_index" });

            migrationBuilder.CreateIndex(
                name: "ix_prep_link_transfer",
                table: "preparing_order_transfer_links",
                column: "TransferOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_purification_reports_prep_order",
                table: "purification_reports",
                column: "PreparingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_purification_reports_StockMoveId",
                table: "purification_reports",
                column: "StockMoveId");

            migrationBuilder.CreateIndex(
                name: "ix_purification_reports_uom",
                table: "purification_reports",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_purification_reports_WorkerId",
                table: "purification_reports",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "ix_sterilization_reports_prep_order",
                table: "sterilization_reports",
                column: "PreparingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_sterilization_reports_SampleId",
                table: "sterilization_reports",
                column: "SampleId");

            migrationBuilder.CreateIndex(
                name: "IX_sterilization_reports_StockMoveId",
                table: "sterilization_reports",
                column: "StockMoveId");

            migrationBuilder.CreateIndex(
                name: "ix_sterilization_reports_uom",
                table: "sterilization_reports",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_sterilization_reports_WorkerId",
                table: "sterilization_reports",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "ix_washing_reports_prep_order",
                table: "washing_reports",
                column: "PreparingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_washing_reports_StockMoveId",
                table: "washing_reports",
                column: "StockMoveId");

            migrationBuilder.CreateIndex(
                name: "ix_washing_reports_uom",
                table: "washing_reports",
                column: "UomId");

            migrationBuilder.CreateIndex(
                name: "IX_washing_reports_WorkerId",
                table: "washing_reports",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_steps_locations_VirtualLocationId",
                table: "steps",
                column: "VirtualLocationId",
                principalTable: "locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_steps_locations_VirtualLocationId",
                table: "steps");

            migrationBuilder.DropTable(
                name: "drying_reports");

            migrationBuilder.DropTable(
                name: "preparing_order_transfer_links");

            migrationBuilder.DropTable(
                name: "purification_reports");

            migrationBuilder.DropTable(
                name: "sterilization_reports");

            migrationBuilder.DropTable(
                name: "washing_reports");

            migrationBuilder.DropIndex(
                name: "IX_steps_VirtualLocationId",
                table: "steps");

            migrationBuilder.DropCheckConstraint(
                name: "ck_samples_link_delivery",
                table: "samples");

            migrationBuilder.DropCheckConstraint(
                name: "ck_samples_link_preparing",
                table: "samples");

            migrationBuilder.DropCheckConstraint(
                name: "ck_samples_link_supplier",
                table: "samples");

            migrationBuilder.DropCheckConstraint(
                name: "ck_locations_inventory_zone",
                table: "locations");

            migrationBuilder.DeleteData(
                table: "locations",
                keyColumn: "Id",
                keyValue: 911);

            migrationBuilder.DeleteData(
                table: "locations",
                keyColumn: "Id",
                keyValue: 912);

            migrationBuilder.DeleteData(
                table: "locations",
                keyColumn: "Id",
                keyValue: 913);

            migrationBuilder.DeleteData(
                table: "locations",
                keyColumn: "Id",
                keyValue: 914);

            migrationBuilder.DropColumn(
                name: "VirtualLocationId",
                table: "steps");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "preparing_orders");

            migrationBuilder.AddCheckConstraint(
                name: "ck_samples_link",
                table: "samples",
                sql: "([sample_type] = 1 AND [ContractReviewId] IS NOT NULL AND [UomId] IS NULL) OR ([sample_type] = 2 AND [UomId] IS NOT NULL AND [ContractReviewId] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_locations_inventory_zone",
                table: "locations",
                sql: "([kind] = 1 AND [InventoryId] IS NOT NULL AND [zone] IS NOT NULL) OR ([kind] = 2 AND [InventoryId] IS NULL AND [zone] IS NULL)");
        }
    }
}
