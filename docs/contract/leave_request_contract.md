# Leave Request Contract

This document defines the API contract for the `LeaveRequest` module.

## Scope

This module manages leave request records submitted by employees.

Primary focus:
- create leave request
- list leave requests with filtering and sorting
- get leave request by id
- update leave request
- delete leave request

## Base Route

- Base path: `/api/v1/leave-requests`
- Controller: `LeaveRequestsController`
- Auth: required
- Permission policy:
  - create: `HasPermission("create", "LeaveRequest")`
  - read: `HasPermission("read", "LeaveRequest")`
  - update: `HasPermission("update", "LeaveRequest")`
  - delete: `HasPermission("delete", "LeaveRequest")`

## Entity Shape

Leave request stores the following fields:

- `id`
- `requestNo`
- `employeeId`
- `leaveMasterId`
- `fromDate`
- `toDate`
- `reason`
- `attachmentPath`
- `createdAt`
- `updatedAt`

## DTO Contract

### ListLeaveRequestQuery

Query params:
- `q` optional
- `search` optional
- `employeeId` optional
- `leaveId` optional
- `fromDate` optional
- `toDate` optional
- `page` default `1`
- `limit` default `10`
- `sort` default `createdAt:desc`

Sorting format:
- `field:asc`
- `field:desc`

Supported sort fields:
- `createdAt`
- `fromDate`
- `toDate`
- `requestNo`

### CreateLeaveRequestRequest

Request body:

```json
{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "fromDate": "2026-01-10",
  "toDate": "2026-01-12",
  "reason": "Family event",
  "attachmentPath": "/uploads/leave/family-event.pdf"
}
```

Fields:
- `employeeId` required
- `leaveId` required
- `fromDate` required
- `toDate` required
- `reason` required
- `attachmentPath` optional

Validation rules:
- `employeeId` must not be empty
- `leaveId` must not be empty
- `fromDate` must be present
- `toDate` must be present
- `toDate` must be greater than or equal to `fromDate`
- `reason` must be present
- `reason` must not exceed `1000` characters
- `attachmentPath`, if supplied, must not exceed `500` characters

### UpdateLeaveRequestRequest

Request body:

```json
{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "fromDate": "2026-01-10",
  "toDate": "2026-01-12",
  "reason": "Updated reason",
  "attachmentPath": "/uploads/leave/updated.pdf"
}
```

Fields:
- `employeeId` optional
- `leaveId` optional
- `fromDate` optional
- `toDate` optional
- `reason` optional
- `attachmentPath` optional

Validation rules:
- if `employeeId` is supplied, it must not be empty
- if `leaveId` is supplied, it must not be empty
- if both `fromDate` and `toDate` are supplied, `toDate` must be greater than or equal to `fromDate`
- if `reason` is supplied, it must not exceed `1000` characters
- if `attachmentPath` is supplied, it must not exceed `500` characters

### LeaveRequestDto

Response body:

```json
{
  "id": "77777777-0000-0000-0000-000000000001",
  "requestNo": "LR-20260524012345-ABCDEF",
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "employeeName": "John Doe",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "leaveName": "Annual Leave",
  "fromDate": "2026-01-10",
  "toDate": "2026-01-12",
  "reason": "Family event",
  "attachmentPath": "/uploads/leave/family-event.pdf",
  "createdAt": "2026-05-24T01:23:45Z",
  "updatedAt": "2026-05-24T01:23:45Z"
}
```

## Endpoints

### Create Leave Request

- Method: `POST`
- Path: `/api/v1/leave-requests`
- Permission: `create LeaveRequest`

Request body: `CreateLeaveRequestRequest`

Success response:
- `201 Created`
- body: `Response<LeaveRequestDto>`

Example:

```http
POST /api/v1/leave-requests
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "employeeId": "11111111-1111-1111-1111-111111111111",
  "leaveId": "22222222-2222-2222-2222-222222222222",
  "fromDate": "2026-01-10",
  "toDate": "2026-01-12",
  "reason": "Family event",
  "attachmentPath": "/uploads/leave/family-event.pdf"
}
```

### Get All Leave Requests

- Method: `GET`
- Path: `/api/v1/leave-requests`
- Permission: `read LeaveRequest`

Query params: `ListLeaveRequestQuery`

Success response:
- `200 OK`
- body: `Response<IEnumerable<LeaveRequestDto>>`
- paginated via `meta`

Example:

```http
GET /api/v1/leave-requests?page=1&limit=10&sort=createdAt:desc
Authorization: Bearer <access_token>
```

### Get Leave Request By Id

- Method: `GET`
- Path: `/api/v1/leave-requests/{id}`
- Permission: `read LeaveRequest`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<LeaveRequestDto>`

### Update Leave Request

- Method: `PATCH`
- Path: `/api/v1/leave-requests/{id}`
- Permission: `update LeaveRequest`

Constraints:
- `id` must not be `Guid.Empty`

Request body: `UpdateLeaveRequestRequest`

Success response:
- `200 OK`
- body: `Response<LeaveRequestDto>`

### Delete Leave Request

- Method: `DELETE`
- Path: `/api/v1/leave-requests/{id}`
- Permission: `delete LeaveRequest`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<object?>`

## Validation Summary

The module validator enforces:

- `EmployeeId` is required on create
- `LeaveId` is required on create
- `FromDate` is required on create
- `ToDate` is required on create
- `Reason` is required on create
- `ToDate >= FromDate` on create
- `Reason` must not exceed `1000` characters
- `AttachmentPath` must not exceed `500` characters
- if `FromDate` and `ToDate` are both present on update, `ToDate >= FromDate`
- pagination `Page > 0`
- pagination `Limit` between `1` and `100`
- `Sort` must match `field:asc` or `field:desc`

## Business Rules

- A leave request must reference an existing employee.
- A leave request must reference an existing leave master.
- `RequestNo` is generated automatically by the service.
- `RequestNo` is unique at the database level.
- Leave request records are soft deleted through the shared base entity pattern.
- `FromDate` and `ToDate` define the leave period.

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
- invalid date range
- invalid reason length
- invalid attachment path length

## Notes for Test Cases

Recommended test coverage:
- create leave request with a valid payload
- reject create when `toDate < fromDate`
- list leave requests with pagination and filters
- get by id with valid and invalid `Guid`
- update leave request with partial payload
- delete leave request by id
