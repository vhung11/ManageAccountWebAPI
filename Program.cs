using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Implementations;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Services.Implementations;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

var bootstrapLogger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

try
{
	var builder = WebApplication.CreateBuilder(args);

	builder.Logging.ClearProviders();
	builder.Host.UseNLog();

	var configuredLoggingMode = builder.Configuration["Logging:LoggingMode:ModeFlag"]
		?? builder.Configuration["LoggingMode:ModeFlag"]
		?? "file";

	configuredLoggingMode = configuredLoggingMode.Trim().ToLowerInvariant();
	if (configuredLoggingMode is not ("console" or "database" or "file"))
	{
		configuredLoggingMode = "file";
	}

	GlobalDiagnosticsContext.Set("loggingMode", configuredLoggingMode);
	GlobalDiagnosticsContext.Set("appName", builder.Environment.ApplicationName);
	bootstrapLogger.Info("NLog initialized with mode '{LoggingMode}'.", configuredLoggingMode);

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
	builder.Services.AddScoped<ITransactionService, TransactionService>();

	var app = builder.Build();

	if (app.Environment.IsDevelopment())
	{
		app.MapOpenApi();
		app.UseSwagger();
		app.UseSwaggerUI();
	}

	app.UseHttpsRedirection();
	app.MapControllers();
	app.Run();
}
catch (Exception ex)
{
	bootstrapLogger.Error(ex, "Application stopped because of an exception.");
	throw;
}
finally
{
	LogManager.Shutdown();
}
