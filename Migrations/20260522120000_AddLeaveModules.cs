using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetcoreHRIS.Migrations;

public partial class AddLeaveModules : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "leave_masters",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                quota_days = table.Column<int>(type: "integer", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_leave_masters", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "attendances",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                check_in = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                check_out = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_attendances", x => x.id);
                table.ForeignKey(
                    name: "FK_attendances_employees_employee_id",
                    column: x => x.employee_id,
                    principalTable: "employees",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "leave_allowances",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                leave_master_id = table.Column<Guid>(type: "uuid", nullable: false),
                year = table.Column<int>(type: "integer", nullable: false),
                quota_days = table.Column<int>(type: "integer", nullable: false),
                notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_leave_allowances", x => x.id);
                table.ForeignKey(
                    name: "FK_leave_allowances_employees_employee_id",
                    column: x => x.employee_id,
                    principalTable: "employees",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_leave_allowances_leave_masters_leave_master_id",
                    column: x => x.leave_master_id,
                    principalTable: "leave_masters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "leave_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                request_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                leave_master_id = table.Column<Guid>(type: "uuid", nullable: false),
                from_date = table.Column<DateOnly>(type: "date", nullable: false),
                to_date = table.Column<DateOnly>(type: "date", nullable: false),
                reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                attachment_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_leave_requests", x => x.id);
                table.ForeignKey(
                    name: "FK_leave_requests_employees_employee_id",
                    column: x => x.employee_id,
                    principalTable: "employees",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_leave_requests_leave_masters_leave_master_id",
                    column: x => x.leave_master_id,
                    principalTable: "leave_masters",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_leave_masters_name",
            table: "leave_masters",
            column: "name",
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "IX_leave_masters_code",
            table: "leave_masters",
            column: "code",
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "IX_attendances_employee_id_date",
            table: "attendances",
            columns: new[] { "employee_id", "date" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "IX_attendances_employee_id",
            table: "attendances",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "IX_leave_allowances_employee_id_leave_master_id_year",
            table: "leave_allowances",
            columns: new[] { "employee_id", "leave_master_id", "year" },
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "IX_leave_allowances_employee_id",
            table: "leave_allowances",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "IX_leave_allowances_leave_master_id",
            table: "leave_allowances",
            column: "leave_master_id");

        migrationBuilder.CreateIndex(
            name: "IX_leave_requests_request_no",
            table: "leave_requests",
            column: "request_no",
            unique: true,
            filter: "is_deleted = false");

        migrationBuilder.CreateIndex(
            name: "IX_leave_requests_employee_id",
            table: "leave_requests",
            column: "employee_id");

        migrationBuilder.CreateIndex(
            name: "IX_leave_requests_leave_master_id",
            table: "leave_requests",
            column: "leave_master_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "attendances");
        migrationBuilder.DropTable(name: "leave_allowances");
        migrationBuilder.DropTable(name: "leave_requests");
        migrationBuilder.DropTable(name: "leave_masters");
    }
}
