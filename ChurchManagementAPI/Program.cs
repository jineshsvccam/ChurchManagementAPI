using System.Text.Json;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchRepositories;
using ChurchServices;
using ChurchServices.ChurchServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
