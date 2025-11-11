# Authentication Controller

**Route:** `/api/authentication`

## Overview

The Authentication Controller handles user authentication, logout, and profile management using cookie-based authentication via ASP.NET Core Identity. This controller manages user sessions and provides access to authenticated user data.

## Endpoints

### 1. Login (Public)
**POST** `/api/authentication/login`

Authenticates a user with email and password credentials. Upon successful authentication, an HTTP-only authentication cookie is automatically set in the response headers and will be sent by the browser on all subsequent requests.

**Authentication Required:** No

**Request Body:**
```json
{
  "email": "superadmin@nox.local",
  "password": "SuperAdmin@2024!Nox"
}
```

**Response (200 OK):**
```json
{
  "id": "user-id-123",
  "userName": "superadmin@nox.local",
  "email": "superadmin@nox.local",
  "firstName": "Super",
  "lastName": "Administrator",
  "phone": "+1-555-0123",
  "address": "123 Admin Ave, Tech City, TC 12345",
  "startDate": "2025-01-01T00:00:00Z",
  "employeeId": "NPAX-2025-001",
  "departmentId": 1,
  "departmentName": "System Administration",
  "isActive": true,
  "emailConfirmed": true,
  "createdAt": "2025-11-10T12:00:00Z",
  "updatedAt": null,
  "roles": ["SuperAdmin"]
}
```

**Response (401 Unauthorized):**
```json
{
  "message": "Invalid email or password"
}
```

**Status Codes:**
- `200 OK` - Login successful; authentication cookie set
- `400 Bad Request` - Invalid request body
- `401 Unauthorized` - Invalid credentials or account inactive
- `500 Internal Server Error` - Server error during login

**Implementation Notes:**
- Password is validated via `SignInManager.PasswordSignInAsync()`
- Account active status (`IsActive`) is checked after password validation
- Inactive accounts are rejected even with correct password
- Account lockout is enabled after multiple failed login attempts
- Cookie is automatically set in response headers; no token in response body

---

### 2. Get Current User Profile
**GET** `/api/authentication/me`

Retrieves the profile information of the currently authenticated user from the authentication cookie.

**Authentication Required:** Yes (`[Authorize]`)

**Request Headers:**
```
Cookie: .AspNetCore.Identity.Application=<cookie-value>
```

**Response (200 OK):**
```json
{
  "id": "user-id-123",
  "userName": "superadmin@nox.local",
  "email": "superadmin@nox.local",
  "firstName": "Super",
  "lastName": "Administrator",
  "phone": "+1-555-0123",
  "address": "123 Admin Ave, Tech City, TC 12345",
  "startDate": "2025-01-01T00:00:00Z",
  "employeeId": "NPAX-2025-001",
  "departmentId": 1,
  "departmentName": "System Administration",
  "isActive": true,
  "emailConfirmed": true,
  "createdAt": "2025-11-10T12:00:00Z",
  "updatedAt": null,
  "roles": ["SuperAdmin"]
}
```

**Status Codes:**
- `200 OK` - Profile retrieved successfully
- `401 Unauthorized` - Not authenticated or cookie invalid
- `404 Not Found` - User no longer exists in database
- `500 Internal Server Error` - Server error

---

### 3. Update Current User Profile
**PUT** `/api/authentication/me`

Updates the profile information of the currently authenticated user. Users can only update their own profile (FirstName, LastName, Phone, and Address).

**Authentication Required:** Yes (`[Authorize]`)

