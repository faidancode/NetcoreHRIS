using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.Attendances;
using NetcoreHRIS.Modules.Attendances.Dtos;
using NetcoreHRIS.Modules.Attendances.Validators;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Unit.Attendances;

public class AttendancesServiceTests
{
    [Fact]
    public async Task Create_ValidRequest_ReturnsOnTimeStatus()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new AttendancesService(db);

        var result = await svc.CreateAsync(new CreateAttendanceRequest(
            new DateOnly(2026, 5, 22),
            EntityBuilder.Employee1Id,
            new TimeOnly(7, 30),
            new TimeOnly(16, 30)), CancellationToken.None);

        result.Status.Should().Be(nameof(AttendanceStatus.OnTime));
        result.EmployeeId.Should().Be(EntityBuilder.Employee1Id);
    }

    [Fact]
    public async Task Create_LateCheckIn_ReturnsLateStatus()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new AttendancesService(db);

        var result = await svc.CreateAsync(new CreateAttendanceRequest(
            new DateOnly(2026, 5, 22),
            EntityBuilder.Employee1Id,
            new TimeOnly(8, 15),
            new TimeOnly(17, 0)), CancellationToken.None);

        result.Status.Should().Be(nameof(AttendanceStatus.Late));
    }

    [Fact]
    public async Task Create_DuplicateDateForEmployee_ThrowsConflict()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        db.Attendances.Add(new Attendance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            Date = new DateOnly(2026, 5, 22),
            CheckIn = new TimeOnly(7, 30),
            CheckOut = new TimeOnly(16, 30),
            Status = AttendanceStatus.OnTime
        });
        await db.SaveChangesAsync();

        var svc = new AttendancesService(db);

        await Assert.ThrowsAsync<NetcoreHRIS.Common.Exceptions.ConflictException>(() =>
            svc.CreateAsync(new CreateAttendanceRequest(
                new DateOnly(2026, 5, 22),
                EntityBuilder.Employee1Id,
                new TimeOnly(8, 15),
                new TimeOnly(17, 0)), CancellationToken.None));
    }

    [Fact]
    public async Task Update_RecalculatesStatus()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var attendance = new Attendance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            Date = new DateOnly(2026, 5, 22),
            CheckIn = new TimeOnly(7, 30),
            CheckOut = new TimeOnly(16, 30),
            Status = AttendanceStatus.OnTime
        };
        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();

        var svc = new AttendancesService(db);
        var result = await svc.UpdateAsync(attendance.Id,
            new UpdateAttendanceRequest(null, null, new TimeOnly(8, 20), new TimeOnly(17, 10)), CancellationToken.None);

        result.Status.Should().Be(nameof(AttendanceStatus.Late));
    }

    [Fact]
    public async Task Delete_SoftDeletesAttendance()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var attendance = new Attendance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            Date = new DateOnly(2026, 5, 22),
            CheckIn = new TimeOnly(7, 30),
            CheckOut = new TimeOnly(16, 30),
            Status = AttendanceStatus.OnTime
        };
        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();

        var svc = new AttendancesService(db);
        await svc.DeleteAsync(attendance.Id, CancellationToken.None);

        var deleted = await db.Attendances.IgnoreQueryFilters().FirstAsync(x => x.Id == attendance.Id);
        deleted.IsDeleted.Should().BeTrue();
    }
}

public class AttendanceValidatorTests
{
    private readonly CreateAttendanceRequestValidator _createValidator = new();
    private readonly UpdateAttendanceRequestValidator _updateValidator = new();

    [Fact]
    public void Create_ValidRequest_PassesValidation()
    {
        var result = _createValidator.TestValidate(new CreateAttendanceRequest(
            new DateOnly(2026, 5, 22),
            EntityBuilder.Employee1Id,
            new TimeOnly(7, 30),
            new TimeOnly(16, 30)));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_CheckOutBeforeCheckIn_FailsValidation()
    {
        var result = _createValidator.TestValidate(new CreateAttendanceRequest(
            new DateOnly(2026, 5, 22),
            EntityBuilder.Employee1Id,
            new TimeOnly(8, 30),
            new TimeOnly(7, 30)));
        result.ShouldHaveValidationErrorFor(x => x.CheckOut);
    }

    [Fact]
    public void Update_NullFields_PassesValidation()
    {
        var result = _updateValidator.TestValidate(new UpdateAttendanceRequest(null, null, null, null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_CheckOutBeforeCheckIn_FailsValidation()
    {
        var result = _updateValidator.TestValidate(new UpdateAttendanceRequest(
            null,
            null,
            new TimeOnly(8, 30),
            new TimeOnly(7, 30)));
        result.ShouldHaveValidationErrorFor(x => x.CheckOut);
    }
}
