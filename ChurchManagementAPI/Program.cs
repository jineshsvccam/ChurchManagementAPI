using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchContracts.Interfaces.Repositories;
using ChurchContracts.Interfaces.Services;
using ChurchData;
using ChurchData.Mappings;
using ChurchManagementAPI.Controllers.Middleware;
using ChurchRepositories;
using ChurchRepositories.Queries;
using ChurchServices;
using ChurchServices.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
if (!Directory.Exists(logDir))
    Directory.CreateDirectory(logDir);

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
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.UseNetTopologySuite()
    )
);

// Add Identity services
builder.Services.AddIdentity<User, Role>(options =>
{
    options.User.RequireUniqueEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Explicitly register IRoleStore<Role>
builder.Services.AddScoped<IRoleStore<Role>, RoleStore<Role, ApplicationDbContext, Guid>>();
builder.Services.AddScoped<RoleManager<Role>>();
builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<SignInManager<User>>();
builder.Services.AddScoped<AuthService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins(
                "http://localhost:5173",  // Local development
                "https://salmon-meadow-05386b900.6.azurestaticapps.net",
                "http://localhost:5001",
                "https://master.d6uhfax3swdgj.amplifyapp.com",
                "https://finchurch.com",
                "https://www.finchurch.com",
                "http://finchurch.com",
                "http://www.finchurch.com"

            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Only needed for cookies/auth headers
});


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
builder.Services.AddAutoMapper(typeof(MappingProfile));



builder.Services.AddScoped<IUserAuthenticatorRepository, UserAuthenticatorRepository>();
builder.Services.AddScoped<IUser2FARecoveryCodeRepository, User2FARecoveryCodeRepository>();
builder.Services.AddScoped<IUser2FASessionRepository, User2FASessionRepository>();
builder.Services.AddScoped<ITwoFactorAuthenticationService, TwoFactorAuthenticationService>();

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
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFinancialYearRepository, FinancialYearRepository>();
builder.Services.AddScoped<IFinancialYearService, FinancialYearService>();
builder.Services.AddScoped<LogsHelper>();
builder.Services.AddScoped<IContributionSettingsRepository, ContributionSettingsRepository>();
builder.Services.AddScoped<IContributionSettingsService, ContributionSettingsService>();
builder.Services.AddScoped<IFamilyDueRepository, FamilyDueRepository>();
builder.Services.AddScoped<IFamilyDueService, FamilyDueService>();
builder.Services.AddScoped<IFamilyContributionRepository, FamilyContributionRepository>();
builder.Services.AddScoped<IFamilyContributionService, FamilyContributionService>();
builder.Services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();

builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ILedgerService, LedgerService>();
builder.Services.AddScoped<IBankConsolidatedStatementRepository, BankConsolidatedStatementRepository>();
builder.Services.AddScoped<IBankConsolidatedStatementService, BankConsolidatedStatementService>();
builder.Services.AddScoped<ITrialBalancetRepository, TrialBalanceRepository>();
builder.Services.AddScoped<ITrialBalanceService, TrialBalanceService>();
builder.Services.AddScoped<ICashBookRepository, CashBookRepository>();
builder.Services.AddScoped<ICashBookService, CashBookService>();
builder.Services.AddScoped<INoticeBoardRepository, NoticeBoardRepository>();
builder.Services.AddScoped<INoticeBoardService, NoticeBoardService>();
builder.Services.AddScoped<IAllTransactionsRepository, AllTransacionsRepository>();
builder.Services.AddScoped<IAllTransactionsService, AllTransactionsService>();
builder.Services.AddScoped<IAramanaReportRepository, AramanaReportRepository>();
builder.Services.AddScoped<IAramanaReportService, AramanaReportService>();
builder.Services.AddScoped<IFamilyReportRepository, FamilyReportRepository>();
builder.Services.AddScoped<IFamilyReportService, FamilyReportService>();
builder.Services.AddScoped<IKudishikaReportRepository, KudishikaReportRepository>();
builder.Services.AddScoped<IKudishikaReportService, KudishikaReportService>();

builder.Services.AddScoped<PivotReportService>(); // Register the concrete type first

builder.Services.AddScoped<IPivotReportService>(sp => sp.GetRequiredService<PivotReportService>());
builder.Services.AddScoped<ISingleHeadFiscalReportService>(sp => sp.GetRequiredService<PivotReportService>());
builder.Services.AddScoped<IMonthlyFiscalReportService>(sp => sp.GetRequiredService<PivotReportService>());


builder.Services.AddScoped<IPivotReportRepository, PivotReportRepository>();
builder.Services.AddScoped<ISingleHeadFiscalReportRepository, PivotReportRepository>();
builder.Services.AddScoped<IMonthlyFiscalReportRepository, PivotReportRepository>();

builder.Services.AddScoped<IPublicRepository, PublicRepository>();
builder.Services.AddScoped<IPublicService, PublicService>();
builder.Services.AddTransient<AESEncryptionHelper>();
builder.Services.AddHttpClient<IWhatsAppMessageSender, WhatsAppMessageSender>();
builder.Services.AddScoped<IWhatsAppBotService,ChurchServices.WhatsAppBot.WhatsAppBotService>();
builder.Services.AddScoped<IFamilyFileService, FamilyFileService>();
builder.Services.AddScoped<IFamilyFileRepository, FamilyFileRepository>();
builder.Services.AddScoped<IFileStorageService, S3StorageService>();
builder.Services.AddScoped<IFamilyQueryRepository, FamilyQueryRepository>();


builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserStateService, InMemoryUserStateService>();

//builder.Services.AddStackExchangeRedisCache(...);
//builder.Services.AddScoped<IUserStateService, RedisUserStateService>();


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
    // Only override OnForbidden to return a custom 403 message when the user is authenticated
    options.Events = new JwtBearerEvents
    {
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            // You can customize the response message here.
            return context.Response.WriteAsync("{\"error\": \"Access forbidden: You do not have the required permissions to access this resource.\"}");
        }
    };
});

