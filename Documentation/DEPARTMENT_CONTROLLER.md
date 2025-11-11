# Department Controller

**Route:** `/api/departments`

**Authorization:** All endpoints require authentication. Create, update, and delete require `SuperAdmin` or `Admin` role.

## Overview

The Department Controller manages department operations including creation, retrieval, updating, and deletion. Departments organize users into organizational units with optional manager assignments. Each user must belong to exactly one department.

---

## Endpoints

### 1. Get All Departments
**GET** `/api/departments`

Retrieves a list of all departments sorted by name.

**Authentication Required:** Yes (`[Authorize]`)

**Query Parameters:** None

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Unassigned",
    "description": "Default department for users without specific assignment",
    "isActive": true,
    "createdAt": "2025-11-10T08:00:00Z",
    "updatedAt": null,
    "userCount": 5,
    "manager": null
  },
  {
    "id": 2,
    "name": "Engineering",
    "description": "Software development and engineering team",
    "isActive": true,
    "createdAt": "2025-11-10T08:00:00Z",
    "updatedAt": null,
    "userCount": 12,
    "manager": {
      "id": "manager-id-123",
      "email": "alice@example.com",
      "fullName": "Alice Engineer"
    }
  },
  {
    "id": 3,
    "name": "Sales",
    "description": "Sales and business development team",
    "isActive": true,
    "createdAt": "2025-11-10T08:00:00Z",
    "updatedAt": null,
    "userCount": 8,
    "manager": {
      "id": "manager-id-456",
      "email": "bob@example.com",
      "fullName": "Bob Sales"
    }
  }
]
```

**Status Codes:**
- `200 OK` - Departments retrieved successfully
- `401 Unauthorized` - Not authenticated
- `500 Internal Server Error` - Server error

**Implementation Notes:**
- Returns all departments ordered by name
- Includes user count and manager information
- Departments are soft-deleted (IsActive = false); still returned in list
- All authenticated users can access this endpoint

---

### 2. Get Department by ID
**GET** `/api/departments/{id}`

Retrieves a specific department by its ID.

**Authentication Required:** Yes (`[Authorize]`)

**Path Parameters:**
- `id` (int, required) - The department ID

**Response (200 OK):**
```json
{
  "id": 2,
  "name": "Engineering",
  "description": "Software development and engineering team",
  "isActive": true,
  "createdAt": "2025-11-10T08:00:00Z",
  "updatedAt": null,
  "userCount": 12,
  "manager": {
    "id": "manager-id-123",
    "email": "alice@example.com",
    "fullName": "Alice Engineer"
  }
}
```

**Response (404 Not Found):**
```json
{
  "message": "Department not found."
}
```

**Status Codes:**
- `200 OK` - Department retrieved successfully
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - Department not found
- `500 Internal Server Error` - Server error

---

### 3. Create Department
**POST** `/api/departments`

Creates a new department. SuperAdmin and Admin roles can create departments.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin,Admin")]`

**Request Body:**
```json
{
  "name": "Marketing",
  "description": "Marketing and communications team",
  "managerId": null
}
```

**Request Body (With Manager):**
```json
{
  "name": "Research",
  "description": "Research and development team",
  "managerId": "manager-id-789"
}
```

**Response (201 Created):**
```json
{
  "id": 6,
  "name": "Marketing",
  "description": "Marketing and communications team",
  "isActive": true,
  "createdAt": "2025-11-11T15:30:00Z",
  "updatedAt": null,
  "userCount": 0,
  "manager": null
}
```

**Response (400 Bad Request - Duplicate Name):**
```json
{
  "message": "A department with this name already exists."
}
```

**Response (400 Bad Request - Invalid Manager):**
```json
{
  "message": "Manager user not found."
}
```

**Status Codes:**
- `201 Created` - Department created successfully
- `400 Bad Request` - Invalid request or validation failure
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin or Admin
- `500 Internal Server Error` - Server error

**Validation Rules:**
- Department name must be unique
- Description is optional
- Manager ID is optional; if provided, user must exist
- Manager must belong to this department (validated when assigning)

**Implementation Notes:**
- New departments are active by default (`IsActive = true`)
- Created timestamp set to current UTC time
- User count starts at 0
- Manager can be added later via assign manager endpoint

---

### 4. Update Department
**PUT** `/api/departments/{id}`

Updates an existing department's name, description, and/or manager. SuperAdmin and Admin can update departments.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin,Admin")]`