**Request Headers:**
```
Cookie: .AspNetCore.Identity.Application=<cookie-value>
Content-Type: application/json
```

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "phone": "+1-555-0456",
  "address": "456 User Lane, Tech City, TC 54321"
}
```

**Response (200 OK):**
```json
{
  "id": "user-id-123",
  "userName": "jane.smith@nox.local",
  "email": "jane.smith@nox.local",
  "firstName": "Jane",
  "lastName": "Smith",
  "phone": "+1-555-0456",
  "address": "456 User Lane, Tech City, TC 54321",
  "startDate": null,
  "employeeId": null,
  "departmentId": 1,
  "departmentName": "Engineering",
  "isActive": true,
  "emailConfirmed": true,
  "createdAt": "2025-11-10T12:00:00Z",
  "updatedAt": "2025-11-11T14:30:00Z",
  "roles": ["User"]
}
```

**Response (400 Bad Request):**
```json
{
  "message": "Failed to update profile",
  "errors": ["Password validation failed"]
}
```

**Status Codes:**
- `200 OK` - Profile updated successfully
- `400 Bad Request` - Invalid request or update failed
- `401 Unauthorized` - Not authenticated
- `404 Not Found` - User not found
- `500 Internal Server Error` - Server error

**Validation Rules:**
- FirstName, LastName, Phone, and Address are optional fields
- Only provided fields will be updated
- Empty strings will not update a field
- Phone must be a valid phone number format (optional validation via `[Phone]` attribute)
- Address max length: 255 characters

---

### 4. Logout
**POST** `/api/authentication/logout`

Logs out the currently authenticated user by clearing the authentication cookie. The user must provide a valid authentication cookie.

**Authentication Required:** Yes (`[Authorize]`)

**Request Headers:**
```
Cookie: .AspNetCore.Identity.Application=<cookie-value>
```

**Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

**Status Codes:**
- `200 OK` - Logout successful; cookie cleared
- `401 Unauthorized` - Not authenticated
- `500 Internal Server Error` - Server error during logout

**Implementation Notes:**
- Calls `SignInManager.SignOutAsync()` to clear authentication state
- Cookie is expired in response headers
- Browser will discard the cookie

---

### 5. Access Denied
**GET** `/api/authentication/access-denied`

Returns a 403 Forbidden response for access denied scenarios. This endpoint is used by the cookie configuration as the access denied path.

**Authentication Required:** No

**Response (403 Forbidden):**
```json
{
  "message": "Access denied"
}
```

**Status Codes:**
- `403 Forbidden` - Access denied to requested resource

---

## Cookie Configuration

The authentication system uses HTTP-only, secure cookies to maintain user sessions:

| Property | Value |
|----------|-------|
| **HttpOnly** | `true` (prevents JavaScript access) |
| **Secure** | `true` (HTTPS only in production) |
| **SameSite** | Strict (CSRF protection) |
| **Expiration** | 7 days |
| **Sliding Expiration** | Enabled (resets on each request) |
| **Login Path** | `/api/authentication/login` |
| **Logout Path** | `/api/authentication/logout` |
| **Access Denied Path** | `/api/authentication/access-denied` |

---

## Authentication Flow

1. **User calls `/login`** with email and password
2. **Password validated** via `SignInManager.PasswordSignInAsync()`
3. **Account active status checked** (IsActive must be true)
4. **Authentication cookie created** with signed claims from `ApplicationUserClaimsPrincipalFactory`
5. **`Set-Cookie` response header sent** with encrypted cookie value
6. **Browser stores cookie** and includes it on subsequent requests
7. **Cookie middleware validates** signature and extracts claims on each request
8. **`User.Claims` populated** with user ID, email, roles, full name, department, and more
9. **Protected endpoints** access `User` principal without additional database lookups

---

## User DTO Response

The response DTO includes the following fields:

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | User's unique ID |
| `userName` | string | User's username (typically email) |
| `email` | string | User's email address |
| `firstName` | string | User's first name |
| `lastName` | string | User's last name |
| `phone` | string? | User's phone number (optional) |
| `address` | string? | User's physical address (optional) |
| `startDate` | DateTime? | User's employment start date (optional) |
| `employeeId` | string? | Unique employee ID (e.g., "NPAX-2024-001", optional) |
| `departmentId` | int | Department ID user belongs to |
| `departmentName` | string | Department name (optional, null if department not found) |
| `isActive` | bool | Account active status |
| `emailConfirmed` | bool | Email confirmation status |
| `createdAt` | DateTime | Account creation timestamp (UTC) |
| `updatedAt` | DateTime? | Last profile update timestamp (UTC, null if never updated) |
| `roles` | List<string> | List of assigned roles (e.g., "SuperAdmin", "Admin", "User") |

---

## Error Handling

All endpoints include comprehensive error handling:

| Scenario | Status Code | Response |
|----------|-------------|----------|
| Invalid request body | `400` | BadRequest with model state errors |
| Invalid credentials | `401` | Unauthorized with message |
| Account inactive | `401` | Unauthorized (password validated but account disabled) |
| Not authenticated | `401` | Unauthorized |
| Resource not found | `404` | NotFound |
| Access denied (role-based) | `403` | Forbidden |
| Server error | `500` | Internal Server Error with generic message |

---

## Usage Examples

### Example 1: Complete Login Flow
```bash
# 1. Login
curl -X POST http://localhost:5164/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"email":"superadmin@nox.local","password":"SuperAdmin@2024!Nox"}' \
  -c cookies.txt

# 2. Get current user (automatic cookie included from cookies.txt)
curl -X GET http://localhost:5164/api/authentication/me \
  -b cookies.txt

# 3. Update profile
curl -X PUT http://localhost:5164/api/authentication/me \
  -H "Content-Type: application/json" \
  -b cookies.txt \
  -d '{"firstName":"Jane","lastName":"Doe"}'

# 4. Logout
curl -X POST http://localhost:5164/api/authentication/logout \
  -b cookies.txt
```

### Example 2: Using REST Client in VS Code
```http
### Login
POST http://localhost:5164/api/authentication/login
Content-Type: application/json

{
  "email": "superadmin@nox.local",
  "password": "SuperAdmin@2024!Nox"
}

### Get current user
GET http://localhost:5164/api/authentication/me

### Update profile
PUT http://localhost:5164/api/authentication/me
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe"
}

### Logout
POST http://localhost:5164/api/authentication/logout
```

---

## Default User Credentials

A default SuperAdmin user is created automatically on application startup:

| Field | Value |
|-------|-------|
| **Email/Username** | `superadmin@nox.local` |
| **Password** | `SuperAdmin@2024!Nox` |
| **Role** | SuperAdmin |
| **Department** | System Administration |

⚠️ **Important:** Change these credentials immediately in production!

---

## Related Controllers

- [User Management Controller](./USER_MANAGEMENT_CONTROLLER.md) - Create and manage user accounts
- [Role Management Controller](./ROLE_MANAGEMENT_CONTROLLER.md) - Assign and manage user roles
- [Department Controller](./DEPARTMENT_CONTROLLER.md) - Manage departments

---

## Security Considerations

1. **Cookie-based Authentication:** Secure by default (HttpOnly, SameSite)
2. **Password Hashing:** Passwords are hashed using PBKDF2 via ASP.NET Core Identity
3. **Account Lockout:** Enabled after multiple failed login attempts
4. **Active Status Check:** Deactivated accounts cannot log in
5. **Session Timeout:** 7-day maximum cookie lifetime with sliding expiration
6. **HTTPS Enforcement:** Cookies marked as Secure in production
7. **CSRF Protection:** SameSite=Strict prevents cross-site cookie transmission

---

## Related Files

- **Controller:** `Controllers/AuthenticationController.cs`
- **Models:** `Models/ApplicationUser.cs`, `Models/UserDto.cs`
- **Database Context:** `Data/AppDbContext.cs`
- **Claims Factory:** `Services/ApplicationUserClaimsPrincipalFactory.cs`
