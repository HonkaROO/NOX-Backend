using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NOX_Backend.Models;

namespace NOX_Backend.Services;

/// <summary>
/// Custom claims principal factory that enriches the ClaimsPrincipal with role claims and custom user claims.
/// Role claims are added by the base class; we add custom claims like FullName, Department, and IsActive status.
/// These claims are serialized into the authentication cookie and extracted on each request for authorization decisions.
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
    /// Override to add custom claims to the ClaimsPrincipal.
    /// Role claims are already added by the base class via the RoleClaimsFactory.
    /// We add additional custom claims: FullName, Department, and IsActive status.
    /// </summary>
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        if (principal?.Identity is ClaimsIdentity identity)
        {
            // Get department name for claim
            var department = await _context.Departments.FindAsync(user.DepartmentId);
            var departmentName = department?.Name ?? "";

            // Add custom user claims (role claims are already added by base class)
            identity.AddClaim(new Claim("FullName", user.GetFullName()));
            identity.AddClaim(new Claim("DepartmentId", user.DepartmentId.ToString()));
            identity.AddClaim(new Claim("DepartmentName", departmentName));
            identity.AddClaim(new Claim("IsActive", user.IsActive.ToString()));
        }

        return principal ?? throw new InvalidOperationException("Failed to create principal for user");
    }
}
