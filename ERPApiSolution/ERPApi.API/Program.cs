using ERPApi.API.Middleware;
using ERPApi.Application.Extensions;
using ERPApi.Core.Entities;
using ERPApi.Infrastructure.Data;
using ERPApi.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Clear default logging providers
builder.Logging.ClearProviders();

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });


builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerServices();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database
//await SeedDatabase(app);

app.Run();

//async Task SeedDatabase(WebApplication app)
//{
//    using var scope = app.Services.CreateScope();
//    var services = scope.ServiceProvider;

//    try
//    {
//        var context = services.GetRequiredService<ApplicationDbContext>();
//        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

//        await context.Database.MigrateAsync();

//        // Create admin user if not exists
//        var adminEmail = "admin@erp.com";
//        var adminUser = await userManager.FindByEmailAsync(adminEmail);
//        if (adminUser == null)
//        {
//            adminUser = new ApplicationUser
//            {
//                FirstName = "Admin",
//                LastName = "User",
//                Email = adminEmail,
//                UserName = adminEmail,
//                IsActive = true,
//                CreatedDate = DateTime.UtcNow
//            };

//            var result = await userManager.CreateAsync(adminUser, "Admin@123");
//            if (result.Succeeded)
//            {
//                var adminRole = await roleManager.FindByNameAsync("Admin");
//                if (adminRole != null)
//                {
//                    await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
//                }
//            }
//        }
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the database");
//    }
//}