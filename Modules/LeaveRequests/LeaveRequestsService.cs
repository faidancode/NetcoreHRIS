using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Common.Extensions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Data;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.LeaveRequests.Dtos;

namespace NetcoreHRIS.Modules.LeaveRequests;

public interface ILeaveRequestsService
{
    Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestRequest request, CancellationToken ct);
    Task<PagedResult<LeaveRequestDto>> GetAllAsync(ListLeaveRequestQuery query, CancellationToken ct);
    Task<LeaveRequestDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<LeaveRequestDto> UpdateAsync(Guid id, UpdateLeaveRequestRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public class LeaveRequestsService : ILeaveRequestsService
{
    private readonly AppDbContext _db;

    public LeaveRequestsService(AppDbContext db) => _db = db;

    public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequestRequest request, CancellationToken ct)
    {
        if (!await _db.Employees.AnyAsync(x => x.Id == request.EmployeeId, ct))
            throw new NotFoundException("Employee", request.EmployeeId);

        if (!await _db.LeaveMasters.AnyAsync(x => x.Id == request.LeaveId, ct))
            throw new NotFoundException("LeaveMaster", request.LeaveId);

        var requestNo = await GenerateRequestNoAsync(ct);

        var entity = new LeaveRequest
        {
            RequestNo = requestNo,
            EmployeeId = request.EmployeeId,
            LeaveMasterId = request.LeaveId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Reason = request.Reason,
            AttachmentPath = request.AttachmentPath
        };

        _db.LeaveRequests.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<PagedResult<LeaveRequestDto>> GetAllAsync(ListLeaveRequestQuery query, CancellationToken ct)
    {
        var term = (query.Search ?? query.Q)?.Trim();
        var dbQuery = _db.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveMaster)
            .AsQueryable();

        if (query.EmployeeId.HasValue)
            dbQuery = dbQuery.Where(x => x.EmployeeId == query.EmployeeId.Value);

        if (query.LeaveId.HasValue)
            dbQuery = dbQuery.Where(x => x.LeaveMasterId == query.LeaveId.Value);

        if (query.FromDate.HasValue)
            dbQuery = dbQuery.Where(x => x.FromDate >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            dbQuery = dbQuery.Where(x => x.ToDate <= query.ToDate.Value);

        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{term}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.ILike(x.RequestNo, pattern) ||
                EF.Functions.ILike(x.Employee.FullName, pattern) ||
                EF.Functions.ILike(x.Employee.Nip, pattern) ||
                EF.Functions.ILike(x.LeaveMaster.Name, pattern) ||
                EF.Functions.ILike(x.LeaveMaster.Code, pattern) ||
                EF.Functions.ILike(x.Reason, pattern));
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var limit = query.Limit < 1 ? 10 : Math.Min(query.Limit, 100);
        dbQuery = ApplySorting(dbQuery, query.Sort);

        var total = await dbQuery.CountAsync(ct);
        var items = await dbQuery
            .ApplyPagination(page, limit)
            .Select(x => new LeaveRequestDto(
                x.Id,
                x.RequestNo,
                x.EmployeeId,
                x.Employee.FullName,
                x.LeaveMasterId,
                x.LeaveMaster.Name,
                x.FromDate,
                x.ToDate,
                x.Reason,
                x.AttachmentPath,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<LeaveRequestDto>
        {
            Items = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<LeaveRequestDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.LeaveRequests
            .Include(x => x.Employee)
            .Include(x => x.LeaveMaster)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveRequest", id);

        return new LeaveRequestDto(
            entity.Id,
            entity.RequestNo,
            entity.EmployeeId,
            entity.Employee.FullName,
            entity.LeaveMasterId,
            entity.LeaveMaster.Name,
            entity.FromDate,
            entity.ToDate,
            entity.Reason,
            entity.AttachmentPath,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public async Task<LeaveRequestDto> UpdateAsync(Guid id, UpdateLeaveRequestRequest request, CancellationToken ct)
    {
        var entity = await _db.LeaveRequests
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveRequest", id);

        var employeeId = request.EmployeeId ?? entity.EmployeeId;
        var leaveId = request.LeaveId ?? entity.LeaveMasterId;
        var fromDate = request.FromDate ?? entity.FromDate;
        var toDate = request.ToDate ?? entity.ToDate;

        if (!await _db.Employees.AnyAsync(x => x.Id == employeeId, ct))
            throw new NotFoundException("Employee", employeeId);

        if (!await _db.LeaveMasters.AnyAsync(x => x.Id == leaveId, ct))
            throw new NotFoundException("LeaveMaster", leaveId);

        if (toDate < fromDate)
            throw new AppException("ToDate must be greater than or equal to FromDate.", 400);

        entity.EmployeeId = employeeId;
        entity.LeaveMasterId = leaveId;
        entity.FromDate = fromDate;
        entity.ToDate = toDate;

        if (request.Reason != null)
            entity.Reason = request.Reason;

        if (request.AttachmentPath != null)
            entity.AttachmentPath = request.AttachmentPath;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.LeaveRequests
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveRequest", id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<string> GenerateRequestNoAsync(CancellationToken ct)
    {
        var value = $"LR-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..40];
        while (await _db.LeaveRequests.AnyAsync(x => x.RequestNo == value, ct))
        {
            value = $"LR-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..40];
        }

        return value;
    }

    private static IQueryable<LeaveRequest> ApplySorting(IQueryable<LeaveRequest> query, string? sort)
    {
        return sort switch
        {
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            "fromDate:asc" => query.OrderBy(x => x.FromDate),
            "fromDate:desc" => query.OrderByDescending(x => x.FromDate),
            "toDate:asc" => query.OrderBy(x => x.ToDate),
            "toDate:desc" => query.OrderByDescending(x => x.ToDate),
            "requestNo:asc" => query.OrderBy(x => x.RequestNo),
            "requestNo:desc" => query.OrderByDescending(x => x.RequestNo),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}
