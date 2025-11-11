# CORS Configuration for React/Vite Frontend

This document explains the CORS (Cross-Origin Resource Sharing) configuration for the NOX-Backend API, enabling seamless integration with React and Vite development servers.

## Overview

**CORS** is a browser security mechanism that controls which external domains can access your API. Since frontend applications often run on different ports or domains than the backend API during development, CORS configuration is essential.

**Key Points:**
- CORS is **development-only** - The policy only applies when running in Development environment
- Cookie-based authentication requires special CORS configuration - `AllowCredentials: true` must be set
- Allowed origins are defined in `appsettings.Development.json` for easy customization

## Configuration Details

### Allowed Origins (Development)

The following origins are permitted to access the API during development:

**Vite Default Ports:**
- `http://localhost:5173`
- `http://localhost:5174`
- `http://127.0.0.1:5173`
- `http://127.0.0.1:5174`

**React Create-React-App / Traditional Ports:**
- `http://localhost:3000`
- `http://localhost:3001`
- `http://127.0.0.1:3000`
- `http://127.0.0.1:3001`

### Allowed Methods

The following HTTP methods are permitted:
- `GET` - Retrieve data
- `POST` - Create resources
- `PUT` - Update resources
- `DELETE` - Remove resources
- `OPTIONS` - CORS preflight requests (automatic)

### Allowed Headers

The following request headers are permitted:
- `Content-Type` - Required for JSON request bodies
- `Authorization` - For Bearer token authentication (if needed alongside cookies)

### Credentials (Cookies)

- **AllowCredentials:** `true` - **Critical for cookie-based authentication**
- Cookies from responses are automatically sent by the browser on subsequent requests
- This is **required** for ASP.NET Core Identity cookie authentication to work across origins

### Max Age

- **MaxAge:** `3600` seconds (1 hour)
- Browser caches CORS preflight responses for 1 hour to reduce preflight requests
- After 1 hour, browser will re-validate with an OPTIONS request

## Configuration Files

### appsettings.Development.json

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:5174",
      "http://localhost:3000",
      "http://localhost:3001",
      "http://127.0.0.1:5173",
      "http://127.0.0.1:5174",
      "http://127.0.0.1:3000",
      "http://127.0.0.1:3001"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    "AllowedHeaders": ["Content-Type", "Authorization"],
    "AllowCredentials": true,
    "MaxAge": 3600
  }
}
```

### Program.cs Registration

**Service Registration** (Program.cs:34-51):
```csharp
// Configure CORS for development (React/Vite frontend)
if (builder.Environment.IsDevelopment())
{
    var corsConfig = builder.Configuration.GetSection("Cors");
    var allowedOrigins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCorsPolicy", policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}
```

**Middleware Registration** (Program.cs:92-96):
```csharp
// Apply CORS policy for development
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCorsPolicy");
}
```

## How It Works

### Request Flow with CORS

1. **Browser sends preflight request** (for non-simple requests like POST with JSON)
   - Request: `OPTIONS /api/endpoint`
   - Headers: `Origin: http://localhost:5173`, `Access-Control-Request-Method: POST`

2. **Backend validates CORS policy**
   - Checks if origin is in `AllowedOrigins`
   - Checks if method is in `AllowedMethods`
   - Checks if headers are in `AllowedHeaders`

3. **Backend responds to preflight**
   - Response headers include:
     - `Access-Control-Allow-Origin: http://localhost:5173`
     - `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
     - `Access-Control-Allow-Headers: Content-Type, Authorization`
     - `Access-Control-Allow-Credentials: true`
     - `Access-Control-Max-Age: 3600`

4. **Browser caches preflight response**
   - For 1 hour (MaxAge: 3600), no additional preflight needed for same endpoint

5. **Browser sends actual request**
   - Request includes authentication cookie (if set)
   - Response cookie is automatically sent by browser on subsequent requests

### Example: Login Flow with Cookies

```javascript
// Frontend (React/Vite)

// 1. Send login request
const response = await fetch('http://localhost:5164/api/authentication/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include', // IMPORTANT: Include cookies in requests
  body: JSON.stringify({ email: 'user@example.com', password: 'password123' })
});

// 2. Browser receives Set-Cookie header and stores the cookie
// 3. Browser automatically includes cookie on subsequent requests

// 4. Send authenticated request (cookie automatically included)
const userResponse = await fetch('http://localhost:5164/api/authentication/me', {
  method: 'GET',
  credentials: 'include', // IMPORTANT: Include cookies in requests
});

