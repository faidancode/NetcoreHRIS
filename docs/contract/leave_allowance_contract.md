# Leave Allowance Contract

This document defines the API contract for the `LeaveAllowance` module.

## Scope

This module manages leave allowance records assigned to employees for a specific leave type and year.

Primary focus:
- create leave allowance
- list leave allowances with filtering and sorting
- get leave allowance by id
- update leave allowance
- delete leave allowance

## Base Route

- Base path: `/api/v1/leave-allowances`
- Controller: `LeaveAllowancesController`
- Auth: required
- Permission policy:
  - create: `HasPermission("create", "LeaveAllowance")`
  - read: `HasPermission("read", "LeaveAllowance")`
  - update: `HasPermission("update", "LeaveAllowance")`
  - delete: `HasPermission("delete", "LeaveAllowance")`

## Entity Shape

Leave allowance stores the following fields:

- `id`
- `employeeId`
- `leaveMasterId`
- `year`
- `quotaDays`
- `notes`
- `createdAt`
- `updatedAt`

## DTO Contract

### ListLeaveAllowanceQuery

Query params:
- `q` optional
- `search` optional
- `employeeId` optional
- `leaveId` optional
- `year` optional
- `page` default `1`
- `limit` default `10`
- `sort` default `createdAt:desc`

Sorting format:
- `field:asc`
- `field:desc`

Supported sort fields:
- `createdAt`
- `year`
- `quotaDays`

### CreateLeaveAllowanceRequest

Request body:

```json
{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "year": 2026,
  "quotaDays": 12,
  "notes": "Annual quota"
}
```

Fields:
- `employeeId` required
- `leaveId` required
- `year` required
- `quotaDays` required
- `notes` optional

Validation rules:
- `employeeId` must not be empty
- `leaveId` must not be empty
- `year` must be between `2000` and `2100`
- `quotaDays` must be between `1` and `365`
- `notes`, if supplied, must not exceed `500` characters

### UpdateLeaveAllowanceRequest

Request body:

```json
{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "year": 2026,
  "quotaDays": 14,
  "notes": "Updated quota"
}
```

Fields:
- `employeeId` optional
- `leaveId` optional
- `year` optional
- `quotaDays` optional
- `notes` optional

Validation rules:
- if `employeeId` is supplied, it must not be empty
- if `leaveId` is supplied, it must not be empty
- if `year` is supplied, it must be between `2000` and `2100`
- if `quotaDays` is supplied, it must be between `1` and `365`
- `notes`, if supplied, must not exceed `500` characters

### LeaveAllowanceDto

Response body:

```json
{
  "id": "77777777-0000-0000-0000-000000000001",
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "employeeName": "John Doe",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "leaveName": "Annual Leave",
  "year": 2026,
  "quotaDays": 12,
  "notes": "Annual quota",
  "createdAt": "2026-05-24T01:23:45Z",
  "updatedAt": "2026-05-24T01:23:45Z"
}
```

## Endpoints

### Create Leave Allowance

- Method: `POST`
- Path: `/api/v1/leave-allowances`
- Permission: `create LeaveAllowance`

Request body: `CreateLeaveAllowanceRequest`

Success response:
- `201 Created`
- body: `Response<LeaveAllowanceDto>`

Example:

```http
POST /api/v1/leave-allowances
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "year": 2026,
  "quotaDays": 12,
  "notes": "Annual quota"
}
```

### Get All Leave Allowances

- Method: `GET`
- Path: `/api/v1/leave-allowances`
- Permission: `read LeaveAllowance`

Query params: `ListLeaveAllowanceQuery`

Success response:
- `200 OK`
- body: `Response<IEnumerable<LeaveAllowanceDto>>`
- paginated via `meta`

Example:

```http
GET /api/v1/leave-allowances?page=1&limit=10&sort=createdAt:desc
Authorization: Bearer <access_token>
```

### Get Leave Allowance By Id

- Method: `GET`
- Path: `/api/v1/leave-allowances/{id}`
- Permission: `read LeaveAllowance`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<LeaveAllowanceDto>`

### Update Leave Allowance

- Method: `PATCH`
- Path: `/api/v1/leave-allowances/{id}`
- Permission: `update LeaveAllowance`

Constraints:
- `id` must not be `Guid.Empty`

Request body: `UpdateLeaveAllowanceRequest`

Success response:
- `200 OK`
- body: `Response<LeaveAllowanceDto>`

### Delete Leave Allowance

- Method: `DELETE`
- Path: `/api/v1/leave-allowances/{id}`
- Permission: `delete LeaveAllowance`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<object?>`

## Validation Summary

The module validator enforces:

- `EmployeeId` is required on create
- `LeaveId` is required on create
- `Year` is required on create
- `QuotaDays` is required on create
- `Year` must be between `2000` and `2100`
- `QuotaDays` must be between `1` and `365`
- `Notes` must not exceed `500` characters
- pagination `Page > 0`
- pagination `Limit` between `1` and `100`
- `Sort` must match `field:asc` or `field:desc`

## Business Rules

- A leave allowance must reference an existing employee.
- A leave allowance must reference an existing leave master.
- A leave allowance must be unique for the combination of employee, leave master, and year.
- Delete is soft delete through the shared base entity pattern.
- `QuotaDays` can be adjusted independently during update.

## Common Errors

Expected errors follow the shared API contract from `docs/api_reference.md`, including:

- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- `CONFLICT`
- validation errors

Module-specific errors commonly include:

- invalid `id`
- missing employee
- missing leave master
- duplicate leave allowance for the same employee, leave type, and year
- invalid year range
- invalid quota range

## Notes for Test Cases

Recommended test coverage:
- create leave allowance with a valid payload
- reject create when allowance is duplicated
- list leave allowances with pagination and filters
- get by id with valid and invalid `Guid`
- update leave allowance with partial payload
- delete leave allowance by id
