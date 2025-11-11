# Role Management Controller

**Route:** `/api/rolemanagement`

**Authorization Required:** `SuperAdmin` role (all endpoints)

## Overview

The Role Management Controller handles role management and user role assignments. Only SuperAdmin users have access to these endpoints, allowing them to assign and remove roles from users, and manage the role system.

**Three-Tier Role Hierarchy:**
- **SuperAdmin** - Full system access; can manage all users and roles
- **Admin** - Moderate system access; can manage only User-role accounts
- **User** - Standard user with limited access

---

## Endpoints

### 1. Get All Roles
**GET** `/api/rolemanagement`

Retrieves a list of all available roles in the system.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin")]`

**Response (200 OK):**
```json
[
  "SuperAdmin",
  "Admin",
  "User"
]
```

**Status Codes:**
- `200 OK` - Roles retrieved successfully
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin
- `500 Internal Server Error` - Server error

**Implementation Notes:**
- Returns list of all available roles
- Roles are created automatically on application startup by `RoleSeederService`
- Null role names are filtered out

---

### 2. Get User's Roles
**GET** `/api/rolemanagement/user/{userId}`

Retrieves all roles assigned to a specific user.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID

**Response (200 OK):**
```json
[
  "SuperAdmin"
]
```

**Response Example (Multiple Roles):**
```json
[
  "SuperAdmin",
  "Admin"
]
```

**Response (404 Not Found):**
```json
{
  "message": "User not found"
}
```

**Status Codes:**
- `200 OK` - Roles retrieved successfully
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

---

### 3. Assign Role to User
**POST** `/api/rolemanagement/user/{userId}/assign`

Assigns a role to a user. The user must not already have the role being assigned.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID

**Request Body:**
```json
{
  "roleName": "Admin"
}
```

**Response (200 OK):**
```json
{
  "message": "Role 'Admin' assigned successfully"
}
```

**Response (400 Bad Request - User Already Has Role):**
```json
{
  "message": "User already has the 'Admin' role"
}
```

**Response (400 Bad Request - Role Does Not Exist):**
```json
{
  "message": "Role 'InvalidRole' does not exist"
}
```

**Response (404 Not Found - User Does Not Exist):**
```json
{
  "message": "User not found"
}
```

**Status Codes:**
- `200 OK` - Role assigned successfully
- `400 Bad Request` - Invalid request or user already has role
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

**Validation Rules:**
- Role must exist in the system
- User must not already have the specified role
- SuperAdmin can assign any role to any user

**Implementation Notes:**
- User can have multiple roles simultaneously
- Assignment is logged for audit purposes
- New role becomes effective immediately
- User needs to log in again for new role to appear in authentication cookie

---

### 4. Remove Role from User
**DELETE** `/api/rolemanagement/user/{userId}/remove/{roleName}`

Removes a role from a user. SuperAdmin cannot remove the SuperAdmin role from themselves.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID
- `roleName` (string, required) - The role to remove

**Response (200 OK):**
```json
{
  "message": "Role 'Admin' removed successfully"
}
```

**Response (400 Bad Request - User Does Not Have Role):**
```json
{
  "message": "User does not have the 'Admin' role"
}
```

**Response (400 Bad Request - Cannot Remove SuperAdmin from Self):**
```json
{
  "message": "You cannot remove the SuperAdmin role from your own account"
}
```

**Status Codes:**
- `200 OK` - Role removed successfully
- `400 Bad Request` - Invalid request or user does not have role
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

**Validation Rules:**
- User must have the role to remove it
- SuperAdmin cannot remove SuperAdmin role from their own account (prevents lockout)
- Other users can have SuperAdmin role removed by another SuperAdmin

**Implementation Notes:**
- User can have multiple roles; removing one does not affect others
- Removal is logged for audit purposes
- Change becomes effective on next login (existing cookie continues to work)

---

### 5. Get Users in Role
**GET** `/api/rolemanagement/{roleName}/users`

Retrieves a list of all users assigned to a specific role.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin")]`

**Path Parameters:**
- `roleName` (string, required) - The role name (e.g., "Admin", "User", "SuperAdmin")

**Response (200 OK):**
```json
[
  {
    "id": "user-id-123",
    "userName": "alice@example.com",
    "email": "alice@example.com",
    "firstName": "Alice",
    "lastName": "Administrator",
    "departmentId": 1,
    "departmentName": "System Administration",
    "isActive": true,
    "emailConfirmed": true,
    "createdAt": "2025-11-10T12:00:00Z",
    "updatedAt": null,
    "roles": ["SuperAdmin"]
  },
  {
    "id": "user-id-456",
    "userName": "bob@example.com",
    "email": "bob@example.com",
    "firstName": "Bob",
    "lastName": "Manager",
    "departmentId": 2,
    "departmentName": "Engineering",
    "isActive": true,
    "emailConfirmed": true,
    "createdAt": "2025-11-09T10:30:00Z",
    "updatedAt": null,
    "roles": ["Admin"]
  }
]
```

