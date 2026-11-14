using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace neotonic.Migrations
{
    /// <inheritdoc />
    public partial class HRStuff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employee_feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feedback_date = table.Column<DateOnly>(type: "date", nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: false),
                    worker_id = table.Column<int>(type: "int", nullable: true),
                    job_title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employee_feedbacks_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employee_feedbacks_workers_worker_id",
                        column: x => x.worker_id,
                        principalTable: "workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "employee_feedback_answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_feedback_id = table.Column<int>(type: "int", nullable: false),
                    question_number = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_feedback_answers", x => x.Id);
                    table.CheckConstraint("ck_feedback_answers_question", "[question_number] BETWEEN 1 AND 10");
                    table.CheckConstraint("ck_feedback_answers_score", "[score] IN (2, 4, 6, 8, 10)");
                    table.ForeignKey(
                        name: "FK_employee_feedback_answers_employee_feedbacks_employee_feedback_id",
                        column: x => x.employee_feedback_id,
                        principalTable: "employee_feedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employee_feedback_answers_employee_feedback_id_question_number",
                table: "employee_feedback_answers",
                columns: new[] { "employee_feedback_id", "question_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_feedbacks_department_id",
                table: "employee_feedbacks",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_feedbacks_feedback_date",
                table: "employee_feedbacks",
                column: "feedback_date");

            migrationBuilder.CreateIndex(
                name: "IX_employee_feedbacks_worker_id",
                table: "employee_feedbacks",
                column: "worker_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_feedback_answers");

            migrationBuilder.DropTable(
                name: "employee_feedbacks");
        }
    }
}
