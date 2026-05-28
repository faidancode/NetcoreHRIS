# Report Module Implementation Plan

## Summary

Buat module `Reports` untuk menyediakan 3 report utama:

- `EmployeeReport`
- `AttendanceReport`
- `LeavesReport`

Module ini read-only, memakai data existing dari `Employees`, `Departments`, `Positions`, `Attendances`, `LeaveRequests`, `LeaveMasters`, dan `LeaveAllowances`. Semua endpoint butuh auth dan permission `HasPermission("read", "Report")`.

## Key Changes

- Tambahkan folder `Modules/Reports` berisi `ReportsController`, `ReportsService`, DTOs, validators, dan helper export.
- Tambahkan permission seed baru: `read Report`.
- Tambahkan DI registration `IReportsService -> ReportsService`.
- Tambahkan package export:
  - Excel: `ClosedXML`
  - PDF: `QuestPDF`
- Gunakan response JSON standar `Response<T>` untuk endpoint statistik/data.
- Gunakan `File(...)` response untuk export `.xlsx` dan `.pdf`.

## API Contract

Base route:

```http
/api/v1/reports
```

Endpoints:

```http
GET /api/v1/reports/employees
GET /api/v1/reports/employees/export?format=xlsx|pdf

GET /api/v1/reports/attendances
GET /api/v1/reports/attendances/export?format=xlsx|pdf

GET /api/v1/reports/leaves
GET /api/v1/reports/leaves/export?format=xlsx|pdf
```

Common filters:

- `q` or `search`
- `departmentId`
- `positionId`
- `employeeId`
- `employeeStatus`
- `employmentType`
- `isActive`
- `gender`
- `fromDate`
- `toDate`
- `page`, `limit`, `sort`

Report-specific filters:

- Attendance: `attendanceStatus`, `date`
- Leaves: `leaveId`, `requestNo`

## Report Content

Employee report statistics:

- `totalEmployees`
- `totalActiveEmployees`
- `totalInactiveEmployees`
- `totalPermanentEmployees`
- `totalContractEmployees`
- `totalMaleEmployees`
- `totalFemaleEmployees`
- `totalByDepartment`
- `totalByPosition`
- `totalByEmployeeStatus`

Attendance report statistics:

- `totalAttendanceRecords`
- `totalOnTime`
- `totalLate`
- `totalEmployeesWithAttendance`
- `totalMissingCheckOut`
- `attendanceByDepartment`
- `attendanceByDate`
- `attendanceByStatus`

Leaves report statistics:

- `totalLeaveRequests`
- `totalLeaveDays`
- `totalEmployeesTakingLeave`
- `totalByLeaveType`
- `totalByDepartment`
- `leaveDaysByEmployee`
- `leaveRequestsByMonth`

Each report returns:

- `summary`
- `items`
- pagination metadata in the standard response `meta`

## Export Behavior

- `format=xlsx` returns an Excel workbook with `Summary` and `Details` sheets.
- `format=pdf` returns a printable report with title, generated timestamp, summary statistics, and a detail table.
- Export uses the same filters as the JSON report endpoint.
- Export ignores pagination and exports all filtered rows up to `10_000` rows.

## Test Plan

- Verify all report endpoints require authentication.
- Verify missing `read Report` permission returns forbidden.
- Verify employee report filters by department, position, employee status, employment type, gender, and active status.
- Verify attendance report filters by date range, employee, department, and attendance status.
- Verify leaves report filters by date range, employee, department, and leave type.
- Verify statistics match filtered data.
- Verify Excel export returns correct content type and file extension.
- Verify PDF export returns correct content type and file extension.
- Verify invalid `format` returns validation error.
- Verify pagination applies to JSON detail rows but export returns full filtered dataset within cap.

## Assumptions

- Report module is read-only.
- Leave request approval status is not implemented yet, so `LeavesReport` does not include approved/rejected/pending statistics.
- Export libraries are added as NuGet dependencies.
