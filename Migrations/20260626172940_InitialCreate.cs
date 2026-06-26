using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_company",
                columns: table => new
                {
                    comp_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    comp_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    comp_ph_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comp_email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    comp_location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    office_start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    office_end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    comp_dummy1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    comp_dummy2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_company", x => x.comp_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_leave_type",
                columns: table => new
                {
                    leave_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    leave_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_leave_type", x => x.leave_type_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_leave_policy",
                columns: table => new
                {
                    policy_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    leave_type_id = table.Column<int>(type: "integer", nullable: false),
                    total_days = table.Column<int>(type: "integer", nullable: false),
                    carry_forward = table.Column<bool>(type: "boolean", nullable: false),
                    carry_duration = table.Column<int>(type: "integer", nullable: true),
                    max_carry_day = table.Column<int>(type: "integer", nullable: true),
                    reset_cycle = table.Column<DateTime>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_leave_policy", x => x.policy_id);
                    table.ForeignKey(
                        name: "FK_tbl_leave_policy_tbl_leave_type_leave_type_id",
                        column: x => x.leave_type_id,
                        principalTable: "tbl_leave_type",
                        principalColumn: "leave_type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_attendance",
                columns: table => new
                {
                    attendance_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    attendance_date = table.Column<DateTime>(type: "date", nullable: false),
                    check_in = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    check_out = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attendance_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_work_hours = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    work_location = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    check_in_mode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attachment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    location_detail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_attendance", x => x.attendance_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_audit_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    performed_user_id = table.Column<int>(type: "integer", nullable: false),
                    target_user_id = table.Column<int>(type: "integer", nullable: true),
                    module_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    action_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_audit_log", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_department",
                columns: table => new
                {
                    dept_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dept_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dept_head_user_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_department", x => x.dept_id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dept_id = table.Column<int>(type: "integer", nullable: false),
                    user_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    nrc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    dob = table.Column<DateTime>(type: "date", nullable: false),
                    married_status = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hired_date = table.Column<DateTime>(type: "date", nullable: false),
                    qualification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    user_ph_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_dummy1 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    user_dummy2 = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_tbl_user_tbl_department_dept_id",
                        column: x => x.dept_id,
                        principalTable: "tbl_department",
                        principalColumn: "dept_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_leave_balance",
                columns: table => new
                {
                    balance_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    leave_type_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    allocated_days = table.Column<int>(type: "integer", nullable: false),
                    used_days = table.Column<int>(type: "integer", nullable: false),
                    remaining_days = table.Column<int>(type: "integer", nullable: false),
                    carried_forward_days = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_leave_balance", x => x.balance_id);
                    table.ForeignKey(
                        name: "FK_tbl_leave_balance_tbl_leave_type_leave_type_id",
                        column: x => x.leave_type_id,
                        principalTable: "tbl_leave_type",
                        principalColumn: "leave_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_leave_balance_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_leave_request",
                columns: table => new
                {
                    leave_request_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    leave_type_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_days = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    attachment = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approved_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    remark = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_leave_request", x => x.leave_request_id);
                    table.ForeignKey(
                        name: "FK_tbl_leave_request_tbl_leave_type_leave_type_id",
                        column: x => x.leave_type_id,
                        principalTable: "tbl_leave_type",
                        principalColumn: "leave_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_leave_request_tbl_user_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tbl_leave_request_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_profile_update_request",
                columns: table => new
                {
                    request_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    new_ph_no = table.Column<string>(type: "text", nullable: true),
                    new_address = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    reviewed_by_user_id = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_profile_update_request", x => x.request_id);
                    table.ForeignKey(
                        name: "FK_tbl_profile_update_request_tbl_user_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id");
                    table.ForeignKey(
                        name: "FK_tbl_profile_update_request_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_public_holiday",
                columns: table => new
                {
                    holiday_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    holiday_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    holiday_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: true),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_public_holiday", x => x.holiday_id);
                    table.ForeignKey(
                        name: "FK_tbl_public_holiday_tbl_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_resignation",
                columns: table => new
                {
                    resignation_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    resignation_date = table.Column<DateTime>(type: "date", nullable: false),
                    resignation_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    resigned_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_resignation", x => x.resignation_id);
                    table.ForeignKey(
                        name: "FK_tbl_resignation_tbl_user_resigned_by_user_id",
                        column: x => x.resigned_by_user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_resignation_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_account",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_first_login = table.Column<bool>(type: "boolean", nullable: false),
                    last_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    password_reset_token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    token_expiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_account", x => x.account_id);
                    table.ForeignKey(
                        name: "FK_tbl_user_account_tbl_role_role_id",
                        column: x => x.role_id,
                        principalTable: "tbl_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_account_tbl_user_user_id",
                        column: x => x.user_id,
                        principalTable: "tbl_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_attendance_user_id",
                table: "tbl_attendance",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_audit_log_performed_user_id",
                table: "tbl_audit_log",
                column: "performed_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_audit_log_target_user_id",
                table: "tbl_audit_log",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_department_dept_head_user_id",
                table: "tbl_department",
                column: "dept_head_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_leave_balance_leave_type_id",
                table: "tbl_leave_balance",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_leave_balance_user_id",
                table: "tbl_leave_balance",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_leave_policy_leave_type_id",
                table: "tbl_leave_policy",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_leave_request_approved_by_user_id",
                table: "tbl_leave_request",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_leave_request_leave_type_id",
                table: "tbl_leave_request",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_leave_request_user_id",
                table: "tbl_leave_request",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_profile_update_request_reviewed_by_user_id",
                table: "tbl_profile_update_request",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_profile_update_request_user_id",
                table: "tbl_profile_update_request",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_public_holiday_created_by_user_id",
                table: "tbl_public_holiday",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_resignation_resigned_by_user_id",
                table: "tbl_resignation",
                column: "resigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_resignation_user_id",
                table: "tbl_resignation",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_dept_id",
                table: "tbl_user",
                column: "dept_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_nrc",
                table: "tbl_user",
                column: "nrc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_user_ph_no",
                table: "tbl_user",
                column: "user_ph_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_account_email",
                table: "tbl_user_account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_account_role_id",
                table: "tbl_user_account",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_account_user_id",
                table: "tbl_user_account",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_attendance_tbl_user_user_id",
                table: "tbl_attendance",
                column: "user_id",
                principalTable: "tbl_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_audit_log_tbl_user_performed_user_id",
                table: "tbl_audit_log",
                column: "performed_user_id",
                principalTable: "tbl_user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_audit_log_tbl_user_target_user_id",
                table: "tbl_audit_log",
                column: "target_user_id",
                principalTable: "tbl_user",
                principalColumn: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_department_tbl_user_dept_head_user_id",
                table: "tbl_department",
                column: "dept_head_user_id",
                principalTable: "tbl_user",
                principalColumn: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_department_tbl_user_dept_head_user_id",
                table: "tbl_department");

            migrationBuilder.DropTable(
                name: "tbl_attendance");

            migrationBuilder.DropTable(
                name: "tbl_audit_log");

            migrationBuilder.DropTable(
                name: "tbl_company");

            migrationBuilder.DropTable(
                name: "tbl_leave_balance");

            migrationBuilder.DropTable(
                name: "tbl_leave_policy");

            migrationBuilder.DropTable(
                name: "tbl_leave_request");

            migrationBuilder.DropTable(
                name: "tbl_profile_update_request");

            migrationBuilder.DropTable(
                name: "tbl_public_holiday");

            migrationBuilder.DropTable(
                name: "tbl_resignation");

            migrationBuilder.DropTable(
                name: "tbl_user_account");

            migrationBuilder.DropTable(
                name: "tbl_leave_type");

            migrationBuilder.DropTable(
                name: "tbl_role");

            migrationBuilder.DropTable(
                name: "tbl_user");

            migrationBuilder.DropTable(
                name: "tbl_department");
        }
    }
}
