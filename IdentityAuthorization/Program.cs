using System.Security.Claims;
using IdentityAuthorization.Authorization;
using IdentityAuthorization.Data;
using IdentityAuthorization.Features;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Required for password reset tokens
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters=new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidIssuer=builder.Configuration["Jwt:Issuer"],
        ValidAudience=builder.Configuration["Jwt:Audience"],
        IssuerSigningKey=new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { Roles.Admin, Roles.Member };
    foreach (var role in roles)
    {
        var roleExist = await roleManager.FindByNameAsync(role);
        if (roleExist is null)
        {
            await roleManager.CreateAsync(roleExist=new IdentityRole(role));
            await roleManager.AddClaimAsync(roleExist,
                new Claim(CustomClaims.Permissions, Permissions.UsersRead));
            await roleManager.AddClaimAsync(roleExist,
                new Claim(CustomClaims.Permissions, Permissions.UsersUpdate));
            await roleManager.AddClaimAsync(roleExist,
                new Claim(CustomClaims.Permissions, Permissions.UsersDelete));
            
        }
    }
}



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();
RegisterUser.MapEndpoint(app);
LoginUser.MapEndpoint(app);
ResetPassword.MapEndpoint(app); // NEW: Password reset endpoints
app.MapGet("me-role", (ClaimsPrincipal claimsPrincipal) =>
{
    return Results.Ok(claimsPrincipal.Claims.GroupBy(c=>c.Type)
        .ToDictionary(g=>g.Key,
            g=>g.Select(c=>c.Value).ToArray()));
}).RequireAuthorization(policy=>policy.RequireRole(Roles.Member));
app.MapGet("me-permissions", (ClaimsPrincipal claimsPrincipal) =>
{
    return Results.Ok(claimsPrincipal.Claims.GroupBy(c=>c.Type)
        .ToDictionary(g=>g.Key,
            g=>g.Select(c=>c.Value).ToArray()));
}).RequireAuthorization(policy=>policy.RequirePermissions(Permissions.UsersRead));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}