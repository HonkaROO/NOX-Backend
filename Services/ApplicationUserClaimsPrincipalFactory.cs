using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NOX_Backend.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Custom claims principal factory that adds role claims to the JWT token.
/// This ensures that the JWT token includes the user's roles which can be used for client-side authorization.
/// </summary>
public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    private readonly AppDbContext _context;

    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        AppDbContext context)
        : base(userManager, roleManager, optionsAccessor)
    {
        _context = context;
    }

    /// <summary>
    /// Override to add custom claims including roles to the ClaimsPrincipal.
    /// </summary>
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        if (principal?.Identity is ClaimsIdentity identity)
        {
            // Get user roles and add them as claims
            var roles = await UserManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Get department name for claim
            var department = await _context.Departments.FindAsync(user.DepartmentId);
            var departmentName = department?.Name ?? "";

            // Add custom user claims
            identity.AddClaim(new Claim("FullName", user.GetFullName()));
            identity.AddClaim(new Claim("DepartmentId", user.DepartmentId.ToString()));
            identity.AddClaim(new Claim("DepartmentName", departmentName));
            identity.AddClaim(new Claim("IsActive", user.IsActive.ToString()));

            // Add user ID as "sub" claim if not already present
            if (!identity.HasClaim(ClaimTypes.NameIdentifier, user.Id))
            {
                identity.AddClaim(new Claim("sub", user.Id));
            }
        }

        return principal ?? throw new InvalidOperationException("Failed to create principal for user");
    }
}
