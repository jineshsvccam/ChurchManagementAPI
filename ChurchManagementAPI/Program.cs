using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using ChurchManagementAPI.Middleware;
using ChurchRepositories;
using ChurchServices;
using ChurchServices.ChurchServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Read Serilog Enabled flag from appsettings.json
var isLoggingEnabled = builder.Configuration.GetValue<bool>("Serilog:Enabled");

if (isLoggingEnabled)
{
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger();

    builder.Host.UseSerilog();
}

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<User, Role>(options =>
{
    options.User.RequireUniqueEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Explicitly register IRoleStore<Role>
builder.Services.AddScoped<IRoleStore<Role>, RoleStore<Role, ApplicationDbContext, int>>();
builder.Services.AddScoped<RoleManager<Role>>();
builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();
builder.Services.AddScoped<AuthService>();

// JSON serialization settings
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Register Services and Repositories
builder.Services.AddScoped<IParishService, ParishService>();
builder.Services.AddScoped<IParishRepository, ParishRepository>();
builder.Services.AddScoped<IDioceseRepository, DioceseRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<IDioceseService, DioceseService>();
builder.Services.AddScoped<IDistrictService, DistrictService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IFamilyRepository, FamilyRepository>();
builder.Services.AddScoped<ITransactionHeadRepository, TransactionHeadRepository>();
builder.Services.AddScoped<ITransactionHeadService, TransactionHeadService>();
builder.Services.AddScoped<IBankRepository, BankRepository>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IFamilyMemberRepository, FamilyMemberRepository>();
builder.Services.AddScoped<IFamilyMemberService, FamilyMemberService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IBankConsolidatedStatementRepository, BankConsolidatedStatementRepository>();
builder.Services.AddScoped<IBankConsolidatedStatementService, BankConsolidatedStatementService>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFinancialYearRepository, FinancialYearRepository>();
builder.Services.AddScoped<IFinancialYearService, FinancialYearService>();

// Register configuration settings
builder.Services.Configure<LoggingSettings>(builder.Configuration.GetSection("Logging"));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing in appsettings.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        NameClaimType = ClaimTypes.NameIdentifier,  //  Ensure NameClaimType is set
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Add Endpoints API Explorer
builder.Services.AddMemoryCache(); // Enable MemoryCache

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChurchManagementAPI", Version = "v1" });

    // Add the JWT authorization header to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityDefinition("User-ID", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "User-ID",
        Type = SecuritySchemeType.ApiKey,
        Description = "User ID for logging"
    });

    var securityRequirement = new OpenApiSecurityRequirement
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
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "User-ID"
                }
            },
            new string[] {}
        }
    };
    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChurchManagementAPI v1"));

    // Disable authentication in development mode
    app.Use(async (context, next) =>
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"), // Default User ID
            new Claim(ClaimTypes.Name, "DevUser"),
            new Claim(ClaimTypes.Role, "Admin") // Set default role(s)
        }, "mock"));

        context.User = user;
        await next();
    });
}

// ✅ Ensure authentication runs before middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Modify middleware to extract NameIdentifier manually
app.Use(async (context, next) =>
{
    var user = context.User;
    var userName = user.Identity?.Name
                   ?? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

    Log.Information("Middleware Logging - User: {User}", userName);

    await next();
});

// ✅ Register Middleware after Authentication
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Enable Serilog request logging if enabled
if (isLoggingEnabled)
{
    app.UseSerilogRequestLogging();
}

app.MapControllers();
app.Run();
