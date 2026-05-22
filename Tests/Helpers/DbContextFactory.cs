using NetcoreHRIS.Entities;
using NetcoreHRIS.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace NetcoreHRIS.Tests.Helpers;

/// <summary>
/// Creates a fresh in-memory AppDbContext for each test.
/// Uses a unique database name so tests are fully isolated.
/// </summary>
public static class DbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}

/// <summary>
/// Fluent builder helpers that produce valid, saved seed entities.
/// All IDs are deterministic so tests can reference them.
/// </summary>
public static class EntityBuilder
{
    // ── Well-known IDs ────────────────────────────────────────────────────────
    public static readonly Guid AdminRoleId = new("11111111-0000-0000-0000-000000000001");
    public static readonly Guid ViewerRoleId = new("11111111-0000-0000-0000-000000000002");
    public static readonly Guid AdminUserId = new("22222222-0000-0000-0000-000000000001");
    public static readonly Guid RegularUserId = new("22222222-0000-0000-0000-000000000002");
    public static readonly Guid ManagerUserId = new("22222222-0000-0000-0000-000000000003");
    public static readonly Guid ReadDepartmentPermId = new("66666666-0000-0000-0000-000000000010");
    public static readonly Guid CreateDepartmentPermId = new("66666666-0000-0000-0000-000000000011");
    public static readonly Guid UpdateDepartmentPermId = new("66666666-0000-0000-0000-000000000012");
    public static readonly Guid DeleteDepartmentPermId = new("66666666-0000-0000-0000-000000000013");
    public static readonly Guid ReadEmployeePermId = new("66666666-0000-0000-0000-000000000014");
    public static readonly Guid CreateEmployeePermId = new("66666666-0000-0000-0000-000000000015");
    public static readonly Guid UpdateEmployeePermId = new("66666666-0000-0000-0000-000000000016");
    public static readonly Guid DeleteEmployeePermId = new("66666666-0000-0000-0000-000000000017");
    public static readonly Guid ReadAttendancePermId = new("66666666-0000-0000-0000-000000000018");
    public static readonly Guid CreateAttendancePermId = new("66666666-0000-0000-0000-000000000019");
    public static readonly Guid UpdateAttendancePermId = new("66666666-0000-0000-0000-000000000020");
    public static readonly Guid DeleteAttendancePermId = new("66666666-0000-0000-0000-000000000021");
    public static readonly Guid ReadLeaveMasterPermId = new("66666666-0000-0000-0000-000000000022");
    public static readonly Guid CreateLeaveMasterPermId = new("66666666-0000-0000-0000-000000000023");
    public static readonly Guid UpdateLeaveMasterPermId = new("66666666-0000-0000-0000-000000000024");
    public static readonly Guid DeleteLeaveMasterPermId = new("66666666-0000-0000-0000-000000000025");
    public static readonly Guid ReadLeaveAllowancePermId = new("66666666-0000-0000-0000-000000000026");
    public static readonly Guid CreateLeaveAllowancePermId = new("66666666-0000-0000-0000-000000000027");
    public static readonly Guid UpdateLeaveAllowancePermId = new("66666666-0000-0000-0000-000000000028");
    public static readonly Guid DeleteLeaveAllowancePermId = new("66666666-0000-0000-0000-000000000029");
    public static readonly Guid ReadLeaveRequestPermId = new("66666666-0000-0000-0000-000000000030");
    public static readonly Guid CreateLeaveRequestPermId = new("66666666-0000-0000-0000-000000000031");
    public static readonly Guid UpdateLeaveRequestPermId = new("66666666-0000-0000-0000-000000000032");
    public static readonly Guid DeleteLeaveRequestPermId = new("66666666-0000-0000-0000-000000000033");
    public static readonly Guid EngineeringId = new("33333333-0000-0000-0000-000000000001");
    public static readonly Guid HrDeptId = new("33333333-0000-0000-0000-000000000002");
    public static readonly Guid SeniorDevId = new("44444444-0000-0000-0000-000000000001");
    public static readonly Guid HrManagerId = new("44444444-0000-0000-0000-000000000002");
    public static readonly Guid Employee1Id = new("55555555-0000-0000-0000-000000000001");
    public static readonly Guid Employee2Id = new("55555555-0000-0000-0000-000000000002");
    public static readonly Guid AnnualLeaveId = new("77777777-0000-0000-0000-000000000001");
    public static readonly Guid SickLeaveId = new("77777777-0000-0000-0000-000000000002");
    public static readonly Guid ManageAllPermId = new("66666666-0000-0000-0000-000000000001");
    public static readonly Guid ReadRolePermId = new("66666666-0000-0000-0000-000000000006");
    public static readonly Guid ReadUserPermId = new("66666666-0000-0000-0000-000000000002");

