# Attendance Contract

This document defines the API contract for the `Attendance` module.

## Scope

This module handles daily employee attendance records.

Primary focus:
- create attendance
- list attendance with filtering and sorting
- get attendance by id
- update attendance
- delete attendance

## Base Route

- Base path: `/api/v1/attendances`
- Controller: `AttendancesController`
- Auth: required
- Permission policy:
  - create: `HasPermission("create", "Attendance")`
  - read: `HasPermission("read", "Attendance")`
  - update: `HasPermission("update", "Attendance")`
  - delete: `HasPermission("delete", "Attendance")`

## Entity Shape

Attendance stores the following fields:

- `id`
- `date`
- `employeeId`
- `checkIn`
- `checkOut`
- `status`
- `createdAt`
- `updatedAt`

## DTO Contract

### ListAttendanceQuery

Query params:
- `q` optional
- `search` optional
- `employeeId` optional
- `date` optional
- `fromDate` optional
- `toDate` optional
- `page` default `1`
- `limit` default `10`
- `sort` default `date:desc`

Sorting format:
- `field:asc`
- `field:desc`

### CreateAttendanceRequest

Request body:

```json
{
  "date": "2026-05-24",
  "employeeId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "checkIn": "08:00:00",
  "checkOut": "17:00:00"
}
```

Fields:
- `date` required
- `employeeId` required
- `checkIn` required
- `checkOut` required

Validation rules:
- `date` must be present
- `employeeId` must be present
- `checkOut` must be greater than or equal to `checkIn`

### UpdateAttendanceRequest

Request body:

```json
{
  "date": "2026-05-24",
  "employeeId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "checkIn": "08:15:00",
  "checkOut": "17:10:00"
}
```

Fields:
- `date` optional
- `employeeId` optional
- `checkIn` optional
- `checkOut` optional

Validation rules:
- if `employeeId` is supplied, it must not be empty
- if both `checkIn` and `checkOut` are supplied, `checkOut` must be greater than or equal to `checkIn`

### AttendanceDto

Response body:

```json
{
  "id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "date": "2026-05-24",
  "employeeId": "11111111-2222-3333-4444-555555555555",
  "employeeName": "Budi Santoso",
  "employeeNip": "EMP-0001",
  "checkIn": "08:00:00",
  "checkOut": "17:00:00",
  "status": "OnTime",
  "createdAt": "2026-05-24T01:23:45Z",
  "updatedAt": "2026-05-24T01:23:45Z"
}
```

`status` follows the `AttendanceStatus` enum:
- `OnTime`
- `Late`

## Endpoints

### Create Attendance

- Method: `POST`
- Path: `/api/v1/attendances`
- Permission: `create Attendance`

Request body: `CreateAttendanceRequest`

Success response:
- `201 Created`
- body: `Response<AttendanceDto>`

Example:

```http
POST /api/v1/attendances
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "date": "2026-05-24",
  "employeeId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "checkIn": "08:00:00",
  "checkOut": "17:00:00"
}
```

### Get All Attendances

- Method: `GET`
- Path: `/api/v1/attendances`
- Permission: `read Attendance`

Query params: `ListAttendanceQuery`

Success response:
- `200 OK`
- body: `Response<IEnumerable<AttendanceDto>>`
- paginated via `meta`

Example:

```http
GET /api/v1/attendances?page=1&limit=10&sort=date:desc
Authorization: Bearer <access_token>
```

### Get Attendance By Id

- Method: `GET`
- Path: `/api/v1/attendances/{id}`
- Permission: `read Attendance`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<AttendanceDto>`

### Update Attendance

- Method: `PATCH`
- Path: `/api/v1/attendances/{id}`
- Permission: `update Attendance`

Constraints:
- `id` must not be `Guid.Empty`

Request body: `UpdateAttendanceRequest`

Success response:
- `200 OK`
- body: `Response<AttendanceDto>`

### Delete Attendance

- Method: `DELETE`
- Path: `/api/v1/attendances/{id}`
- Permission: `delete Attendance`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<object?>`

## Validation Summary

The module validator enforces:

- `Date` is required on create
- `EmployeeId` is required on create
- `CheckIn` is required on create
- `CheckOut` is required on create
- `CheckOut >= CheckIn` when both are present
- pagination `Page > 0`
- pagination `Limit` between `1` and `100`
- `Sort` must match `field:asc` or `field:desc`

## Business Rules

- Attendance is linked to one employee.
- Attendance date is unique per employee at the database level.
- `Status` is derived by service logic and returned as part of the DTO.
- Delete is soft delete through the shared base entity pattern.

## Common Errors

Expected errors follow the shared API contract from `docs/api_reference.md`, including:

- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- `CONFLICT`
- validation errors

Module-specific errors commonly include:

- invalid `id`
- invalid date or time range
- duplicate attendance for the same employee and date

## Notes for Test Cases

Recommended test coverage:
- create attendance with a valid payload
- reject create when `checkOut < checkIn`
- list attendance with pagination and filters
- get by id with valid and invalid `Guid`
- update attendance with partial payload
- delete attendance by id
