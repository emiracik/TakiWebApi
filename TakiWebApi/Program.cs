using TakiWebApi.Data;
using TakiWebApi.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Yönetici aracı: sürücüye/kullanıcıya panelden şifre atamak için BCrypt
// hash'i üretir. Kullanım: dotnet TakiWebApi.dll hash-password <şifre>
if (args.Length == 2 && args[0] == "hash-password")
{
    Console.WriteLine(new PasswordService().HashPassword(args[1]));
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    // Varsayılan: her uç token ister. Açık uçlar [AllowAnonymous] ile işaretlenir.
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
    // URL'deki userId/passengerId/driverId token sahibine ait olmalı.
    options.Filters.Add<TakiWebApi.Security.ResourceOwnerFilter>();
});

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
    // Uygulama yine de ayağa kalkar; durum /health/database ile izlenir.
    Console.WriteLine("❌ Database connection failed!");
    Console.WriteLine($"💥 Error: {ex.Message}");
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
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Anlık bildirimler (FCM). Firebase ayarı yoksa sessizce kapalı kalır.
builder.Services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();

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

// Yeni ekranlar (Migrations/003): promosyon kodu, ileri tarihli yolculuk.
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddSingleton<IRouteService, RouteService>();
builder.Services.AddScoped<IScheduledRideRepository, ScheduledRideRepository>();
builder.Services.AddHostedService<ScheduledRideReminderService>();

// Register JWT service
builder.Services.AddScoped<IJwtService, JwtService>();

// Register Password service
builder.Services.AddScoped<IPasswordService, PasswordService>();

// Giriş/kayıt uçlarına deneme sınırı: IP başına dakikada 30 istek (mobil operatörlerde çok kullanıcı aynı IP'den çıkar; token yenileme sınırsız) (şifre
// deneme saldırılarına karşı). Aşılırsa 429 döner.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor",
            _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"success\":false,\"message\":\"Çok fazla deneme. Lütfen bir dakika sonra tekrar deneyin.\"}", ct);
    };
});

var app = builder.Build();

// Yakalanmayan hatalar istemciye ayrıntı (SQL mesajı, tablo adı vb.)
// sızdırmadan loglanır; istemci sadece genel mesaj ve traceId görür.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("UnhandledException");
    if (feature != null)
        logger.LogError(feature.Error, "Unhandled exception on {Path}", context.Request.Path);

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new
    {
        Success = false,
        Message = "Beklenmeyen bir hata oluştu.",
        TraceId = context.TraceIdentifier
    });
}));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add authentication and authorization middleware
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Sağlık kontrolü — yalnızca durum döner; sunucu/DB adı veya sayılar
// dışarıya verilmez.
app.MapGet("/health/database", async (IConfiguration config) =>
{
    try
    {
        using var connection = new SqlConnection(config.GetConnectionString("SqlServerConnection"));
        await connection.OpenAsync();

        // Migrations/ altındaki betikler çalıştırılmadıysa giriş çalışmaz.
        using var check = new SqlCommand(
            "SELECT CASE WHEN OBJECT_ID(N'dbo.RefreshTokens', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.DeviceTokens', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.ScheduledRides', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Trips', N'TipAmount') IS NOT NULL AND COL_LENGTH(N'dbo.Drivers', N'ApprovalStatus') IS NOT NULL AND COL_LENGTH(N'dbo.Trips', N'ArrivedAt') IS NOT NULL THEN 1 END",
            connection);
        if (await check.ExecuteScalarAsync() is null or DBNull)
        {
            return Results.Json(new
            {
                Status = "Unhealthy",
                Reason = "Migration eksik: Migrations/ altındaki betikleri (001-005) çalıştırın",
                Timestamp = DateTime.UtcNow
            }, statusCode: 503);
        }
        return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
    catch
    {
        return Results.Json(new { Status = "Unhealthy", Timestamp = DateTime.UtcNow }, statusCode: 503);
    }
})
.WithName("DatabaseHealthCheck");

app.Run();