    // ── Seed a minimal but realistic dataset ─────────────────────────────────
    public static async Task SeedDefaultDataAsync(AppDbContext db)
    {
        // Permissions
        var manageAll = new Permission { Id = ManageAllPermId, Action = "manage", Subject = "all" };
        db.Permissions.AddRange(
            manageAll,
            new Permission { Id = ReadUserPermId, Action = "read", Subject = "User" },
            new Permission { Id = ReadRolePermId, Action = "read", Subject = "Role" },
            new Permission { Id = ReadDepartmentPermId, Action = "read", Subject = "Department" },
            new Permission { Id = CreateDepartmentPermId, Action = "create", Subject = "Department" },
            new Permission { Id = UpdateDepartmentPermId, Action = "update", Subject = "Department" },
            new Permission { Id = DeleteDepartmentPermId, Action = "delete", Subject = "Department" },
            new Permission { Id = ReadEmployeePermId, Action = "read", Subject = "Employee" },
            new Permission { Id = CreateEmployeePermId, Action = "create", Subject = "Employee" },
            new Permission { Id = UpdateEmployeePermId, Action = "update", Subject = "Employee" },
            new Permission { Id = DeleteEmployeePermId, Action = "delete", Subject = "Employee" },
            new Permission { Id = ReadAttendancePermId, Action = "read", Subject = "Attendance" },
            new Permission { Id = CreateAttendancePermId, Action = "create", Subject = "Attendance" },
            new Permission { Id = UpdateAttendancePermId, Action = "update", Subject = "Attendance" },
            new Permission { Id = DeleteAttendancePermId, Action = "delete", Subject = "Attendance" },
            new Permission { Id = ReadLeaveMasterPermId, Action = "read", Subject = "LeaveMaster" },
            new Permission { Id = CreateLeaveMasterPermId, Action = "create", Subject = "LeaveMaster" },
            new Permission { Id = UpdateLeaveMasterPermId, Action = "update", Subject = "LeaveMaster" },
            new Permission { Id = DeleteLeaveMasterPermId, Action = "delete", Subject = "LeaveMaster" },
            new Permission { Id = ReadLeaveAllowancePermId, Action = "read", Subject = "LeaveAllowance" },
            new Permission { Id = CreateLeaveAllowancePermId, Action = "create", Subject = "LeaveAllowance" },
            new Permission { Id = UpdateLeaveAllowancePermId, Action = "update", Subject = "LeaveAllowance" },
            new Permission { Id = DeleteLeaveAllowancePermId, Action = "delete", Subject = "LeaveAllowance" },
            new Permission { Id = ReadLeaveRequestPermId, Action = "read", Subject = "LeaveRequest" },
            new Permission { Id = CreateLeaveRequestPermId, Action = "create", Subject = "LeaveRequest" },
            new Permission { Id = UpdateLeaveRequestPermId, Action = "update", Subject = "LeaveRequest" },
            new Permission { Id = DeleteLeaveRequestPermId, Action = "delete", Subject = "LeaveRequest" }
        );

        // Roles
        var adminRole = new Role { Id = AdminRoleId, Name = "Admin" };
        var viewerRole = new Role { Id = ViewerRoleId, Name = "Viewer" };
        db.Roles.AddRange(adminRole, viewerRole);
        await db.SaveChangesAsync();

        db.RolePermissions.Add(new RolePermission { RoleId = AdminRoleId, PermissionId = ManageAllPermId });

        // Users
        db.Users.AddRange(
            new User
            {
                Id = AdminUserId,
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = AdminRoleId,
                IsActive = true
            },
            new User
            {
                Id = RegularUserId,
                Name = "Regular User",
                Email = "user@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                RoleId = ViewerRoleId,
                IsActive = true
            }
        );

        // Departments
        db.Departments.AddRange(
            new Department { Id = EngineeringId, Name = "Engineering" },
            new Department { Id = HrDeptId, Name = "Human Resources" }
        );

        // Positions
        db.Positions.AddRange(
            new Position { Id = SeniorDevId, Name = "Senior Developer", DepartmentId = EngineeringId },
            new Position { Id = HrManagerId, Name = "HR Manager", DepartmentId = HrDeptId }
        );

        await db.SaveChangesAsync();

        // Employees
        db.Employees.AddRange(
            new Employee
            {
                Id = Employee1Id,
                FullName = "John Doe",
                Nip = "EMP-001",
                Gender = Gender.Male,
                PositionId = SeniorDevId,
                DepartmentId = EngineeringId,
                DateOfJoining = new DateOnly(2020, 1, 15),
                DateOfActivePosition = new DateOnly(2020, 1, 15),
                EmployeeStatus = EmployeeStatus.Active,
                IsActive = true
            },
            new Employee
            {
                Id = Employee2Id,
                FullName = "Jane Smith",
                Nip = "EMP-002",
                Gender = Gender.Female,
                PositionId = HrManagerId,
                DepartmentId = HrDeptId,
                DateOfJoining = new DateOnly(2021, 3, 1),
                EmployeeStatus = EmployeeStatus.Active,
                IsActive = true
            }
        );

        await db.SaveChangesAsync();

        // Initial position histories
        db.PositionHistories.AddRange(
            new PositionHistory
            {
                EmployeeId = Employee1Id,
                PositionId = SeniorDevId,
                StartDate = new DateOnly(2020, 1, 15),
                IsActive = true
            },
            new PositionHistory
            {
                EmployeeId = Employee2Id,
                PositionId = HrManagerId,
                StartDate = new DateOnly(2021, 3, 1),
                IsActive = true
            }
        );

        await db.SaveChangesAsync();

        db.LeaveMasters.AddRange(
            new LeaveMaster
            {
                Id = AnnualLeaveId,
                Name = "Annual Leave",
                Code = "AL",
                QuotaDays = 12,
                IsActive = true
            },
            new LeaveMaster
            {
                Id = SickLeaveId,
                Name = "Sick Leave",
                Code = "SL",
                QuotaDays = 6,
                IsActive = true
            }
        );

        await db.SaveChangesAsync();
    }
}