// Add Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy("ManagementPolicy", policy =>
        policy.RequireRole("Admin", "Secretary", "Trustee","Priest"));
    options.AddPolicy("FamilyMemberPolicy", policy =>
        policy.RequireRole("Admin", "Secretary", "Trustee", "FamilyMember"));
});


// Configure Kestrel to listen to the port from AWS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "5000"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Add Endpoints API Explorer
builder.Services.AddMemoryCache(); // Enable MemoryCache

// Swagger configuration
// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChurchManagementAPI", Version = "v1" });

    // Add support for annotations (optional, requires Swashbuckle.AspNetCore.Annotations)
    c.EnableAnnotations(); // This line requires the package and a using directive (see below)

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
            new string[] { }
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
            new string[] { }
        }
    };
    c.AddSecurityRequirement(securityRequirement);

    //// Optional: Custom filter to exclude abstract controllers
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            return !controllerActionDescriptor.ControllerTypeInfo.IsAbstract;
        }
        return true;
    });

    // Optional: Log Swagger generation issues for debugging
    //c.DocumentFilter<SwaggerDebugFilter>(); // Define this filter as shown previously if needed
});
// Add logging with console output
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug); // Set to Debug for more detailed logs
});

// Configure forwarded headers for reverse proxy (e.g., AWS ALB)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear(); // Clears the known networks so it trusts all
    options.KnownProxies.Clear();  // Clears the known proxies so it trusts all
});

var app = builder.Build();

// Use forwarded headers before authentication
app.UseForwardedHeaders();


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

// Use CORS before other middlewares
app.UseCors("AllowSpecificOrigin");

// ✅ Ensure authentication runs before middleware
// ✅ Enable HTTPS redirection only in dev/local
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
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
// Add health check endpoint
app.MapGet("/health", () => Results.Ok("Healthy"));

// Run the application with logging
try
{
    Log.Information("Starting up FinChurch API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}
//app.Run();
