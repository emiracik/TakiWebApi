using TakiWebApi.Data;
using TakiWebApi.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JWT SecretKey not found in configuration");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// Test database connection at startup
var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("SqlServerConnection string is not configured.");
}

try
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✅ Database connection successful!");
    Console.WriteLine($"📊 Connected to: {connection.Database} on {connection.DataSource}");
}
catch (Exception ex)
{
    Console.WriteLine("❌ Database connection failed!");
    Console.WriteLine($"🔗 Connection String: {connectionString}");
    Console.WriteLine($"💥 Error: {ex.Message}");
    throw;
}

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IFAQRepository, FAQRepository>();
builder.Services.AddScoped<IUserCreditCardRepository, UserCreditCardRepository>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IDriverRatingRepository, DriverRatingRepository>();
builder.Services.AddScoped<IUserNotificationSettingRepository, UserNotificationSettingRepository>();

builder.Services.AddScoped<IUserRatingRepository, UserRatingRepository>();
builder.Services.AddScoped<ITripRatingRepository, TripRatingRepository>();

// Register Wallet repository
builder.Services.AddScoped<IWalletRepository, WalletRepository>();

// Register Driver Earnings repository — sürücü hakedişi (komisyon modeli
// aktifken tam ücretten düşük olabilir) gerçek kayıtları burada tutulur.
builder.Services.AddScoped<IDriverEarningRepository, DriverEarningRepository>();

// Register RefreshToken repository
builder.Services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();

// Register Matching repository - NEW MATCHING SYSTEM
builder.Services.AddScoped<IMatchingRepository, MatchingRepository>();

// Bunlar controller'ları vardı ama DI'a hiç kayıtlı değillerdi — her
// endpoint'leri (BankInfo, DriverDocuments, Feedbacks, Invoices, Messages,
// SharedRides) constructor injection çözülemediği için 500 dönüyordu.
builder.Services.AddScoped<IBankInfoRepository, BankInfoRepository>();
builder.Services.AddScoped<IDriverDocumentRepository, DriverDocumentRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<ISharedRideRepository, SharedRideRepository>();

// Register JWT service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Password service
builder.Services.AddScoped<IPasswordService, PasswordService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Database health check endpoint
app.MapGet("/health/database", async (IConfiguration config) =>
{
    try
    {
        var connectionString = config.GetConnectionString("SqlServerConnection");
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        // Test if Users table exists
        const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
        using var command = new SqlCommand(sql, connection);
        var tableExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        
        if (tableExists)
        {
            // Get user count
            const string countSql = "SELECT COUNT(*) FROM Users";
            using var countCommand = new SqlCommand(countSql, connection);
            var userCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            
            return Results.Ok(new { 
                Status = "Healthy", 
                Database = connection.Database,
                Server = connection.DataSource,
                UsersTableExists = true,
                UserCount = userCount,
                Timestamp = DateTime.UtcNow
            });
        }
        else
        {
            return Results.Ok(new { 
                Status = "Connected but Users table missing", 
                Database = connection.Database,
                Server = connection.DataSource,
                UsersTableExists = false,
                Timestamp = DateTime.UtcNow
            });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
})
.WithName("DatabaseHealthCheck");

// Connection strings info endpoint
app.MapGet("/health/connections", (IConfiguration config) =>
{
    var connections = new
    {
        DefaultConnection = config.GetConnectionString("DefaultConnection"),
        SqlServerConnection = config.GetConnectionString("SqlServerConnection"),
        CurrentlyUsing = "SqlServerConnection"
    };
    return Results.Ok(connections);
})
.WithName("ConnectionStringsInfo");


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
