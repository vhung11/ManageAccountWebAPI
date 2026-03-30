using ManageAccountWebAPI.Infrastructure.Context;
using ManageAccountWebAPI.Infrastructure.Implementations;
using ManageAccountWebAPI.Infrastructure.Repositories;
using ManageAccountWebAPI.Infrastructure.Settings;
using ManageAccountWebAPI.Services.Implementations;
using ManageAccountWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NLog;
using NLog.Web;
using System.Text;

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
	GlobalDiagnosticsContext.Set("dbConnection", builder.Configuration.GetConnectionString("OracleConnection"));
	bootstrapLogger.Info("NLog initialized with mode '{LoggingMode}'.", configuredLoggingMode);

	builder.Services.AddControllers()
		.AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
		});
	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen(options =>
	{
		options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
		{
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = "Nhập JWT token vào đây. Ví dụ: eyJhbGciOi..."
		});

		options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
		{
			[new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
		});
	});

	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

	builder.Services.AddScoped<IAccountRepository, AccountRepository>();
	builder.Services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
	builder.Services.AddScoped<IInterestTypeRepository, InterestTypeRepository>();
	builder.Services.AddScoped<IAuthRepository, AuthRepository>();
	builder.Services.AddScoped<IAccountService, AccountService>();
	builder.Services.AddScoped<ITransactionService, TransactionService>();
	builder.Services.AddScoped<IAuthService, AuthService>();
	// TokenService has no scoped dependencies → safe to register as Singleton.
	builder.Services.AddSingleton<ITokenService, TokenService>();

	// --- JWT Options pattern registration -------------------------------------------
	// Binds appsettings.json "JwtSettings" section to the strongly-typed JwtSettings
	// class. Both TokenService (via IOptions<JwtSettings>) and AddJwtBearer below
	// consume values from this single binding — no string-key lookups anywhere.
	builder.Services.Configure<JwtSettings>(
		builder.Configuration.GetSection(JwtSettings.SectionName));

	var jwtSettings = builder.Configuration
		.GetSection(JwtSettings.SectionName)
		.Get<JwtSettings>()
		?? throw new InvalidOperationException(
			$"Configuration section '{JwtSettings.SectionName}' is missing or empty.");

	if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
		throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

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
				ValidIssuer              = jwtSettings.Issuer,
				ValidAudience            = jwtSettings.Audience,
				IssuerSigningKey         = new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
				ClockSkew                = TimeSpan.Zero
			};
		});

	builder.Services.AddAuthorization();

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

	if (app.Environment.IsDevelopment())
	{
		app.UseSwagger();
		app.UseSwaggerUI();
	}

	app.UseCors("AllowAll");
	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseAuthorization();
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
