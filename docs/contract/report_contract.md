# Report Contract

This document defines the API contract for the `Reports` module.

## Scope

This module provides read-only reporting for:

- employees
- attendances
- leave requests

Primary focus:

- retrieve summary and detail data
- filter and sort report data
- export report data to Excel or PDF

## Base Route

- Base path: `/api/v1/reports`
- Controller: `ReportsController`
- Auth: required
- Permission policy:
  - read: `HasPermission("read", "Report")`

## Response Contract

All JSON endpoints return the shared API wrapper:

- `Response<T>`

Pagination metadata is returned through:

- `meta: PaginationMeta`

Standard paginated shape:

```json
{
  "success": true,
  "message": null,
  "code": null,
  "data": {
    "summary": {},
    "items": []
  },
  "meta": {
    "page": 1,
    "limit": 10,
    "total": 0,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "errors": null
}
```

Export endpoints return file responses:

- `FileContentResult`
- Excel content type: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- PDF content type: `application/pdf`

## Query Contract

### Shared Filters

These filters are supported across report queries where applicable:

- `q` optional
- `search` optional
- `departmentId` optional
- `positionId` optional
- `employeeId` optional
- `employeeStatus` optional
- `employmentType` optional
- `isActive` optional
- `gender` optional
- `fromDate` optional
- `toDate` optional
- `page` default `1`
- `limit` default `10`
- `sort` default depends on report type

Sorting format:

- `field:asc`
- `field:desc`

Validator rules:

- `page > 0`
- `limit` between `1` and `100`
- `sort` must match the `field:asc` or `field:desc` pattern
- if both `fromDate` and `toDate` are provided, `toDate >= fromDate`

## Report Queries

### EmployeeReportQuery

Query params:

- `q`
- `search`
- `departmentId`
- `positionId`
- `employeeId`
- `employeeStatus`
- `employmentType`
- `isActive`
- `gender`
- `fromDate`
- `toDate`
- `page`
- `limit`
- `sort` default `createdAt:desc`

Supported sort fields:

- `fullName`
- `nip`
- `dateOfJoining`
- `createdAt`

### AttendanceReportQuery

Query params:

- `q`
- `search`
- `departmentId`
- `positionId`
- `employeeId`
- `employeeStatus`
- `employmentType`
- `isActive`
- `gender`
- `fromDate`
- `toDate`
- `date`
- `attendanceStatus`
- `page`
- `limit`
- `sort` default `date:desc`

Supported sort fields:

- `date`
- `checkIn`
- `createdAt`

### LeavesReportQuery

Query params:

- `q`
- `search`
- `departmentId`
- `positionId`
- `employeeId`
- `employeeStatus`
- `employmentType`
- `isActive`
- `gender`
- `fromDate`
- `toDate`
- `leaveId`
- `requestNo`
- `page`
- `limit`
- `sort` default `createdAt:desc`

Supported sort fields:

- `requestNo`
- `fromDate`
- `toDate`
- `createdAt`

## DTO Contract

### ReportExportFormat

Allowed values:

- `xlsx`
- `pdf`

### EmployeeReportDto

Response body:

```json
{
  "summary": {
    "totalEmployees": 10,
    "totalActiveEmployees": 8,
    "totalInactiveEmployees": 2,
    "totalPermanentEmployees": 6,
    "totalContractEmployees": 4,
    "totalMaleEmployees": 6,
    "totalFemaleEmployees": 4,
    "totalByDepartment": [],
    "totalByPosition": [],
    "totalByEmployeeStatus": []
  },
  "items": []
}
```

Summary fields:

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

### EmployeeReportItemDto

Item fields:

- `id`
- `fullName`
- `nip`
- `gender`
- `employmentType`
- `employeeStatus`
- `isActive`
- `departmentId`
- `departmentName`
- `positionId`
- `positionName`
- `dateOfJoining`
- `createdAt`

### AttendanceReportDto

Response body:

```json
{
  "summary": {
    "totalAttendanceRecords": 10,
    "totalOnTime": 7,
    "totalLate": 3,
    "totalEmployeesWithAttendance": 5,
    "totalMissingCheckOut": 1,
    "attendanceByDepartment": [],
    "attendanceByDate": [],
    "attendanceByStatus": []
  },
  "items": []
}
```

Summary fields:

- `totalAttendanceRecords`
- `totalOnTime`
- `totalLate`
- `totalEmployeesWithAttendance`
- `totalMissingCheckOut`
- `attendanceByDepartment`
- `attendanceByDate`
- `attendanceByStatus`

### AttendanceReportItemDto

Item fields:

- `id`
- `date`
- `employeeId`
- `employeeName`
- `employeeNip`
- `departmentId`
- `departmentName`
- `positionId`
- `positionName`
- `checkIn`
- `checkOut`
- `status`

### LeavesReportDto

Response body:

