namespace NetcoreHRIS.Modules.Dashboard.Dtos;

public record DashboardSummaryDto(
    int TotalDepartments,
    int TotalPositions,
    int TotalActiveEmployees,
    int TotalPermanentEmployees,
    int TotalContractEmployees,
    int TotalMaleEmployees,
    int TotalFemaleEmployees
);
