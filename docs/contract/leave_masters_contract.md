# Leave Masters Contract

This document defines the API contract for the `LeaveMaster` module.

## Scope

This module manages leave master records used by other leave-related modules.

Primary focus:
- create leave master
- list leave masters with filtering and sorting
- get leave master by id
- update leave master
- delete leave master

## Base Route

- Base path: `/api/v1/leave-masters`
- Controller: `LeaveMastersController`
- Auth: required
- Permission policy:
  - create: `HasPermission("create", "LeaveMaster")`
  - read: `HasPermission("read", "LeaveMaster")`
  - update: `HasPermission("update", "LeaveMaster")`
  - delete: `HasPermission("delete", "LeaveMaster")`

## Entity Shape

Leave master stores the following fields:

- `id`
- `name`
- `code`
- `quotaDays`
- `isActive`
- `createdAt`
- `updatedAt`

## DTO Contract

### ListLeaveMasterQuery

Query params:
- `q` optional
- `search` optional
- `isActive` optional
- `page` default `1`
- `limit` default `10`
- `sort` default `createdAt:desc`

Sorting format:
- `field:asc`
- `field:desc`

### CreateLeaveMasterRequest

Request body:

```json
{
  "name": "Annual Leave",
  "code": "AL",
  "quotaDays": 12,
  "isActive": true
}
```

Fields:
- `name` required
- `code` required
- `quotaDays` required
- `isActive` optional, default `true`

Validation rules:
- `name` must be present
- `name` must be between `3` and `150` characters
- `code` must be present
- `code` must be between `2` and `50` characters
- `quotaDays` must be between `1` and `365`

### UpdateLeaveMasterRequest

Request body:

```json
{
  "name": "Annual Leave Updated",
  "code": "ALU",
  "quotaDays": 14,
  "isActive": false
}
```

Fields:
- `name` optional
- `code` optional
- `quotaDays` optional
- `isActive` optional

Validation rules:
- if `name` is supplied, it must be between `3` and `150` characters
- if `code` is supplied, it must be between `2` and `50` characters
- if `quotaDays` is supplied, it must be between `1` and `365`

### LeaveMasterDto

Response body:

```json
{
  "id": "77777777-0000-0000-0000-000000000001",
  "name": "Annual Leave",
  "code": "AL",
  "quotaDays": 12,
  "isActive": true,
  "createdAt": "2026-05-24T01:23:45Z",
  "updatedAt": "2026-05-24T01:23:45Z"
}
```

## Endpoints

### Create Leave Master

- Method: `POST`
- Path: `/api/v1/leave-masters`
- Permission: `create LeaveMaster`

Request body: `CreateLeaveMasterRequest`

Success response:
- `201 Created`
- body: `Response<LeaveMasterDto>`

Example:

```http
POST /api/v1/leave-masters
Authorization: Bearer <access_token>
Content-Type: application/json

{
  "name": "Annual Leave",
  "code": "AL",
  "quotaDays": 12,
  "isActive": true
}
```

### Get All Leave Masters

- Method: `GET`
- Path: `/api/v1/leave-masters`
- Permission: `read LeaveMaster`

Query params: `ListLeaveMasterQuery`

Success response:
- `200 OK`
- body: `Response<IEnumerable<LeaveMasterDto>>`
- paginated via `meta`

Example:

```http
GET /api/v1/leave-masters?page=1&limit=10&sort=createdAt:desc
Authorization: Bearer <access_token>
```

### Get Leave Master By Id

- Method: `GET`
- Path: `/api/v1/leave-masters/{id}`
- Permission: `read LeaveMaster`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<LeaveMasterDto>`

### Update Leave Master

- Method: `PATCH`
- Path: `/api/v1/leave-masters/{id}`
- Permission: `update LeaveMaster`

Constraints:
- `id` must not be `Guid.Empty`

Request body: `UpdateLeaveMasterRequest`

Success response:
- `200 OK`
- body: `Response<LeaveMasterDto>`

### Delete Leave Master

- Method: `DELETE`
- Path: `/api/v1/leave-masters/{id}`
- Permission: `delete LeaveMaster`

Constraints:
- `id` must not be `Guid.Empty`

Success response:
- `200 OK`
- body: `Response<object?>`

## Validation Summary

The module validator enforces:

- `Name` is required on create
- `Code` is required on create
- `QuotaDays` is required on create
- `Name` length must be between `3` and `150`
- `Code` length must be between `2` and `50`
- `QuotaDays` must be between `1` and `365`
- pagination `Page > 0`
- pagination `Limit` between `1` and `100`
- `Sort` must match `field:asc` or `field:desc`

## Business Rules

- Leave master codes must be unique.
- Leave master names must be unique.
- Leave master records are referenced by leave allowances and leave requests.
- Delete is soft delete through the shared base entity pattern.
- `IsActive` controls whether the leave type is available for use.

## Common Errors

Expected errors follow the shared API contract from `docs/api_reference.md`, including:

- `UNAUTHORIZED`
- `FORBIDDEN`
- `NOT_FOUND`
- `CONFLICT`
- validation errors

Module-specific errors commonly include:

- invalid `id`
- duplicate leave master `code`
- duplicate leave master `name`
- invalid quota range

## Notes for Test Cases

Recommended test coverage:
- create leave master with a valid payload
- reject create when `code` is duplicated
- list leave masters with pagination and filters
- get by id with valid and invalid `Guid`
- update leave master with partial payload
- delete leave master by id
