using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OnlineCourses.Application;
using OnlineCourses.Infrastructure;
using OnlineCourses.Infrastructure.Data;
using OnlineCourses.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineCourses API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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

    // XML Documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new Asp.Versioning.UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Layer Dependencies
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Check if JWT_KEY is present
var jwtKey = builder.Configuration["JWT_KEY"] ?? "SecretKeyForTestingOnly_PleaseChangeInEnvFile_AtLeast32CharsLong";

// Authentication
builder.Services.AddAuthentication(options =>
{
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
        ValidIssuer = builder.Configuration["JWT_ISSUER"] ?? "OnlineCourses",
        ValidAudience = builder.Configuration["JWT_AUDIENCE"] ?? "OnlineCourses",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// CORS (Allow Frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Use Swagger in all environments for this test assessment to make it easy to test
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

        // Database Seeding & Migration
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();
            var context = services.GetRequiredService<AppDbContext>();
            
            int retries = 10;
            while (retries > 0)
            {
                try
                {
                    logger.LogInformation("Attempting to apply migrations (Retries left: {Retries})...", retries);
                    context.Database.Migrate();

                    var userManager = services.GetRequiredService<UserManager<AppUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var config = services.GetRequiredService<IConfiguration>();

                    logger.LogInformation("Seeding database...");
                    await DbSeeder.SeedAsync(userManager, roleManager, config);
                    logger.LogInformation("Database migration and seeding completed successfully.");
                    break;
                }
                catch (Exception ex)
                {
                    retries--;
                    logger.LogWarning(ex, "An error occurred during database migration or seeding. Retrying in 5 seconds...");
                    if (retries == 0)
                    {
                        logger.LogError(ex, "Could not apply migrations or seed database after multiple attempts.");
                    }
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

app.Run();
