using Microsoft.AspNetCore.Authorization;

namespace IdentityAuthorization.Authorization;

public class PermissionAuthorizationRequirement(params string[] allowedPermissions):AuthorizationHandler<PermissionAuthorizationRequirement>,
    IAuthorizationRequirement
{
   public string[] AllowedPermissions { get; } = allowedPermissions;
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement)
    {
        foreach (var permission in requirement.AllowedPermissions)
        {
            var hasPermission = context.User.HasClaim(c => c.Type == CustomClaims.Permissions && c.Value == permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
                break;
            }
        }
        return Task.CompletedTask;
    }
}
public static class PermissionExtensions
{
    public static AuthorizationPolicyBuilder RequirePermissions(this AuthorizationPolicyBuilder builder, params string[] permissions)
    {
        builder.AddRequirements(new PermissionAuthorizationRequirement(permissions));
        return builder;
    }
}