using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Implementations;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Implementations;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var aspNetCoreUrls = builder.Configuration["ASPNETCORE_URLS"];
var hasHttpsUrl = !string.IsNullOrWhiteSpace(aspNetCoreUrls)
	&& aspNetCoreUrls
		.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
		.Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
builder.Services.AddScoped<IInterestTypeRepository, InterestTypeRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI();
	app.MapGet("/", () => Results.Redirect("/swagger"));
}

if (hasHttpsUrl)
{
	app.UseHttpsRedirection();
}
app.MapControllers();
app.Run();
