using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Common.Extensions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Data;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.LeaveAllowances.Dtos;

namespace NetcoreHRIS.Modules.LeaveAllowances;

public interface ILeaveAllowancesService
{
    Task<LeaveAllowanceDto> CreateAsync(CreateLeaveAllowanceRequest request, CancellationToken ct);
    Task<PagedResult<LeaveAllowanceDto>> GetAllAsync(ListLeaveAllowanceQuery query, CancellationToken ct);
    Task<LeaveAllowanceDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<LeaveAllowanceDto> UpdateAsync(Guid id, UpdateLeaveAllowanceRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public class LeaveAllowancesService : ILeaveAllowancesService
{
    private readonly AppDbContext _db;

    public LeaveAllowancesService(AppDbContext db) => _db = db;

    public async Task<LeaveAllowanceDto> CreateAsync(CreateLeaveAllowanceRequest request, CancellationToken ct)
    {
        if (!await _db.Employees.AnyAsync(x => x.Id == request.EmployeeId, ct))
            throw new NotFoundException("Employee", request.EmployeeId);

        if (!await _db.LeaveMasters.AnyAsync(x => x.Id == request.LeaveId, ct))
            throw new NotFoundException("LeaveMaster", request.LeaveId);

        var exists = await _db.LeaveAllowances.AnyAsync(x =>
            x.EmployeeId == request.EmployeeId &&
            x.LeaveMasterId == request.LeaveId &&
            x.Year == request.Year, ct);

        if (exists)
            throw new ConflictException("Leave allowance already exists for this employee, leave type, and year.");

        var entity = new LeaveAllowance
        {
            EmployeeId = request.EmployeeId,
            LeaveMasterId = request.LeaveId,
            Year = request.Year,
            QuotaDays = request.QuotaDays,
            Notes = request.Notes
        };

        _db.LeaveAllowances.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await MapToDtoAsync(entity.Id, ct);
    }

    public async Task<PagedResult<LeaveAllowanceDto>> GetAllAsync(ListLeaveAllowanceQuery query, CancellationToken ct)
    {
        var term = (query.Search ?? query.Q)?.Trim();
        var dbQuery = _db.LeaveAllowances
            .Include(x => x.Employee)
            .Include(x => x.LeaveMaster)
            .AsQueryable();

        if (query.EmployeeId.HasValue)
            dbQuery = dbQuery.Where(x => x.EmployeeId == query.EmployeeId.Value);

        if (query.LeaveId.HasValue)
            dbQuery = dbQuery.Where(x => x.LeaveMasterId == query.LeaveId.Value);

        if (query.Year.HasValue)
            dbQuery = dbQuery.Where(x => x.Year == query.Year.Value);

        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{term}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.ILike(x.Employee.FullName, pattern) ||
                EF.Functions.ILike(x.Employee.Nip, pattern) ||
                EF.Functions.ILike(x.LeaveMaster.Name, pattern) ||
                EF.Functions.ILike(x.LeaveMaster.Code, pattern));
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var limit = query.Limit < 1 ? 10 : Math.Min(query.Limit, 100);
        dbQuery = ApplySorting(dbQuery, query.Sort);

        var total = await dbQuery.CountAsync(ct);
        var items = await dbQuery
            .ApplyPagination(page, limit)
            .Select(x => new LeaveAllowanceDto(
                x.Id,
                x.EmployeeId,
                x.Employee.FullName,
                x.LeaveMasterId,
                x.LeaveMaster.Name,
                x.Year,
                x.QuotaDays,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<LeaveAllowanceDto>
        {
            Items = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<LeaveAllowanceDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.LeaveAllowances
            .Include(x => x.Employee)
            .Include(x => x.LeaveMaster)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveAllowance", id);

        return new LeaveAllowanceDto(
            entity.Id,
            entity.EmployeeId,
            entity.Employee.FullName,
            entity.LeaveMasterId,
            entity.LeaveMaster.Name,
            entity.Year,
            entity.QuotaDays,
            entity.Notes,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public async Task<LeaveAllowanceDto> UpdateAsync(Guid id, UpdateLeaveAllowanceRequest request, CancellationToken ct)
    {
        var entity = await _db.LeaveAllowances
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveAllowance", id);

        var employeeId = request.EmployeeId ?? entity.EmployeeId;
        var leaveId = request.LeaveId ?? entity.LeaveMasterId;
        var year = request.Year ?? entity.Year;

        if (!await _db.Employees.AnyAsync(x => x.Id == employeeId, ct))
            throw new NotFoundException("Employee", employeeId);

        if (!await _db.LeaveMasters.AnyAsync(x => x.Id == leaveId, ct))
            throw new NotFoundException("LeaveMaster", leaveId);

        if (await _db.LeaveAllowances.AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.LeaveMasterId == leaveId &&
                x.Year == year &&
                x.Id != id, ct))
        {
            throw new ConflictException("Leave allowance already exists for this employee, leave type, and year.");
        }

        entity.EmployeeId = employeeId;
        entity.LeaveMasterId = leaveId;
        entity.Year = year;

        if (request.QuotaDays.HasValue)
            entity.QuotaDays = request.QuotaDays.Value;

        if (request.Notes != null)
            entity.Notes = request.Notes;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.LeaveAllowances
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveAllowance", id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<LeaveAllowanceDto> MapToDtoAsync(Guid id, CancellationToken ct)
        => await GetByIdAsync(id, ct);

    private static IQueryable<LeaveAllowance> ApplySorting(IQueryable<LeaveAllowance> query, string? sort)
    {
        return sort switch
        {
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            "year:asc" => query.OrderBy(x => x.Year),
            "year:desc" => query.OrderByDescending(x => x.Year),
            "quotaDays:asc" => query.OrderBy(x => x.QuotaDays),
            "quotaDays:desc" => query.OrderByDescending(x => x.QuotaDays),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}
