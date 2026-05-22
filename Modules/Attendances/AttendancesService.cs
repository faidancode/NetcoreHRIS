using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Common.Extensions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Data;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.Attendances.Dtos;

namespace NetcoreHRIS.Modules.Attendances;

public interface IAttendancesService
{
    Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request, CancellationToken ct);
    Task<PagedResult<AttendanceDto>> GetAllAsync(ListAttendanceQuery query, CancellationToken ct);
    Task<AttendanceDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public class AttendancesService : IAttendancesService
{
    private static readonly TimeOnly StandardStartTime = new(8, 0);
    private readonly AppDbContext _db;

    public AttendancesService(AppDbContext db) => _db = db;

    public async Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request, CancellationToken ct)
    {
        if (!await _db.Employees.AnyAsync(x => x.Id == request.EmployeeId, ct))
            throw new NotFoundException("Employee", request.EmployeeId);

        if (await _db.Attendances.AnyAsync(x => x.EmployeeId == request.EmployeeId && x.Date == request.Date, ct))
            throw new ConflictException("Attendance already exists for this employee on the selected date.");

        var entity = new Attendance
        {
            Date = request.Date,
            EmployeeId = request.EmployeeId,
            CheckIn = request.CheckIn!.Value,
            CheckOut = request.CheckOut,
            Status = ResolveStatus(request.CheckIn!.Value)
        };

        _db.Attendances.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }

    public async Task<PagedResult<AttendanceDto>> GetAllAsync(ListAttendanceQuery query, CancellationToken ct)
    {
        var term = (query.Search ?? query.Q)?.Trim();
        var dbQuery = _db.Attendances
            .Include(x => x.Employee)
            .AsQueryable();

        if (query.EmployeeId.HasValue)
            dbQuery = dbQuery.Where(x => x.EmployeeId == query.EmployeeId.Value);

        if (query.Date.HasValue)
            dbQuery = dbQuery.Where(x => x.Date == query.Date.Value);

        if (query.FromDate.HasValue)
            dbQuery = dbQuery.Where(x => x.Date >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            dbQuery = dbQuery.Where(x => x.Date <= query.ToDate.Value);

        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{term}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.ILike(x.Employee.FullName, pattern) ||
                EF.Functions.ILike(x.Employee.Nip, pattern));
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var limit = query.Limit < 1 ? 10 : Math.Min(query.Limit, 100);
        dbQuery = ApplySorting(dbQuery, query.Sort);

        var total = await dbQuery.CountAsync(ct);
        var items = await dbQuery
            .ApplyPagination(page, limit)
            .Select(x => new AttendanceDto(
                x.Id,
                x.Date,
                x.EmployeeId,
                x.Employee.FullName,
                x.Employee.Nip,
                x.CheckIn,
                x.CheckOut,
                x.Status.ToString(),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResult<AttendanceDto>
        {
            Items = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<AttendanceDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Attendances
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Attendance", id);

        return new AttendanceDto(
            entity.Id,
            entity.Date,
            entity.EmployeeId,
            entity.Employee.FullName,
            entity.Employee.Nip,
            entity.CheckIn,
            entity.CheckOut,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public async Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceRequest request, CancellationToken ct)
    {
        var entity = await _db.Attendances
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Attendance", id);

        var employeeId = request.EmployeeId ?? entity.EmployeeId;
        var date = request.Date ?? entity.Date;
        var checkIn = request.CheckIn ?? entity.CheckIn;
        var checkOut = request.CheckOut ?? entity.CheckOut;

        if (!await _db.Employees.AnyAsync(x => x.Id == employeeId, ct))
            throw new NotFoundException("Employee", employeeId);

        if (await _db.Attendances.AnyAsync(x => x.EmployeeId == employeeId && x.Date == date && x.Id != id, ct))
            throw new ConflictException("Attendance already exists for this employee on the selected date.");

        if (checkOut.HasValue && checkOut.Value < checkIn)
            throw new AppException("CheckOut must be greater than or equal to CheckIn.", 400);

        entity.EmployeeId = employeeId;
        entity.Date = date;
        entity.CheckIn = checkIn;
        entity.CheckOut = checkOut;
        entity.Status = ResolveStatus(checkIn);

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Attendances
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Attendance", id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private static AttendanceStatus ResolveStatus(TimeOnly checkIn)
        => checkIn <= StandardStartTime ? AttendanceStatus.OnTime : AttendanceStatus.Late;

    private static IQueryable<Attendance> ApplySorting(IQueryable<Attendance> query, string? sort)
    {
        return sort switch
        {
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            "date:asc" => query.OrderBy(x => x.Date),
            "date:desc" => query.OrderByDescending(x => x.Date),
            "checkIn:asc" => query.OrderBy(x => x.CheckIn),
            "checkIn:desc" => query.OrderByDescending(x => x.CheckIn),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }
}
