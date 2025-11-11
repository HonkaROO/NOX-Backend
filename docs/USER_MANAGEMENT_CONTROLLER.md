# User Management Controller

**Route:** `/api/usermanagement`

**Authorization Required:** `SuperAdmin` or `Admin` role

## Overview

The User Management Controller handles user account creation, updating, deactivation, and password management. It implements role-based authorization where:
- **SuperAdmin** can manage all users
- **Admin** can only manage User-role accounts (not SuperAdmin or Admin accounts)

## Endpoints

### 1. Get All Users
**GET** `/api/usermanagement`

Retrieves a list of all users accessible to the current administrator. SuperAdmin sees all users; Admin sees only User-role accounts.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin, Admin")]`

**Query Parameters:** None

**Response (200 OK):**
```json
[
  {
    "id": "user-id-123",
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "departmentId": 2,
    "departmentName": "Engineering",
    "isActive": true,
    "emailConfirmed": true,
    "createdAt": "2025-11-10T14:00:00Z",
    "updatedAt": null,
    "roles": ["User"]
  },
  {
    "id": "user-id-456",
    "userName": "jane.smith@example.com",
    "email": "jane.smith@example.com",
    "firstName": "Jane",
    "lastName": "Smith",
    "departmentId": 3,
    "departmentName": "Sales",
    "isActive": true,
    "emailConfirmed": true,
    "createdAt": "2025-11-09T10:30:00Z",
    "updatedAt": null,
    "roles": ["User"]
  }
]
```

**Status Codes:**
- `200 OK` - Users retrieved successfully
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - User lacks required role (SuperAdmin or Admin)
- `500 Internal Server Error` - Server error

**Implementation Notes:**
- SuperAdmin sees all users
- Admin sees only users with "User" role
- Response is filtered based on admin's role
- Roles are loaded efficiently in a single query to avoid N+1 problem

---

### 2. Get User by ID
**GET** `/api/usermanagement/{userId}`

Retrieves a specific user's information by their ID. Admin can only retrieve User-role accounts.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin, Admin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID

**Response (200 OK):**
```json
{
  "id": "user-id-123",
  "userName": "john.doe@example.com",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "departmentId": 2,
  "departmentName": "Engineering",
  "isActive": true,
  "emailConfirmed": true,
  "createdAt": "2025-11-10T14:00:00Z",
  "updatedAt": null,
  "roles": ["User"]
}
```

**Status Codes:**
- `200 OK` - User retrieved successfully
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Admin trying to access SuperAdmin/Admin user
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

---

### 3. Create User
**POST** `/api/usermanagement`

Creates a new user account with specified credentials and role. SuperAdmin can create any role; Admin can only create User-role accounts.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin, Admin")]`

**Request Body:**
```json
{
  "userName": "john.doe@example.com",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "departmentId": 2,
  "role": "User"
}
```

**Response (201 Created):**
```json
{
  "id": "user-id-789",
  "userName": "john.doe@example.com",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "departmentId": 2,
  "departmentName": "Engineering",
  "isActive": true,
  "emailConfirmed": true,
  "createdAt": "2025-11-11T15:45:00Z",
  "updatedAt": null,
  "roles": ["User"]
}
```

**Response (400 Bad Request - Duplicate Email):**
```json
{
  "message": "User with this email already exists"
}
```

**Response (400 Bad Request - Invalid Department):**
```json
{
  "message": "Department not found"
}
```

**Response (403 Forbidden - Insufficient Permissions):**
```
Access denied (Admin cannot create non-User roles)
```

**Status Codes:**
- `201 Created` - User created successfully
- `400 Bad Request` - Invalid request or validation failure
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Admin trying to create non-User role
- `500 Internal Server Error` - Server error

**Validation Rules:**
- Email must be unique (cannot duplicate existing email)
- Username must be unique
- Password must meet ASP.NET Core Identity password requirements:
  - Minimum 6 characters
  - Must contain uppercase letter
  - Must contain lowercase letter
  - Must contain digit
  - Must contain special character
- Department must exist in database
- Role must exist (defaults to "User" if not specified)
- Admin can only assign "User" role to new accounts

**Implementation Notes:**
- Creates account with `EmailConfirmed = true`
- Account is active by default (`IsActive = true`)
- Timestamps set to current UTC time
- Role assignment happens after user creation

---

### 4. Update User
**PUT** `/api/usermanagement/{userId}`

Updates user information including name, department, and active status. Admin can only update User-role accounts.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin, Admin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID

