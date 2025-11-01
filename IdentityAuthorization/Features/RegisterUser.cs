using IdentityAuthorization.Data;
using Microsoft.AspNetCore.Identity;

namespace IdentityAuthorization.Features;

public static class RegisterUser
{
    public record Request(string Email,string Password,bool EnableNotifications=false);
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/register", async (Request request, UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext) =>
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EnableNotifications = request.EnableNotifications
            };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }
            var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Member);
            if (!addToRoleResult.Succeeded)
            {
                return Results.BadRequest(addToRoleResult.Errors);
            }
            await transaction.CommitAsync();
            return Results.Ok(new { user.Id, user.Email, user.EnableNotifications });
        }).WithName("RegisterUser").WithOpenApi();
    }
}