**Path Parameters:**
- `id` (int, required) - The department ID

**Request Body:**
```json
{
  "name": "Research and Development",
  "description": "R&D and innovation team",
  "managerId": "manager-id-123"
}
```

**Request Body (Clear Manager):**
```json
{
  "name": "Engineering",
  "description": "Software development team",
  "managerId": null
}
```

**Response (200 OK):**
```json
{
  "id": 2,
  "name": "Research and Development",
  "description": "R&D and innovation team",
  "isActive": true,
  "createdAt": "2025-11-10T08:00:00Z",
  "updatedAt": "2025-11-11T15:45:00Z",
  "userCount": 12,
  "manager": {
    "id": "manager-id-123",
    "email": "alice@example.com",
    "fullName": "Alice Manager"
  }
}
```

**Response (400 Bad Request - Name Already Exists):**
```json
{
  "message": "A department with this name already exists."
}
```

**Response (400 Bad Request - Manager Not in Department):**
```json
{
  "message": "Manager must belong to this department."
}
```

**Status Codes:**
- `200 OK` - Department updated successfully
- `400 Bad Request` - Invalid request or validation failure
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin or Admin
- `404 Not Found` - Department not found
- `500 Internal Server Error` - Server error

**Validation Rules:**
- Department name must be unique (or same as current name)
- If manager specified, user must exist and belong to this department
- Can clear manager by setting to null
- Cannot change name to conflict with another department

**Implementation Notes:**
- Updates `UpdatedAt` timestamp to current UTC time
- All fields are optional in request; only provided fields are updated
- Null manager ID clears the department manager

---

### 5. Assign Manager to Department
**PUT** `/api/departments/{id}/manager`

Assigns a manager to a department. The manager must belong to the department.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin,Admin")]`

**Path Parameters:**
- `id` (int, required) - The department ID

**Request Body:**
```json
{
  "managerId": "manager-id-123"
}
```

**Response (200 OK):**
```json
{
  "id": 2,
  "name": "Engineering",
  "description": "Software development and engineering team",
  "isActive": true,
  "createdAt": "2025-11-10T08:00:00Z",
  "updatedAt": "2025-11-11T16:00:00Z",
  "userCount": 12,
  "manager": {
    "id": "manager-id-123",
    "email": "alice@example.com",
    "fullName": "Alice Engineer"
  }
}
```

**Response (400 Bad Request - Manager Not in Department):**
```json
{
  "message": "Manager must belong to this department."
}
```

**Response (404 Not Found):**
```json
{
  "message": "Department not found." OR "Manager user not found."
}
```

**Status Codes:**
- `200 OK` - Manager assigned successfully
- `400 Bad Request` - Manager does not belong to department
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin or Admin
- `404 Not Found` - Department or user not found
- `500 Internal Server Error` - Server error

**Validation Rules:**
- User being assigned as manager must exist
- User must belong to this department (DepartmentId must match)
- User can be reassigned as manager to different department (in update endpoint)

**Implementation Notes:**
- Dedicated endpoint for assigning manager
- Simpler than update endpoint when only changing manager
- Updates `UpdatedAt` timestamp

---

### 6. Delete (Soft-Delete) Department
**DELETE** `/api/departments/{id}`

Soft-deletes a department by setting `IsActive = false`. Only SuperAdmin can delete departments. Cannot delete a department with assigned users.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin")]`

**Path Parameters:**
- `id` (int, required) - The department ID

**Response (204 No Content):**
No response body (successful deletion)

**Response (400 Bad Request - Has Users):**
```json
{
  "message": "Cannot delete a department with assigned users. Please reassign users first."
}
```

**Status Codes:**
- `204 No Content` - Department deleted successfully
- `400 Bad Request` - Department has assigned users
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin
- `404 Not Found` - Department not found
- `500 Internal Server Error` - Server error

**Implementation Notes:**
- Soft delete: sets `IsActive = false` instead of deleting record
- Updates `UpdatedAt` timestamp to current UTC time
- Department remains queryable but marked as inactive
- Prevents deletion if users are assigned (referential integrity)
- Users must be reassigned to different department before deletion

---

## Request/Response Models

### CreateDepartmentRequest
```csharp
public class CreateDepartmentRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ManagerId { get; set; }
}
```

### UpdateDepartmentRequest
```csharp
public class UpdateDepartmentRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ManagerId { get; set; }
}
```