**Request Body:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "departmentId": 3,
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "id": "user-id-123",
  "userName": "john.doe@example.com",
  "email": "john.doe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "departmentId": 3,
  "departmentName": "Sales",
  "isActive": true,
  "emailConfirmed": true,
  "createdAt": "2025-11-10T14:00:00Z",
  "updatedAt": "2025-11-11T16:20:00Z",
  "roles": ["User"]
}
```

**Status Codes:**
- `200 OK` - User updated successfully
- `400 Bad Request` - Invalid request or validation failure
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Admin trying to update SuperAdmin/Admin user
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

**Validation Rules:**
- All fields are optional; only provided fields are updated
- Empty strings do not update fields
- Department must exist if specified
- IsActive can be `true` or `false`

**Implementation Notes:**
- Updates `UpdatedAt` timestamp to current UTC time
- Does not change password (use reset-password endpoint)
- Does not change email or username

---

### 5. Deactivate User
**DELETE** `/api/usermanagement/{userId}`

Deactivates a user account by setting `IsActive = false`. Deactivated users cannot log in. Admin can only deactivate User-role accounts.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin, Admin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID

**Response (204 No Content):**
No response body (successful deletion)

**Response (400 Bad Request - Self-Deactivation):**
```json
{
  "message": "You cannot deactivate your own account"
}
```

**Status Codes:**
- `204 No Content` - User deactivated successfully
- `400 Bad Request` - Cannot deactivate own account
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Admin trying to deactivate SuperAdmin/Admin user
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

**Implementation Notes:**
- Sets `IsActive = false` (soft delete)
- Updates `UpdatedAt` timestamp
- Deactivated users cannot log in (checked in AuthenticationController)
- Does not delete user record from database
- Old authentication cookies become invalid

---

### 6. Reset User Password
**POST** `/api/usermanagement/{userId}/reset-password`

Resets a user's password to a new value. Admin can only reset passwords for User-role accounts.

**Authentication Required:** Yes - `[Authorize(Roles = "SuperAdmin, Admin")]`

**Path Parameters:**
- `userId` (string, required) - The user's unique ID

**Request Body:**
```json
{
  "newPassword": "NewSecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password has been reset successfully"
}
```

**Response (400 Bad Request - Weak Password):**
```json
{
  "message": "Failed to set new password",
  "errors": ["Password does not meet complexity requirements"]
}
```

**Status Codes:**
- `200 OK` - Password reset successfully
- `400 Bad Request` - Invalid password or operation failed
- `401 Unauthorized` - Not authenticated
- `403 Forbidden` - Admin trying to reset password for SuperAdmin/Admin user
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

**Password Validation Rules:**
- Minimum 6 characters
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain digit
- Must contain special character

**Implementation Notes:**
- Removes old password first
- Then adds new password
- User can log in immediately with new password
- Does not affect existing authentication cookies (they continue to work until expiration)

---

## Request/Response Models

### CreateUserRequest
```csharp
public class CreateUserRequest
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required int DepartmentId { get; set; }
    public string? Role { get; set; } = "User";
}
```

### UpdateUserRequest
```csharp
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? DepartmentId { get; set; }
    public bool? IsActive { get; set; }
}
```

### ResetPasswordRequest
```csharp
public class ResetPasswordRequest
{
    public required string NewPassword { get; set; }
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

## Authorization Rules

| Endpoint | SuperAdmin | Admin | User | Anonymous |
|----------|:----------:|:-----:|:----:|:---------:|
| GET all users | ✓ (all users) | ✓ (User-role only) | ✗ | ✗ |
| GET user by ID | ✓ (all users) | ✓ (User-role only) | ✗ | ✗ |
| POST (create) | ✓ (any role) | ✓ (User-role only) | ✗ | ✗ |
| PUT (update) | ✓ (all users) | ✓ (User-role only) | ✗ | ✗ |
| DELETE (deactivate) | ✓ (all users) | ✓ (User-role only) | ✗ | ✗ |
| POST reset-password | ✓ (all users) | ✓ (User-role only) | ✗ | ✗ |

---

## Usage Examples

### Example 1: Create a New User
```bash
curl -X POST http://localhost:5164/api/usermanagement \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "userName": "john.doe@example.com",
    "email": "john.doe@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe",
    "departmentId": 2,
    "role": "User"
  }'
```

### Example 2: Update User Information
```bash
curl -X PUT http://localhost:5164/api/usermanagement/user-id-123 \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{
    "firstName": "Jonathan",
    "departmentId": 3
  }'
```

### Example 3: Reset User Password
```bash
curl -X POST http://localhost:5164/api/usermanagement/user-id-123/reset-password \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"newPassword": "NewPassword123!"}'
```

### Example 4: Deactivate User
```bash
curl -X DELETE http://localhost:5164/api/usermanagement/user-id-123 \
  -b cookies.txt
```

---

## Related Controllers

- [Authentication Controller](./AUTHENTICATION_CONTROLLER.md) - User login and profile management
- [Role Management Controller](./ROLE_MANAGEMENT_CONTROLLER.md) - Assign and manage user roles
- [Department Controller](./DEPARTMENT_CONTROLLER.md) - Manage departments

---

## Related Files

- **Controller:** `Controllers/UserManagementController.cs`
- **Models:** `Models/ApplicationUser.cs`, `Models/UserDto.cs`
- **Database Context:** `Data/AppDbContext.cs`
