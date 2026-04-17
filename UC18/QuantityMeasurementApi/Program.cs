using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuantityMeasurementApi.Middleware;
using QuantityMeasurementbusinessLayer;
using QuantityMeasurementbusinessLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Interfaces;
using QuantityMeasurementRepositoryLayer.Repositories;
using QuantityMeasurementRepositoryLayer.Repositories.EfCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. SQLite (EF Core) ──────────────────────────────────────────
// When no connection string is configured (e.g. during tests), use an in-memory SQLite DB.
// We keep one connection open for the app's lifetime so the in-memory DB is not destroyed
// between requests (in-memory SQLite drops the DB when all connections close).
var sqliteCs = builder.Configuration.GetConnectionString("QuantityMeasurementDb")
if (string.IsNullOrEmpty(sqliteCs))
{
    sqliteCs = "Data Source=quantity-measurement.db";
}

builder.Services.AddDbContext<QuantityMeasurementDbContext>(options =>
    options.UseSqlite(sqliteCs));

// ── 2. Dependency Injection ──────────────────────────────────────
builder.Services.AddScoped<IQuantityMeasurementRepository, EfQuantityMeasurementRepository>();
builder.Services.AddScoped<IQuantityMeasurementService,    QuantityMeasurementServiceImpl>();
builder.Services.AddScoped<IAuthRepository,                AuthRepository>();
builder.Services.AddScoped<IAuthService,                   AuthServiceImpl>();

// ── 3. JWT Authentication ────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── 4. Controllers ───────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new
        {
            Timestamp = DateTime.UtcNow.ToString("o"),
            Status    = 400,
            Error     = "Validation Failed",
            Message   = string.Join("; ", errors.SelectMany(kv => kv.Value)),
            Path      = ctx.HttpContext.Request.Path.ToString()
        });
    };
});

// ── 5. Swagger with JWT Bearer UI ───────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Quantity Measurement API",
        Version     = "v1",
        Description = "UC18 — Quantity Measurement REST API with JWT Authentication & Role-based Admin"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: Bearer eyJhbGci..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ── 6. CORS ──────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var defaultOrigins = new[] {
    "http://localhost:3000", "http://localhost:3001",
    "http://127.0.0.1:5500", "http://localhost:5500",
    "http://127.0.0.1:5501", "http://localhost:5501",
    "http://127.0.0.1:8080", "http://localhost:8080",
    "http://127.0.0.1:8000", "http://localhost:8000",
    "http://127.0.0.1:4200", "http://localhost:4200"
};
var allOrigins = allowedOrigins.Concat(defaultOrigins).Distinct().ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ── Build app ────────────────────────────────────────────────────
var app = builder.Build();

// Auto-create / migrate SQLite DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuantityMeasurementDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quantity Measurement API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }