using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class CleaningRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clothes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clothes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "scale_equalizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    equalization_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScaleId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scale_equalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scale_equalizations_scales_ScaleId",
                        column: x => x.ScaleId,
                        principalTable: "scales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_scale_equalizations_workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cleaning_requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    cleaning_date_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ProtocolId = table.Column<int>(type: "int", nullable: false),
                    cleaning_type = table.Column<int>(type: "int", nullable: false),
                    TargetDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ScaleId = table.Column<int>(type: "int", nullable: true),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    ClotheId = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cleaning_requests", x => x.Id);
                    table.CheckConstraint("ck_cleaning_requests_target", "([cleaning_type] = 1 AND [TargetDepartmentId] IS NOT NULL AND [ScaleId] IS NULL AND [MachineId] IS NULL AND [ClotheId] IS NULL) OR ([cleaning_type] = 2 AND [ScaleId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [MachineId] IS NULL AND [ClotheId] IS NULL) OR ([cleaning_type] = 3 AND [MachineId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [ScaleId] IS NULL AND [ClotheId] IS NULL) OR ([cleaning_type] = 4 AND [ClotheId] IS NOT NULL AND [TargetDepartmentId] IS NULL AND [ScaleId] IS NULL AND [MachineId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_cleaning_requests_clothes_ClotheId",
                        column: x => x.ClotheId,
                        principalTable: "clothes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cleaning_requests_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cleaning_requests_departments_TargetDepartmentId",
                        column: x => x.TargetDepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cleaning_requests_machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cleaning_requests_protocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "protocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cleaning_requests_scales_ScaleId",
                        column: x => x.ScaleId,
                        principalTable: "scales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cleaning_requests_ClotheId",
                table: "cleaning_requests",
                column: "ClotheId");

            migrationBuilder.CreateIndex(
                name: "IX_cleaning_requests_DepartmentId",
                table: "cleaning_requests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_cleaning_requests_MachineId",
                table: "cleaning_requests",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "ix_cleaning_requests_number",
                table: "cleaning_requests",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cleaning_requests_ProtocolId",
                table: "cleaning_requests",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_cleaning_requests_ScaleId",
                table: "cleaning_requests",
                column: "ScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_cleaning_requests_TargetDepartmentId",
                table: "cleaning_requests",
                column: "TargetDepartmentId");

            migrationBuilder.CreateIndex(
                name: "ix_scale_equalizations_number",
                table: "scale_equalizations",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scale_equalizations_ScaleId",
                table: "scale_equalizations",
                column: "ScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_scale_equalizations_WorkerId",
                table: "scale_equalizations",
                column: "WorkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cleaning_requests");

            migrationBuilder.DropTable(
                name: "scale_equalizations");

            migrationBuilder.DropTable(
                name: "clothes");
        }
    }
}
