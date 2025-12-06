using System.Security.Claims;
using System.Text;
using IdentityAuthorization.Authorization;
using IdentityAuthorization.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAuthorization.Features;

/// <summary>
/// Handles user login functionality including JWT token generation.
/// </summary>
public static class LoginUser
{
    /// <summary>
    /// Request model for user login.
    /// </summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    public record LoginRequest(string Email, string Password);

    /// <summary>
    /// Response model containing the authentication token.
    /// </summary>
    /// <param name="Token">The JWT authentication token.</param>
    public record LoginResponse(string Token);

    /// <summary>
    /// Maps the login endpoint to the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/login", HandleLoginAsync)
            .WithName("LoginUser")
            .WithOpenApi()
            .AllowAnonymous();
    }

    /// <summary>
    /// Handles the login request, validates credentials, and generates JWT token.
    /// </summary>
    /// <param name="loginRequest">The login request containing email and password.</param>
    /// <param name="userManager">The ASP.NET Core Identity user manager.</param>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A result containing the JWT token or an unauthorized response.</returns>
    private static async Task<IResult> HandleLoginAsync(
        LoginRequest loginRequest,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        // Find user by email
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        // Verify password
        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!isPasswordValid)
        {
            return Results.Unauthorized();
        }

        // Get user roles
        var roles = await userManager.GetRolesAsync(user);

        // Get permissions from role claims
        var permissions = await GetUserPermissionsAsync(dbContext, roles);

        // Build claims for JWT token
        var claims = BuildUserClaims(user, roles, permissions);

        // Generate JWT token
        var token = GenerateJwtToken(claims, configuration);

        return Results.Ok(new LoginResponse(token));
    }

    /// <summary>
    /// Retrieves permissions associated with user roles from the database.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>An array of distinct permission values.</returns>
    private static async Task<string[]> GetUserPermissionsAsync(
        ApplicationDbContext dbContext,
        IList<string> roles)
    {
        return await (
            from role in dbContext.Roles
            join claim in dbContext.RoleClaims on role.Id equals claim.RoleId
            where roles.Contains(role.Name!) && claim.ClaimType == CustomClaims.Permissions
            select claim.ClaimValue!
        )
        .Distinct()
        .ToArrayAsync();
    }

    /// <summary>
    /// Builds the claims collection for the JWT token.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="roles">The user's roles.</param>
    /// <param name="permissions">The user's permissions.</param>
    /// <returns>A list of claims.</returns>
    private static List<Claim> BuildUserClaims(
        ApplicationUser user,
        IList<string> roles,
        string[] permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!)
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Add permission claims
        claims.AddRange(permissions.Select(permission => new Claim(CustomClaims.Permissions, permission)));

        return claims;
    }

    /// <summary>
    /// Generates a JWT token with the provided claims and configuration.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The generated JWT token string.</returns>
    private static string GenerateJwtToken(List<Claim> claims, IConfiguration configuration)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var tokenHandler = new JsonWebTokenHandler();
        return tokenHandler.CreateToken(tokenDescriptor);
    }
}