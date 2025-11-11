using System.Security.Claims;
using System.Text;
using IdentityAuthorization.Authorization;
using IdentityAuthorization.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAuthorization.Features;

public static class ResetPassword
{
    // Request to initiate password reset (sends email with token)
    public record ForgotPasswordRequest(string Email);
    
    // Request to actually reset the password with token
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
    
    // Request to change password when user is logged in
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    
    public static void MapEndpoint(WebApplication app)
    {
        // 1. Forgot Password - Generates reset token (would normally send email)
        app.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            ILogger<ForgotPasswordRequest> logger) =>
        {
            // SECURITY: Input validation
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                logger.LogWarning("Forgot password attempt with empty email");
                // SECURITY: Don't reveal if email exists
                return Results.Ok(new { Message = "If the email exists, a password reset link has been sent." });
            }
            
            var user = await userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                // SECURITY: Don't reveal that user doesn't exist
                logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return Results.Ok(new { Message = "If the email exists, a password reset link has been sent." });
            }
            
            // SECURITY: Check if account is locked
            if (await userManager.IsLockedOutAsync(user))
            {
                logger.LogWarning("Password reset attempted for locked account: {Email}", request.Email);
                return Results.Problem(
                    detail: "Account is locked. Please contact support.",
                    statusCode: StatusCodes.Status423Locked);
            }
            
            // Generate password reset token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            
            // SECURITY: Log the action
            logger.LogInformation("Password reset token generated for user: {Email}", request.Email);
            
            // TODO: In production, send this via email service
            // For now, return it in response (ONLY FOR DEVELOPMENT!)
            return Results.Ok(new 
            { 
                Message = "If the email exists, a password reset link has been sent.",
                // SECURITY WARNING: Remove this in production! Send via email instead
                Token = resetToken, // Remove this line in production
                Email = request.Email // Remove this line in production
            });
        })
        .WithName("ForgotPassword")
        .WithOpenApi()
        .RequireRateLimiting("LoginPolicy"); // SECURITY: Rate limiting to prevent abuse
        
        // 2. Reset Password - Uses token to reset password
        app.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            UserManager<ApplicationUser> userManager,
            ILogger<ResetPasswordRequest> logger) =>
        {
            // SECURITY: Input validation
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                logger.LogWarning("Password reset attempt with missing data");
                return Results.BadRequest(new { Error = "All fields are required." });
            }
            
            var user = await userManager.FindByEmailAsync(request.Email);
            
            if (user == null)
            {
                // SECURITY: Don't reveal that user doesn't exist
                logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                return Results.BadRequest(new { Error = "Invalid reset attempt." });
            }
            
            // Reset the password using the token
            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            
            if (!result.Succeeded)
            {
                logger.LogWarning("Password reset failed for user {Email}: {Errors}",
                    request.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                
                return Results.BadRequest(new
                {
                    Error = "Password reset failed.",
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                });
            }
            
            // SECURITY: Reset lockout count on successful password reset
            await userManager.ResetAccessFailedCountAsync(user);
            await userManager.SetLockoutEndDateAsync(user, null);
            
            // SECURITY: Log successful password reset
            logger.LogInformation("Password successfully reset for user: {Email}", request.Email);
            
            return Results.Ok(new
            {
                Message = "Password has been reset successfully. You can now login with your new password."
            });
        })
        .WithName("ResetPassword")
        .WithOpenApi()
        .RequireRateLimiting("LoginPolicy"); // SECURITY: Rate limiting
        
        // 3. Change Password - For logged-in users
        app.MapPost("/change-password", async (
            ChangePasswordRequest request,
            ClaimsPrincipal claimsPrincipal,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            ILogger<ChangePasswordRequest> logger) =>
        {
            // SECURITY: Get user from claims
            var userIdClaim = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null)
            {
                logger.LogWarning("Change password attempt without valid user claim");
                return Results.Unauthorized();
            }
            
            // SECURITY: Input validation
            if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                logger.LogWarning("Change password attempt with missing data for user: {UserId}", userIdClaim.Value);
                return Results.BadRequest(new { Error = "Current password and new password are required." });
            }
            
            var user = await userManager.FindByIdAsync(userIdClaim.Value);
            
            if (user == null)
            {
                logger.LogError("Change password: User not found with ID: {UserId}", userIdClaim.Value);
                return Results.Unauthorized();
            }
            
            // Change the password
            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            
            if (!result.Succeeded)
            {
                logger.LogWarning("Password change failed for user {Email}: {Errors}",
                    user.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                
                // SECURITY: Track failed attempts
                await userManager.AccessFailedAsync(user);
                
                return Results.BadRequest(new
                {
                    Error = "Password change failed.",
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                });
            }
            
            // SECURITY: Reset failed access count on success
            await userManager.ResetAccessFailedCountAsync(user);
            
            // SECURITY: Generate new token after password change
            var roles = await userManager.GetRolesAsync(user);
            var signingCredentials = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(signingCredentials, SecurityAlgorithms.HmacSha256);
            
            var permissions = await (
                from role in dbContext.Roles
                join claim in dbContext.RoleClaims on role.Id equals claim.RoleId
                where roles.Contains(role.Name!) && claim.ClaimType == CustomClaims.Permissions
                select claim.ClaimValue
                ).Distinct()
                .ToArrayAsync();
            
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                ..roles.Select(r => new Claim(ClaimTypes.Role, r)),
                ..permissions.Select(p => new Claim(CustomClaims.Permissions, p!))
            ];
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                NotBefore = DateTime.UtcNow
            };
            
            var tokenHandler = new JsonWebTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            // SECURITY: Log successful password change
            logger.LogInformation("Password successfully changed for user: {Email}", user.Email);
            
            return Results.Ok(new
            {
                Message = "Password has been changed successfully.",
                Token = token, // New token
                ExpiresIn = configuration.GetValue<int>("Jwt:ExpirationInMinutes") * 60,
                TokenType = "Bearer"
            });
        })
        .WithName("ChangePassword")
        .WithOpenApi()
        .RequireAuthorization() // Must be logged in
        .RequireRateLimiting("GlobalPolicy");
    }
}
