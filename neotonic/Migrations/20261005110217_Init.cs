using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "box_paths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_box_paths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_en = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    department_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lab_equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    job_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    job_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    manufacturer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    equalizing_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lab_equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "material_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_en = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "step_types",
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
                    table.PrimaryKey("PK_step_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    supplier_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    contact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditLogId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogRoles_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "machines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    machine_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_machines_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "scales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    scale_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_scales_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "annual_plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    MaterialTypeId = table.Column<int>(type: "int", nullable: false),
                    annual_plan_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    annual_statuses = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annual_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_annual_plans_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_annual_plans_material_types_MaterialTypeId",
                        column: x => x.MaterialTypeId,
                        principalTable: "material_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    material_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    accounting_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    min = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    max = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    MaterialTypeId = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_serialized = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.Id);
                    table.CheckConstraint("ck_materials_min_max", "[min] >= 0 AND [max] >= [min]");
                    table.ForeignKey(
                        name: "FK_materials_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_materials_material_types_MaterialTypeId",
                        column: x => x.MaterialTypeId,
                        principalTable: "material_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "protocols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    protocol_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    StepTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_protocols_step_types_StepTypeId",
                        column: x => x.StepTypeId,
                        principalTable: "step_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    step_number = table.Column<int>(type: "int", nullable: false),
                    step_name_en = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    step_name_ar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BoxPathId = table.Column<int>(type: "int", nullable: true),
                    color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StepTypeId = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_steps_box_paths_BoxPathId",
                        column: x => x.BoxPathId,
                        principalTable: "box_paths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_steps_step_types_StepTypeId",
                        column: x => x.StepTypeId,
                        principalTable: "step_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "annual_plan_details",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnualPlanId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annual_plan_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_annual_plan_details_annual_plans_AnnualPlanId",
                        column: x => x.AnnualPlanId,
                        principalTable: "annual_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_annual_plan_details_materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "protocol_steps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolId = table.Column<int>(type: "int", nullable: false),
                    StepId = table.Column<int>(type: "int", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_protocol_steps_protocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "protocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_protocol_steps_steps_StepId",
                        column: x => x.StepId,
                        principalTable: "steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_annual_plan_details_AnnualPlanId",
                table: "annual_plan_details",
                column: "AnnualPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_annual_plan_details_MaterialId",
                table: "annual_plan_details",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_annual_plans_DepartmentId",
                table: "annual_plans",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_annual_plans_MaterialTypeId",
                table: "annual_plans",
                column: "MaterialTypeId");

            migrationBuilder.CreateIndex(
                name: "ix_annual_plans_number",
                table: "annual_plans",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogRoles_AuditLogId",
                table: "AuditLogRoles",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogRoles_RoleName",
                table: "AuditLogRoles",
                column: "RoleName");

            migrationBuilder.CreateIndex(
                name: "IX_box_paths_code",
                table: "box_paths",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_name_ar",
                table: "categories",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_name_en",
                table: "categories",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_department_number",
                table: "departments",
                column: "department_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_name_ar",
                table: "departments",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_name_en",
                table: "departments",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_machines_DepartmentId",
                table: "machines",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "ix_machines_machine_number",
                table: "machines",
                column: "machine_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_machines_name_ar",
                table: "machines",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_machines_name_en",
                table: "machines",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_types_name_ar",
                table: "material_types",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_material_types_name_en",
                table: "material_types",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_materials_accounting_code",
                table: "materials",
                column: "accounting_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials_CategoryId",
                table: "materials",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "ix_materials_material_number",
                table: "materials",
                column: "material_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials_MaterialTypeId",
                table: "materials",
                column: "MaterialTypeId");

            migrationBuilder.CreateIndex(
                name: "ix_materials_name_ar",
                table: "materials",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_materials_name_en",
                table: "materials",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocol_steps_protocol_order",
                table: "protocol_steps",
                columns: new[] { "ProtocolId", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_protocol_steps_StepId",
                table: "protocol_steps",
                column: "StepId");

            migrationBuilder.CreateIndex(
                name: "ix_protocols_name_ar",
                table: "protocols",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocols_name_en",
                table: "protocols",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_protocols_protocol_number",
                table: "protocols",
                column: "protocol_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_protocols_StepTypeId",
                table: "protocols",
                column: "StepTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_scales_DepartmentId",
                table: "scales",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "ix_scales_name_ar",
                table: "scales",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scales_name_en",
                table: "scales",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scales_scale_number",
                table: "scales",
                column: "scale_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_step_types_name_ar",
                table: "step_types",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_step_types_name_en",
                table: "step_types",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_steps_BoxPathId",
                table: "steps",
                column: "BoxPathId");

            migrationBuilder.CreateIndex(
                name: "ix_steps_step_name_ar",
                table: "steps",
                column: "step_name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_steps_step_name_en",
                table: "steps",
                column: "step_name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_steps_step_number",
                table: "steps",
                column: "step_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_steps_StepTypeId",
                table: "steps",
                column: "StepTypeId");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_name_ar",
                table: "suppliers",
                column: "name_ar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_name_en",
                table: "suppliers",
                column: "name_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_supplier_number",
                table: "suppliers",
                column: "supplier_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "annual_plan_details");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogRoles");

            migrationBuilder.DropTable(
                name: "lab_equipments");

            migrationBuilder.DropTable(
                name: "machines");

            migrationBuilder.DropTable(
                name: "protocol_steps");

            migrationBuilder.DropTable(
                name: "scales");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "annual_plans");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "protocols");

            migrationBuilder.DropTable(
                name: "steps");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "material_types");

            migrationBuilder.DropTable(
                name: "box_paths");

            migrationBuilder.DropTable(
                name: "step_types");
        }
    }
}
