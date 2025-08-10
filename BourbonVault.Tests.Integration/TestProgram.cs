using BourbonVault.API.Services;
using BourbonVault.Core.Models;
using BourbonVault.Core.Repositories;
using BourbonVault.Core.Services;
using BourbonVault.Data;
using BourbonVault.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace BourbonVault.Tests.Integration
{
    // This class is used as a startup class for tests
    // It doesn't have a Main method to avoid multiple entry points
    public class TestProgram
    {
        // This method will be called by the test infrastructure
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TestStartup>();
                });
    }

    public class TestStartup
    {
        // The test configuration for our application
        public void ConfigureServices(IServiceCollection services)
        {
            // Add controllers for API - scan for assemblies containing controllers
            services.AddControllers()
                .AddApplicationPart(typeof(BourbonVault.API.Controllers.BottlesController).Assembly);

            // Add Swagger services
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bourbon Vault API", Version = "v1" });
                
                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
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
                        Array.Empty<string>()
                    }
                });
            });

            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add database context - USING IN-MEMORY DATABASE FOR TESTS
            string databaseName = $"InMemoryDb_{Guid.NewGuid()}";
            services.AddDbContext<BourbonVaultContext>(options =>
                options.UseInMemoryDatabase(databaseName));

            // Add Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<BourbonVaultContext>()
            .AddDefaultTokenProviders();

            // Configure JWT Authentication - make sure key is at least 512 bits for HMACSHA512
            var secretKey = "TestSecretKeyForIntegrationTestsOnly123456789012345678901234567890123456789012345";
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = "BourbonVaultAPI",
                    ValidateAudience = true,
                    ValidAudience = "BourbonVaultClients",
                    ValidateLifetime = true
                };
                
                // Required for the TestStartup to configure the same JWT settings
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
            });

            // Add repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IBottleRepository, BottleRepository>();
            services.AddScoped<ITastingNoteRepository, TastingNoteRepository>();

            // Add services
            services.AddScoped<IAuthService, AuthService>();
        }
        
        // Configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // Enable CORS
            app.UseCors("AllowAll");

            app.UseRouting();
            
            // Authentication middleware must be after UseRouting but before UseEndpoints
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Initialize database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<BourbonVaultContext>();
                dbContext.Database.EnsureCreated();
                
                // Create default roles
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { "Admin", "User" };
                
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }
            }
        }
    }
}