// 5. Backend validates cookie and processes request
```

**Critical Note:** Always use `credentials: 'include'` in `fetch()` calls to ensure cookies are sent with cross-origin requests.

## Adding New Frontend Origins

To add support for additional frontend origins during development:

1. **Open** `appsettings.Development.json`
2. **Add the new origin** to the `Cors.AllowedOrigins` array:
   ```json
   "AllowedOrigins": [
     "http://localhost:5173",
     "http://localhost:5175",  // New Vite instance
     "http://192.168.1.100:3000" // Remote dev machine
   ]
   ```
3. **Save and restart** the backend application
4. **No code changes needed** - Configuration is read at startup

## Production Considerations

**CORS in Production:**

- `appsettings.json` (production) does **not** include CORS configuration
- CORS middleware is only registered in **Development** environment
- Production deployments should use explicit, trusted origins (not wildcards)
- Consider using HTTPS-only origins in production
- Update `AllowCredentials`, `AllowedMethods`, `AllowedHeaders` based on security requirements

**Example Production Setup (not implemented, for reference):**
```csharp
if (app.Environment.IsProduction())
{
    var productionOrigins = new[] { "https://app.example.com" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ProductionCorsPolicy", policy =>
        {
            policy
                .WithOrigins(productionOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}
```

## Testing CORS

### Using Postman (Simple Requests)

1. Set URL: `http://localhost:5164/api/authentication/me`
2. Set Method: `GET`
3. Add Header: `Cookie: .AspNetCore.Identity.Application=<value>` (from login response)
4. Send request - Should return 200 OK

### Using cURL (Preflight Simulation)

```bash
# Simulate preflight OPTIONS request
curl -X OPTIONS http://localhost:5164/api/authentication/me \
  -H "Origin: http://localhost:5173" \
  -H "Access-Control-Request-Method: GET" \
  -v

# Expected response headers:
# Access-Control-Allow-Origin: http://localhost:5173
# Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
# Access-Control-Allow-Credentials: true
```

### Using Browser Dev Tools

1. Start frontend at `http://localhost:5173`
2. Open browser DevTools (F12)
3. Open Console tab
4. Execute a fetch request:
   ```javascript
   fetch('http://localhost:5164/api/authentication/me', {
     credentials: 'include'
   }).then(r => r.json()).then(console.log)
   ```
5. Check Network tab for preflight request (OPTIONS)
6. Verify response includes `Access-Control-Allow-Origin` header

### Using REST Client Extension (VS Code)

See the `NOX-Backend.http` file in the project root for pre-configured requests that work with CORS.

## Troubleshooting

### Issue: CORS Error in Browser Console

**Error:** `Access to XMLHttpRequest at 'http://localhost:5164/api/...' from origin 'http://localhost:5173' has been blocked by CORS policy`

**Solution:**
1. Verify frontend origin is in `Cors.AllowedOrigins` in `appsettings.Development.json`
2. Restart the backend application (configuration is read at startup)
3. Check that the backend is running in **Development** environment
4. Verify request method is in `AllowedMethods` (GET, POST, PUT, DELETE)
5. Verify request headers are in `AllowedHeaders` (Content-Type, Authorization)

### Issue: Cookies Not Being Sent

**Problem:** Login succeeds, but subsequent requests return 401 Unauthorized

**Solution:**
1. Verify frontend uses `credentials: 'include'` in all fetch requests
2. Verify `AllowCredentials: true` is set in CORS configuration
3. Check browser DevTools Network tab:
   - Login response should have `Set-Cookie` header
   - Subsequent requests should have `Cookie` header
4. Verify cookie is not marked as `Secure` (HTTPS only) when using HTTP for development

### Issue: Preflight Request Fails

**Problem:** OPTIONS request returns 405 Method Not Allowed or doesn't include CORS headers

**Solution:**
1. Verify CORS middleware is registered **before** `app.UseAuthorization()`
2. Verify `app.UseCors("DevelopmentCorsPolicy")` is called in Development
3. Restart the application
4. Check that OPTIONS method is in `AllowedMethods`

### Issue: "CORS policy: Credentials mode is 'include'"

**Error:** `CORS policy: Credentials mode is 'include', but 'Access-Control-Allow-Credentials' header is missing from the CORS response`

**Solution:**
1. Verify `AllowCredentials: true` is set in `appsettings.Development.json`
2. Verify CORS policy includes `.AllowCredentials()` in Program.cs
3. Restart the backend application
4. Clear browser cookies and try again

## Security Notes

### Development vs. Production

- **Development CORS:** Permissive (allows multiple localhost ports and IP variations)
- **Production CORS:** Restrictive (only trusted frontend domains)
- **Never use wildcards** (`*`) with `AllowCredentials: true` - browser blocks this for security

### Cookie Security

- HttpOnly cookies are set by default (JavaScript cannot access them)
- SameSite policy enforced (CSRF protection)
- Cookies sent only over secure HTTPS in production
- `Secure` flag set for production cookies

### Header Validation

- All requests validated against `AllowedHeaders` whitelist
- Custom headers must be explicitly allowed
- Authorization header allowed for Bearer tokens (optional, cookies are primary auth method)

## References

- [Microsoft CORS Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [MDN CORS Guide](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
- [ASP.NET Core Identity with Cookies](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)

## Related Configuration Files

- `appsettings.Development.json` - CORS settings
- `Program.cs` - CORS registration and middleware
- `Properties/launchSettings.json` - Launch profiles (HTTP port 5164, HTTPS port 7238)
- `NOX-Backend.http` - REST Client requests for manual testing
