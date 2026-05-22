using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.LeaveMasters;
using NetcoreHRIS.Modules.LeaveMasters.Dtos;
using NetcoreHRIS.Modules.LeaveMasters.Validators;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Unit.LeaveMasters;

public class LeaveMastersServiceTests
{
    [Fact]
    public async Task Create_ValidRequest_ReturnsLeaveMasterDto()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveMastersService(db);

        var result = await svc.CreateAsync(new CreateLeaveMasterRequest("Marriage Leave", "ML", 3), CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Marriage Leave");
        result.Code.Should().Be("ML");
        result.QuotaDays.Should().Be(3);
    }

    [Fact]
    public async Task Create_DuplicateCode_ThrowsConflict()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveMastersService(db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            svc.CreateAsync(new CreateLeaveMasterRequest("Annual Leave Copy", "AL", 5), CancellationToken.None));
    }

    [Fact]
    public async Task GetAll_ReturnsSeededLeaveMasters()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveMastersService(db);

        var result = await svc.GetAllAsync(new ListLeaveMasterQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.Code == "AL");
        result.Items.Should().Contain(x => x.Code == "SL");
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsLeaveMaster()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveMastersService(db);

        var result = await svc.GetByIdAsync(EntityBuilder.AnnualLeaveId, CancellationToken.None);

        result.Id.Should().Be(EntityBuilder.AnnualLeaveId);
        result.Code.Should().Be("AL");
    }

    [Fact]
    public async Task Update_ChangesNameAndQuota()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveMastersService(db);

        var result = await svc.UpdateAsync(EntityBuilder.AnnualLeaveId,
            new UpdateLeaveMasterRequest("Updated Leave", null, 14, false), CancellationToken.None);

        result.Name.Should().Be("Updated Leave");
        result.QuotaDays.Should().Be(14);
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ExistingLeaveMaster_SoftDeletesRecord()
    {
        await using var db = DbContextFactory.Create();
        await EntityBuilder.SeedDefaultDataAsync(db);
        var svc = new LeaveMastersService(db);

        await svc.DeleteAsync(EntityBuilder.SickLeaveId, CancellationToken.None);

        var leave = await db.LeaveMasters.IgnoreQueryFilters()
            .FirstAsync(x => x.Id == EntityBuilder.SickLeaveId);

        leave.IsDeleted.Should().BeTrue();
        leave.DeletedAt.Should().NotBeNull();
    }
}

public class LeaveMasterValidatorTests
{
    private readonly CreateLeaveMasterRequestValidator _createValidator = new();
    private readonly UpdateLeaveMasterRequestValidator _updateValidator = new();

    [Fact]
    public void Create_ValidRequest_PassesValidation()
    {
        var result = _createValidator.TestValidate(new CreateLeaveMasterRequest("Marriage Leave", "ML", 3));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_EmptyName_FailsValidation()
    {
        var result = _createValidator.TestValidate(new CreateLeaveMasterRequest("", "ML", 3));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Create_EmptyCode_FailsValidation()
    {
        var result = _createValidator.TestValidate(new CreateLeaveMasterRequest("Marriage Leave", "", 3));
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Update_NullFields_PassesValidation()
    {
        var result = _updateValidator.TestValidate(new UpdateLeaveMasterRequest(null, null, null, null));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_TooLongCode_FailsValidation()
    {
        var code = new string('A', 51);
        var result = _updateValidator.TestValidate(new UpdateLeaveMasterRequest(null, code, null, null));
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }
}
