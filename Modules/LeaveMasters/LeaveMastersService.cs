using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Common.Extensions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Data;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.LeaveMasters.Dtos;

namespace NetcoreHRIS.Modules.LeaveMasters;

public interface ILeaveMastersService
{
    Task<LeaveMasterDto> CreateAsync(CreateLeaveMasterRequest request, CancellationToken ct);
    Task<PagedResult<LeaveMasterDto>> GetAllAsync(ListLeaveMasterQuery query, CancellationToken ct);
    Task<LeaveMasterDto> GetByIdAsync(Guid id, CancellationToken ct);
    Task<LeaveMasterDto> UpdateAsync(Guid id, UpdateLeaveMasterRequest request, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

public class LeaveMastersService : ILeaveMastersService
{
    private readonly AppDbContext _db;

    public LeaveMastersService(AppDbContext db) => _db = db;

    public async Task<LeaveMasterDto> CreateAsync(CreateLeaveMasterRequest request, CancellationToken ct)
    {
        if (await _db.LeaveMasters.AnyAsync(x => x.Name == request.Name, ct))
            throw new ConflictException($"Leave master '{request.Name}' already exists.");

        if (await _db.LeaveMasters.AnyAsync(x => x.Code == request.Code, ct))
            throw new ConflictException($"Leave master code '{request.Code}' already exists.");

        var entity = new LeaveMaster
        {
            Name = request.Name,
            Code = request.Code,
            QuotaDays = request.QuotaDays,
            IsActive = request.IsActive
        };

        _db.LeaveMasters.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task<PagedResult<LeaveMasterDto>> GetAllAsync(ListLeaveMasterQuery query, CancellationToken ct)
    {
        var term = (query.Search ?? query.Q)?.Trim();
        var dbQuery = _db.LeaveMasters.AsQueryable();

        if (!string.IsNullOrEmpty(term))
        {
            var pattern = $"%{term}%";
            dbQuery = dbQuery.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                EF.Functions.ILike(x.Code, pattern));
        }

        if (query.IsActive.HasValue)
        {
            dbQuery = dbQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var limit = query.Limit < 1 ? 10 : Math.Min(query.Limit, 100);

        dbQuery = ApplySorting(dbQuery, query.Sort);

        var total = await dbQuery.CountAsync(ct);
        var items = await dbQuery
            .ApplyPagination(page, limit)
            .Select(MapToDtoExpression())
            .ToListAsync(ct);

        return new PagedResult<LeaveMasterDto>
        {
            Items = items,
            Total = total,
            Page = page,
            Limit = limit
        };
    }

    public async Task<LeaveMasterDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.LeaveMasters
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveMaster", id);

        return MapToDto(entity);
    }

    public async Task<LeaveMasterDto> UpdateAsync(Guid id, UpdateLeaveMasterRequest request, CancellationToken ct)
    {
        var entity = await _db.LeaveMasters
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveMaster", id);

        if (request.Name != null && request.Name != entity.Name)
        {
            if (await _db.LeaveMasters.AnyAsync(x => x.Name == request.Name && x.Id != id, ct))
                throw new ConflictException($"Leave master '{request.Name}' already exists.");

            entity.Name = request.Name;
        }

        if (request.Code != null && request.Code != entity.Code)
        {
            if (await _db.LeaveMasters.AnyAsync(x => x.Code == request.Code && x.Id != id, ct))
                throw new ConflictException($"Leave master code '{request.Code}' already exists.");

            entity.Code = request.Code;
        }

        if (request.QuotaDays.HasValue)
            entity.QuotaDays = request.QuotaDays.Value;

        if (request.IsActive.HasValue)
            entity.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync(ct);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.LeaveMasters
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("LeaveMaster", id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    private static LeaveMasterDto MapToDto(LeaveMaster x) =>
        new(x.Id, x.Name, x.Code, x.QuotaDays, x.IsActive, x.CreatedAt, x.UpdatedAt);

    private static IQueryable<LeaveMaster> ApplySorting(IQueryable<LeaveMaster> query, string? sort)
    {
        return sort switch
        {
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            "name:asc" => query.OrderBy(x => x.Name),
            "name:desc" => query.OrderByDescending(x => x.Name),
            "code:asc" => query.OrderBy(x => x.Code),
            "code:desc" => query.OrderByDescending(x => x.Code),
            "quotaDays:asc" => query.OrderBy(x => x.QuotaDays),
            "quotaDays:desc" => query.OrderByDescending(x => x.QuotaDays),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
    }

    private static System.Linq.Expressions.Expression<Func<LeaveMaster, LeaveMasterDto>> MapToDtoExpression()
        => x => new LeaveMasterDto(x.Id, x.Name, x.Code, x.QuotaDays, x.IsActive, x.CreatedAt, x.UpdatedAt);
}
