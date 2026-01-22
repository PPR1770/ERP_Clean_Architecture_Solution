using ERPApi.Core.Entities;
using ERPApi.Core.Interfaces;
using ERPApi.Infrastructure.Data;
using ERPApi.Infrastructure.Repositories;
using ERPApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ERPApi.Infrastructure.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("=== STARTING INFRASTRUCTURE SERVICES SETUP ===");

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            Console.WriteLine("Database context configured");

            // Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            Console.WriteLine("Identity services configured");

            // JWT Configuration Validation
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];
            var tokenExpiration = configuration["Jwt:TokenExpirationMinutes"];
            var refreshTokenExpiration = configuration["Jwt:RefreshTokenExpirationDays"];

            Console.WriteLine("\n=== JWT CONFIGURATION VALIDATION ===");
            Console.WriteLine($"Jwt:Key: {(string.IsNullOrEmpty(jwtKey) ? "❌ MISSING" : $"✅ Present ({jwtKey!.Length} chars)")}");
            Console.WriteLine($"Jwt:Issuer: {jwtIssuer ?? "❌ MISSING"}");
            Console.WriteLine($"Jwt:Audience: {jwtAudience ?? "❌ MISSING"}");
            Console.WriteLine($"Jwt:TokenExpirationMinutes: {tokenExpiration ?? "❌ MISSING"}");
            Console.WriteLine($"Jwt:RefreshTokenExpirationDays: {refreshTokenExpiration ?? "❌ MISSING"}");
            Console.WriteLine("=====================================\n");

            // Validate JWT Configuration
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentException("JWT Key is not configured. Add 'Jwt:Key' to appsettings.json");
            }

            if (jwtKey.Length < 32)
            {
                Console.WriteLine($"⚠️  WARNING: JWT Key is only {jwtKey.Length} characters. Recommended minimum is 32 characters for HS256.");
            }

            if (string.IsNullOrEmpty(jwtIssuer))
            {
                throw new ArgumentException("JWT Issuer is not configured. Add 'Jwt:Issuer' to appsettings.json");
            }

            if (string.IsNullOrEmpty(jwtAudience))
            {
                throw new ArgumentException("JWT Audience is not configured. Add 'Jwt:Audience' to appsettings.json");
            }

            var keyBytes = Encoding.UTF8.GetBytes(jwtKey!);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };

                // Add event handlers for debugging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"\n❌ JWT Authentication Failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"\n✅ JWT Token Validated for user: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    }
                };
            });

            Console.WriteLine("JWT Authentication configured");

            // Authorization
            services.AddAuthorization(options =>
            {
                // Default policy - require any authenticated user
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                // Admin role policy
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));

                // REMOVE THIS LINE or update it:
                // options.AddPolicy("PermissionPolicy", policy =>
                //     policy.RequireAuthenticatedUser());

                Console.WriteLine("Authorization policies configured");
            });

            Console.WriteLine("Authorization policies configured");

            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            Console.WriteLine("CORS policies configured");

            // Repository and Unit of Work
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuditService, AuditService>();

            Console.WriteLine("=== INFRASTRUCTURE SERVICES SETUP COMPLETE ===\n");

            return services;
        }

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ERP API",
                    Version = "v1",
                    Description = "ERP System API with JWT Authentication",
                    Contact = new OpenApiContact
                    {
                        Name = "ERP Admin",
                        Email = "admin@erp.com"
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // Add XML comments if available
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }
    }
}