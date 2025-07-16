using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using ClinicManagement.Data.Context;
using ClinicManagement.Api.Services;
using Microsoft.AspNetCore.Identity; // Make sure this using is present
using ClinicManagement.Data.Models;
using ClinicManagement.Data.Seed;
using System.Text.Json.Serialization; // Make sure this using is present for User and Role

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Configure DbContext
builder.Services.AddDbContext<ClinicManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClinicManagementDbConnection")));

// --- START: ADD MISSING IDENTITY CONFIGURATION ---
// Configure ASP.NET Core Identity
// AddIdentity<TUser, TRole> where TUser is your custom User model and TRole is your custom Role model
builder.Services.AddIdentity<ClinicManagement.Data.Models.User, ClinicManagement.Data.Models.Role>(options =>
{
    // Identity options (e.g., password strength, lockout settings)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ClinicManagementDbContext>() // Tells Identity to use your DbContext
.AddDefaultTokenProviders(); // Provides token generators for password resets, email confirmations etc.
// --- END: ADD MISSING IDENTITY CONFIGURATION ---


// 2. Add Controllers and enable JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ClinicManagement.Api.DTOs.util.DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 3. Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// --- START: Swagger/OpenAPI Configuration for JWT Authorization ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Clinic Management API", Version = "v1" });

    // Define the security scheme for JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                    Enter 'Bearer' [space] and then your token in the text input below.
                    Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add a security requirement for all operations (optional, can be applied per endpoint too)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// --- END: Swagger/OpenAPI Configuration for JWT Authorization ---


// 4. Configure JWT Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured."))),
            ClockSkew = TimeSpan.Zero // Reduces default 5 min grace period for expired tokens
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context => {
                Console.WriteLine("Authentication failed: " + context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully!");
                // Optionally log claims to confirm user identity
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("Authentication challenge: " + context.AuthenticateFailure?.Message);
                return Task.CompletedTask;
            }
        };
    });

// 5. Configure Authorization Policies (if you have specific role-based policies)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ClinicStaff", policy => policy.RequireRole("Admin", "HR", "Receptionist", "Doctor", "Nurse", "InventoryManager"));
    options.AddPolicy("PatientAccess", policy => policy.RequireRole("Patient"));
});

// 6. Register custom services
builder.Services.AddScoped<AuthService>(); // Register AuthService for dependency injection

// 7. Add cors policy (if needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic Management API v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();
// THIS IS CRUCIAL:
// It must be placed after UseRouting() (if you have it)
// and before UseAuthorization() and MapControllers().
app.UseCors(); // This applies the default policy defined above

// 7. Use Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// *** THIS IS THE INTEGRATION POINT FOR SEEDDATA ***
// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicManagementDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ClinicManagement.Data.Models.User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ClinicManagement.Data.Models.Role>>();

    try
    {
        dbContext.Database.Migrate(); // Apply pending migrations

        // Invoke the SeedData method here
        await SeedData.SeedUsersAndRoles(userManager, roleManager, dbContext); // Ensure namespace is correct
    }
    catch (Exception ex)
    {
        // Log errors during migration/seeding
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying migrations or seeding the database.");
    }
}

app.Run();