```json
{
  "summary": {
    "totalLeaveRequests": 10,
    "totalLeaveDays": 24,
    "totalEmployeesTakingLeave": 5,
    "totalByLeaveType": [],
    "totalByDepartment": [],
    "leaveDaysByEmployee": [],
    "leaveRequestsByMonth": []
  },
  "items": []
}
```

Summary fields:

- `totalLeaveRequests`
- `totalLeaveDays`
- `totalEmployeesTakingLeave`
- `totalByLeaveType`
- `totalByDepartment`
- `leaveDaysByEmployee`
- `leaveRequestsByMonth`

### LeavesReportItemDto

Item fields:

- `id`
- `requestNo`
- `employeeId`
- `employeeName`
- `employeeNip`
- `departmentId`
- `departmentName`
- `leaveId`
- `leaveName`
- `fromDate`
- `toDate`
- `totalDays`
- `reason`
- `createdAt`

## Endpoints

### Get Employee Report

- Method: `GET`
- Path: `/api/v1/reports/employees`
- Permission: `read Report`

Query params: `EmployeeReportQuery`

Success response:

- `200 OK`
- body: `Response<EmployeeReportDto>`
- paginated via `meta`

Example:

```http
GET /api/v1/reports/employees?page=1&limit=10&sort=createdAt:desc
Authorization: Bearer <access_token>
```

### Export Employee Report

- Method: `GET`
- Path: `/api/v1/reports/employees/export`
- Permission: `read Report`

Query params:

- `EmployeeReportQuery`
- `format` required, values `xlsx` or `pdf`

Success response:

- `200 OK`
- file response

### Get Attendance Report

- Method: `GET`
- Path: `/api/v1/reports/attendances`
- Permission: `read Report`

Query params: `AttendanceReportQuery`

Success response:

- `200 OK`
- body: `Response<AttendanceReportDto>`
- paginated via `meta`

Example:

```http
GET /api/v1/reports/attendances?page=1&limit=10&sort=date:desc
Authorization: Bearer <access_token>
```

### Export Attendance Report

- Method: `GET`
- Path: `/api/v1/reports/attendances/export`
- Permission: `read Report`

Query params:

- `AttendanceReportQuery`
- `format` required, values `xlsx` or `pdf`

Success response:

- `200 OK`
- file response

### Get Leaves Report

- Method: `GET`
- Path: `/api/v1/reports/leaves`
- Permission: `read Report`

Query params: `LeavesReportQuery`

Success response:

- `200 OK`
- body: `Response<LeavesReportDto>`
- paginated via `meta`

Example:

```http
GET /api/v1/reports/leaves?page=1&limit=10&sort=createdAt:desc
Authorization: Bearer <access_token>
```

### Export Leaves Report

- Method: `GET`
- Path: `/api/v1/reports/leaves/export`
- Permission: `read Report`

Query params:

- `LeavesReportQuery`
- `format` required, values `xlsx` or `pdf`

Success response:

- `200 OK`
- file response

## Export Behavior

- `format=xlsx` generates an Excel workbook with `Summary` and `Details` sheets.
- `format=pdf` generates a printable landscape PDF with title, generated timestamp, summary section, and detail table.
- Export uses the same filters as the JSON report endpoint.
- Export ignores pagination and exports all filtered rows up to `10,000` rows.
- PDF export renders at most `200` detail rows per file.
- Exported file names follow the pattern:
  - `employee-report-YYYYMMDDHHMMSS.xlsx`
  - `employee-report-YYYYMMDDHHMMSS.pdf`
  - `attendance-report-YYYYMMDDHHMMSS.xlsx`
  - `attendance-report-YYYYMMDDHHMMSS.pdf`
  - `leaves-report-YYYYMMDDHHMMSS.xlsx`
  - `leaves-report-YYYYMMDDHHMMSS.pdf`

## Business Rules

- Report module is read-only.
- All endpoints require authentication and `read Report` permission.
- Employee report is derived from `Employees`, `Departments`, and `Positions`.
- Attendance report is derived from `Attendances` and related employee data.
- Leaves report is derived from `LeaveRequests`, `LeaveMasters`, and related employee data.
- Summary statistics are computed from the filtered dataset.
- Detail rows are paginated only for JSON endpoints.
- Export output is based on the filtered dataset without pagination.
- `LeavesReport` calculates `totalDays` as inclusive date range.

## Common Errors

Expected errors follow the shared API contract from `docs/api_reference.md`, including:

- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- validation errors

Module-specific errors commonly include:

- invalid `format`
- invalid `sort`
- invalid date range
- invalid filter values

## Notes for Test Cases

Recommended test coverage:

- verify all report endpoints require authentication
- verify missing `read Report` permission returns forbidden
- verify employee report filters by department, position, employee status, employment type, gender, active status, and date of joining range
- verify attendance report filters by employee, department, position, date, attendance status, and date range
- verify leaves report filters by employee, department, position, leave type, request number, and date range
- verify statistics match filtered data
- verify pagination applies to JSON detail rows
- verify export returns the correct content type for Excel and PDF
- verify invalid `format` returns validation error
- verify export ignores pagination and respects the export row cap