**Response (404 Not Found - Role Does Not Exist):**
```json
{
  "message": "Role 'InvalidRole' does not exist"
}
```

**Status Codes:**
- `200 OK` - Users retrieved successfully
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User is not SuperAdmin
- `404 Not Found` - Role not found
- `500 Internal Server Error` - Server error

**Implementation Notes:**
- Returns all users with the specified role (includes users with multiple roles)
- Includes user profile information and all assigned roles
- Roles are loaded efficiently in a single query to avoid N+1 problem

---

## Request/Response Models

### AssignRoleRequest
```csharp
public class AssignRoleRequest
{
    public required string RoleName { get; set; }
}
```

### UserDto (Response)
```csharp
public class UserDto
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IList<string> Roles { get; set; }
}
```

---

## Role System Configuration

### Default Roles

Three roles are automatically created on application startup by `RoleSeederService`:

| Role | Purpose | Permissions |
|------|---------|-------------|
| **SuperAdmin** | Full system access | • Manage all users and roles<br>• Create/update/delete any account<br>• Assign/remove any role<br>• Manage departments |
| **Admin** | Moderate system access | • Manage User-role accounts only<br>• Cannot manage SuperAdmin or Admin accounts<br>• Can manage departments<br>• Cannot assign Admin/SuperAdmin roles |
| **User** | Standard user | • View own profile<br>• Update own profile<br>• Access standard features |

### Multiple Roles

Users can have multiple roles simultaneously. For example, a user could have both "SuperAdmin" and "Admin" roles. In practice, a SuperAdmin role implicitly has all permissions, so additional roles are redundant.

---

## Usage Examples

### Example 1: Assign Admin Role to User
```bash
curl -X POST http://localhost:5164/api/rolemanagement/user/user-id-123/assign \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"roleName": "Admin"}'
```

### Example 2: Remove User Role
```bash
curl -X DELETE http://localhost:5164/api/rolemanagement/user/user-id-123/remove/Admin \
  -b cookies.txt
```

### Example 3: Get All Admins
```bash
curl -X GET http://localhost:5164/api/rolemanagement/Admin/users \
  -b cookies.txt
```

### Example 4: Check User Roles
```bash
curl -X GET http://localhost:5164/api/rolemanagement/user/user-id-123 \
  -b cookies.txt
```

### Example 5: Using REST Client in VS Code
```http
### Get all roles
GET http://localhost:5164/api/rolemanagement

### Get user roles
GET http://localhost:5164/api/rolemanagement/user/user-id-123

### Assign Admin role
POST http://localhost:5164/api/rolemanagement/user/user-id-123/assign
Content-Type: application/json

{
  "roleName": "Admin"
}

### Remove Admin role
DELETE http://localhost:5164/api/rolemanagement/user/user-id-123/remove/Admin

### Get all users with Admin role
GET http://localhost:5164/api/rolemanagement/Admin/users
```

---

## Role Assignment Workflow

### Typical User Promotion Flow

1. **SuperAdmin creates new user** with "User" role via `UserManagementController.CreateUser()`
2. **SuperAdmin promotes user** to "Admin" via `POST /api/rolemanagement/user/{userId}/assign`
3. **User logs in** and receives updated authentication cookie with new role
4. **User can now** perform admin functions

### Demoting User from Admin

1. **SuperAdmin calls** `DELETE /api/rolemanagement/user/{userId}/remove/Admin`
2. **User's Admin role** is removed immediately
3. **User remains logged in** until cookie expires or user logs out
4. **On next login**, user receives cookie with only remaining roles

---

## Authorization Hierarchy

```
SuperAdmin (Full Control)
    ├── Can assign/remove any role
    ├── Can manage all users
    ├── Can manage departments
    └── Cannot remove SuperAdmin from self

Admin (Limited Control)
    ├── Can manage only "User" role accounts
    ├── Cannot manage other Admins or SuperAdmins
    ├── Can manage departments
    └── Cannot assign Admin or SuperAdmin roles

User (Standard Access)
    ├── Can view/update own profile only
    ├── Cannot manage other users
    └── Cannot manage roles or departments
```

---

## Security Considerations

1. **SuperAdmin-Only Access:** All role management endpoints require SuperAdmin role
2. **Self-Removal Prevention:** SuperAdmin cannot remove SuperAdmin role from themselves
3. **Role Validation:** All role operations validate role existence
4. **User Validation:** All user operations validate user existence
5. **Duplicate Prevention:** Cannot assign a role user already has
6. **Audit Logging:** All role changes are logged for audit purposes

---

## Related Controllers

- [User Management Controller](./USER_MANAGEMENT_CONTROLLER.md) - Create and manage user accounts
- [Authentication Controller](./AUTHENTICATION_CONTROLLER.md) - User login and profile management
- [Department Controller](./DEPARTMENT_CONTROLLER.md) - Manage departments

---

## Related Files

- **Controller:** `Controllers/RoleManagementController.cs`
- **Services:** `Services/RoleSeederService.cs`
- **Models:** `Models/ApplicationUser.cs`, `Models/UserDto.cs`
- **Database Context:** `Data/AppDbContext.cs`
