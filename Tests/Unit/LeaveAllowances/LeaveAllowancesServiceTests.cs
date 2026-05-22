using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Modules.LeaveAllowances;
using NetcoreHRIS.Modules.LeaveAllowances.Dtos;
using NetcoreHRIS.Modules.LeaveAllowances.Validators;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Unit.LeaveAllowances;

public class LeaveAllowancesServiceTests
{
    [Fact]
    public async Task Create_ValidRequest_ReturnsLeaveAllowanceDto()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveAllowancesService(db);

        var result = await svc.CreateAsync(
            new CreateLeaveAllowanceRequest(EntityBuilder.Employee1Id, EntityBuilder.AnnualLeaveId, 2026, 12, "Annual quota"),
            CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.EmployeeId.Should().Be(EntityBuilder.Employee1Id);
        result.LeaveId.Should().Be(EntityBuilder.AnnualLeaveId);
        result.Year.Should().Be(2026);
        result.QuotaDays.Should().Be(12);
    }

    [Fact]
    public async Task Create_DuplicateCombination_ThrowsConflict()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        db.LeaveAllowances.Add(new Entities.LeaveAllowance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            LeaveMasterId = EntityBuilder.AnnualLeaveId,
            Year = 2026,
            QuotaDays = 12
        });
        await db.SaveChangesAsync();

        var svc = new LeaveAllowancesService(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.CreateAsync(new CreateLeaveAllowanceRequest(EntityBuilder.Employee1Id, EntityBuilder.AnnualLeaveId, 2026, 10), CancellationToken.None));
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsLeaveAllowance()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var allowance = new Entities.LeaveAllowance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            LeaveMasterId = EntityBuilder.AnnualLeaveId,
            Year = 2026,
            QuotaDays = 12,
            Notes = "Seeded"
        };
        db.LeaveAllowances.Add(allowance);
        await db.SaveChangesAsync();

        var svc = new LeaveAllowancesService(db);
        var result = await svc.GetByIdAsync(allowance.Id, CancellationToken.None);

        result.Id.Should().Be(allowance.Id);
        result.EmployeeId.Should().Be(EntityBuilder.Employee1Id);
    }

    [Fact]
    public async Task Update_ChangesQuotaAndNotes()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var allowance = new Entities.LeaveAllowance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            LeaveMasterId = EntityBuilder.AnnualLeaveId,
            Year = 2026,
            QuotaDays = 12
        };
        db.LeaveAllowances.Add(allowance);
        await db.SaveChangesAsync();

        var svc = new LeaveAllowancesService(db);
        var result = await svc.UpdateAsync(allowance.Id,
            new UpdateLeaveAllowanceRequest(null, null, null, 14, "Updated note"), CancellationToken.None);

        result.QuotaDays.Should().Be(14);
        result.Notes.Should().Be("Updated note");
    }

    [Fact]
    public async Task Delete_SoftDeletesLeaveAllowance()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var allowance = new Entities.LeaveAllowance
        {
            EmployeeId = EntityBuilder.Employee1Id,
            LeaveMasterId = EntityBuilder.AnnualLeaveId,
            Year = 2026,
            QuotaDays = 12
        };
        db.LeaveAllowances.Add(allowance);
        await db.SaveChangesAsync();

        var svc = new LeaveAllowancesService(db);
        await svc.DeleteAsync(allowance.Id, CancellationToken.None);

        var deleted = await db.LeaveAllowances.IgnoreQueryFilters().FirstAsync(x => x.Id == allowance.Id);
        deleted.IsDeleted.Should().BeTrue();
    }
}

public class LeaveAllowanceValidatorTests
{
    private readonly CreateLeaveAllowanceRequestValidator _createValidator = new();
    private readonly UpdateLeaveAllowanceRequestValidator _updateValidator = new();

    [Fact]
    public void Create_ValidRequest_PassesValidation()
    {
        var result = _createValidator.TestValidate(
            new CreateLeaveAllowanceRequest(EntityBuilder.Employee1Id, EntityBuilder.AnnualLeaveId, 2026, 12, "Annual quota"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_EmptyEmployeeId_FailsValidation()
    {
        var result = _createValidator.TestValidate(
            new CreateLeaveAllowanceRequest(Guid.Empty, EntityBuilder.AnnualLeaveId, 2026, 12));
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Update_NullFields_PassesValidation()
    {
        var result = _updateValidator.TestValidate(new UpdateLeaveAllowanceRequest(null, null, null, null, null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_TooLongNotes_FailsValidation()
    {
        var notes = new string('A', 501);
        var result = _updateValidator.TestValidate(new UpdateLeaveAllowanceRequest(null, null, null, null, notes));
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
