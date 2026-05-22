using System.ComponentModel.DataAnnotations;

namespace NetcoreHRIS.Modules.LeaveMasters.Dtos;

public record ListLeaveMasterQuery(
    string? Q = null,
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "createdAt:desc"
);

public record CreateLeaveMasterRequest(
    [Required(AllowEmptyStrings = false)]
    [StringLength(150, MinimumLength = 3)]
    string Name,

    [Required(AllowEmptyStrings = false)]
    [StringLength(50, MinimumLength = 2)]
    string Code,

    [Range(1, 365)]
    int QuotaDays,

    bool IsActive = true
);

public record UpdateLeaveMasterRequest(
    [StringLength(150, MinimumLength = 3)]
    string? Name,

    [StringLength(50, MinimumLength = 2)]
    string? Code,

    [Range(1, 365)]
    int? QuotaDays,

    bool? IsActive
);

public record LeaveMasterDto(
    Guid Id,
    string Name,
    string Code,
    int QuotaDays,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