### AssignManagerRequest
```csharp
public class AssignManagerRequest
{
    public required string ManagerId { get; set; }
}
```

### DepartmentDto (Response)
```csharp
public class DepartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserCount { get; set; }
    public ManagerDto? Manager { get; set; }
}
```

### ManagerDto
```csharp
public class ManagerDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
}
```

---

## Authorization Rules

| Endpoint | SuperAdmin | Admin | User | Anonymous |
|----------|:----------:|:-----:|:----:|:---------:|
| GET all departments | ✓ | ✓ | ✓ | ✗ |
| GET department by ID | ✓ | ✓ | ✓ | ✗ |
| POST (create) | ✓ | ✓ | ✗ | ✗ |
| PUT (update) | ✓ | ✓ | ✗ | ✗ |
| PUT (assign manager) | ✓ | ✓ | ✗ | ✗ |
| DELETE (soft-delete) | ✓ | ✗ | ✗ | ✗ |

---

## Default Departments

Five default departments are automatically created on application startup by `DepartmentSeederService`:

| ID | Name | Description |
|----|------|-------------|
| 1 | Unassigned | Default department for users without specific assignment |
| 2 | Engineering | Software development and engineering team |
| 3 | Human Resources | HR and personnel management |
| 4 | Sales | Sales and business development |
| 5 | Support | Customer support and success |

---

## Usage Examples

### Example 1: Get All Departments
```bash
curl -X GET http://localhost:5164/api/departments \
  -b cookies.txt
```

### Example 2: Create New Department
```bash
curl -X POST http://localhost:5164/api/departments \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Marketing",
    "description": "Marketing and communications team",
    "managerId": null
  }'
```

### Example 3: Update Department
```bash
curl -X PUT http://localhost:5164/api/departments/6 \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "name": "Marketing and Communications",
    "description": "Updated description"
  }'
```

### Example 4: Assign Manager
```bash
curl -X PUT http://localhost:5164/api/departments/2/manager \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"managerId": "user-id-123"}'
```

### Example 5: Delete Department
```bash
curl -X DELETE http://localhost:5164/api/departments/6 \
  -b cookies.txt
```

### Example 6: Using REST Client in VS Code
```http
### Get all departments
GET http://localhost:5164/api/departments

### Get department by ID
GET http://localhost:5164/api/departments/2

### Create department
POST http://localhost:5164/api/departments
Content-Type: application/json

{
  "name": "Marketing",
  "description": "Marketing team"
}

### Assign manager to department
PUT http://localhost:5164/api/departments/2/manager
Content-Type: application/json

{
  "managerId": "user-id-123"
}

### Update department
PUT http://localhost:5164/api/departments/2
Content-Type: application/json

{
  "name": "Engineering",
  "description": "Updated description",
  "managerId": null
}

### Delete department
DELETE http://localhost:5164/api/departments/2
```

---

## Department-User Relationship Workflow

### Adding User to Department

1. **Admin creates user** via `UserManagementController.CreateUser()` with `departmentId`
2. **User belongs** to specified department
3. **User appears** in department's user count
4. **Department query** includes user in results

### Reassigning User to Different Department

1. **Admin calls** `UserManagementController.UpdateUser()` with new `departmentId`
2. **Old department** user count decreases
3. **New department** user count increases
4. **User is now** in new department

### Removing User from Department

Cannot remove user from department without assigning to another department (required field).

---

## Soft Deletion vs Hard Deletion

The system uses **soft deletion** for departments:

**Soft Delete (Current Implementation):**
- Sets `IsActive = false`
- Record remains in database
- Department still appears in queries (if needed)
- Can be "reactivated" by setting `IsActive = true`
- Preserves referential integrity

**Why Soft Deletion:**
- Maintains audit trail
- Prevents orphaned user records
- Allows future reactivation
- Preserves historical data

---

## Related Controllers

- [User Management Controller](./USER_MANAGEMENT_CONTROLLER.md) - Create and manage user accounts
- [Authentication Controller](./AUTHENTICATION_CONTROLLER.md) - User login and profile management
- [Role Management Controller](./ROLE_MANAGEMENT_CONTROLLER.md) - Manage user roles

---

## Related Files

- **Controller:** `Controllers/DepartmentController.cs`
- **Models:** `Models/Department.cs`, `Models/ApplicationUser.cs`
- **Database Context:** `Data/AppDbContext.cs`
- **Services:** `Services/DepartmentSeederService.cs`
