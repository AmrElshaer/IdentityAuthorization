using System.Security.Claims;
using System.Text;
using IdentityAuthorization.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAuthorization.Features;

public static class LoginUser
{
    public record LoginRequest(string Email, string Password);
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/login", async (LoginRequest loginRequest, UserManager<Data.ApplicationUser> userManager,
            Data.ApplicationDbContext dbContext, IConfiguration configuration) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null)
            {
                return Results.Unauthorized();
            }
            var result = await userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!result)
            {
                return Results.Unauthorized();
            }

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
                ..roles.Select(r => new Claim(ClaimTypes.Role, r)),
                ..permissions.Select(p=> new Claim(CustomClaims.Permissions, p))
            ];
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
                SigningCredentials = credentials,
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"]
            };
            var tokenHandler = new JsonWebTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Results.Ok(new { Token = token });
        }).WithName("LoginUser").WithOpenApi();
    }
}