using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Services;
using TakiWebApi.Security;
using TakiWebApi.Data;
using TakiWebApi.Models;
using System.Security.Claims;
using Google.Apis.Auth;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AuthController> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IJwtService jwtService, IUserRepository userRepository, IDriverRepository driverRepository, IPasswordService passwordService, ILogger<AuthController> logger, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _passwordService = passwordService;
        _logger = logger;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 Login attempt for phone: {PhoneNumber}", request.PhoneNumber);
            var user = await FindUserByPhoneAsync(request.PhoneNumber);
            if (user == null)
            {
                _logger.LogWarning("❌ User not found for phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }
            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("❌ Inactive or deleted user for phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Account is not active" });
            }
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("❌ No password set for user phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }
            bool passwordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
            if (passwordValid)
            {
                _logger.LogInformation("✅ Login successful for phone: {PhoneNumber}", request.PhoneNumber);
                if (_passwordService.NeedsRehash(user.PasswordHash))
                {
                    await _userRepository.UpdateUserPasswordAsync(user.UserID, _passwordService.HashPassword(request.Password));
                }
                var token = _jwtService.GenerateToken(user.UserID.ToString(), "user", UserClaims(user));
                var refreshToken = await _refreshTokenRepository.IssueAsync(user.UserID, "user");
                return Ok(new
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    UserType = "user",
                    UserId = user.UserID,
                    PhoneNumber = user.PhoneNumber,
                    FullName = user.FullName,
                    Email = user.Email,
                    Message = "Login successful"
                });
            }
            _logger.LogWarning("❌ Invalid password for phone: {PhoneNumber}", request.PhoneNumber);
            return Unauthorized(new { Success = false, Message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error during login for phone: {PhoneNumber}", request.PhoneNumber);
            throw;
        }
    }
    [HttpPost("google-login")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var webClientId = _configuration["GoogleAuth:WebClientId"];
            if (string.IsNullOrEmpty(webClientId))
            {
                _logger.LogError("❌ GoogleAuth:WebClientId appsettings içinde tanımlı değil");
                return StatusCode(500, new { Success = false, Message = "Google login yapılandırılmamış" });
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { webClientId } });
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning("❌ Geçersiz Google idToken: {Message}", ex.Message);
                return Unauthorized(new { Success = false, Message = "Geçersiz Google token" });
            }

            if (!payload.EmailVerified)
            {
                return Unauthorized(new { Success = false, Message = "Google e-postası doğrulanmamış" });
            }

            var user = await _userRepository.GetUserByEmailAsync(payload.Email);
            if (user == null)
            {
                var placeholderPhone = $"G{payload.Subject}";
                if (placeholderPhone.Length > 20) placeholderPhone = placeholderPhone.Substring(0, 20);

                var newUser = new User
                {
                    FullName = payload.Name ?? payload.Email,
                    PhoneNumber = placeholderPhone,
                    Email = payload.Email,
                    PasswordHash = null,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                var newUserId = await _userRepository.CreateUserAsync(newUser);
                user = await _userRepository.GetUserByIdAsync(newUserId);
                _logger.LogInformation("✅ Google ile yeni kullanıcı oluşturuldu: {Email}", payload.Email);
            }

            if (user == null)
            {
                return StatusCode(500, new { Success = false, Message = "Kullanıcı oluşturulamadı" });
            }

            if (!user.IsActive || user.IsDeleted)
            {
                return Unauthorized(new { Success = false, Message = "Hesap aktif değil" });
            }

            var token = _jwtService.GenerateToken(user.UserID.ToString(), "user", UserClaims(user));
            var refreshToken = await _refreshTokenRepository.IssueAsync(user.UserID, "user");

            return Ok(new
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                UserType = "user",
                UserId = user.UserID,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Email = user.Email,
                Message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Google login sırasında hata");
            throw;
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        // Yolcu ve sürücü token'ları aynı uçtan yenilenir; kullanılan refresh
        // token iptal edilip yenisi verilir (rotasyon).
        var rotated = await _refreshTokenRepository.RotateAsync(refreshToken);
        if (rotated == null)
            return Unauthorized("Refresh token geçersiz veya süresi dolmuş.");

        var (record, newRefreshToken) = rotated.Value;
        string? newJwt = null;
        if (record.SubjectType == "user")
        {
            var user = await _userRepository.GetUserByIdAsync(record.SubjectId);
            if (user != null && !user.IsDeleted && user.IsActive)
                newJwt = _jwtService.GenerateToken(user.UserID.ToString(), "user", UserClaims(user));
        }
        else if (record.SubjectType == "driver")
        {
            var driver = await _driverRepository.GetDriverByIdAsync(record.SubjectId);
            if (driver != null && !driver.IsDeleted)
                newJwt = _jwtService.GenerateToken(driver.DriverID.ToString(), "driver", DriverClaims(driver));
        }

        if (newJwt == null)
        {
            await _refreshTokenRepository.RevokeAllAsync(record.SubjectId, record.SubjectType);
            return Unauthorized("Kullanıcı bulunamadı.");
        }
        return Ok(new { token = newJwt, refreshToken = newRefreshToken });
    }

    /// <summary>
    /// Çıkış: cihazdaki refresh token iptal edilir (gövde: düz JSON string).
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _refreshTokenRepository.RevokeAsync(refreshToken);
        return Ok(new { Success = true });
    }

    // Numara veritabanında hangi biçimde kayıtlıysa bulunur (+90…, 0…, 10 hane).
    private async Task<User?> FindUserByPhoneAsync(string phoneNumber)
    {
        foreach (var candidate in PhoneNumbers.Candidates(phoneNumber))
        {
            var user = await _userRepository.GetUserByPhoneNumberAsync(candidate);
            if (user != null) return user;
        }
        return null;
    }

    private async Task<Driver?> FindDriverByPhoneAsync(string phoneNumber)
    {
        foreach (var candidate in PhoneNumbers.Candidates(phoneNumber))
        {
            var driver = await _driverRepository.GetDriverByPhoneNumberAsync(candidate);
            if (driver != null) return driver;
        }
        return null;
    }

    private static Claim[] UserClaims(User user) => new[]
    {
        new Claim("phone_number", user.PhoneNumber),
        new Claim("role", "user"),
        new Claim("email", user.Email ?? ""),
        new Claim("full_name", user.FullName ?? ""),
    };

    private static Claim[] DriverClaims(Driver driver) => new[]
    {
        new Claim("phone_number", driver.PhoneNumber),
        new Claim("role", "driver"),
        new Claim("email", driver.Email ?? ""),
        new Claim("full_name", driver.FullName ?? ""),
        new Claim("vehicle_plate", driver.VehiclePlate ?? "")
    };

    [HttpPost("driver-login")]
    [AllowAnonymous]
    public async Task<IActionResult> DriverLogin([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 Driver login attempt for phone: {PhoneNumber}", request.PhoneNumber);
            
            // Retrieve driver from database using DriverRepository
            var driver = await FindDriverByPhoneAsync(request.PhoneNumber);
            _logger.LogInformation("📋 Driver found: {Found}, PasswordHash exists: {HasPassword}", 
                driver != null, 
                driver?.PasswordHash != null);
            
            if (driver == null)
            {
                _logger.LogWarning("❌ Driver not found for phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Invalid driver credentials" });
            }

            // Check if driver is deleted
            if (driver.IsDeleted)
            {
                _logger.LogWarning("❌ Deleted driver for phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Driver account is not active" });
            }

            // Şifresi tanımlı olmayan sürücü giriş yapamaz; şifre yönetici
            // tarafından atanmalı (bkz. "dotnet TakiWebApi.dll hash-password").
            if (string.IsNullOrEmpty(driver.PasswordHash))
            {
                _logger.LogWarning("❌ No password set for driver phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Invalid driver credentials" });
            }

            bool passwordValid = _passwordService.VerifyPassword(request.Password, driver.PasswordHash);

            if (passwordValid)
            {
                _logger.LogInformation("✅ Driver login successful for phone: {PhoneNumber}", request.PhoneNumber);
                if (_passwordService.NeedsRehash(driver.PasswordHash))
                {
                    await _driverRepository.UpdateDriverPasswordAsync(driver.DriverID, _passwordService.HashPassword(request.Password));
                }

                var token = _jwtService.GenerateToken(driver.DriverID.ToString(), "driver", DriverClaims(driver));
                var refreshToken = await _refreshTokenRepository.IssueAsync(driver.DriverID, "driver");
                var (approvalStatus, approvalNote) = await _driverRepository.GetApprovalAsync(driver.DriverID);

                return Ok(new
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    UserType = "driver",
                    ApprovalStatus = approvalStatus,
                    ApprovalNote = approvalNote,
                    DriverId = driver.DriverID,
                    PhoneNumber = driver.PhoneNumber,
                    FullName = driver.FullName,
                    Email = driver.Email,
                    VehiclePlate = driver.VehiclePlate,
                    VehicleModel = driver.VehicleModel,
                    VehicleColor = driver.VehicleColor,
                    Message = "Driver login successful"
                });
            }

            _logger.LogWarning("❌ Invalid password for driver phone: {PhoneNumber}", request.PhoneNumber);
            return Unauthorized(new { Success = false, Message = "Invalid driver credentials" });
        }
        catch
        {
            throw;
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await FindUserByPhoneAsync(request.PhoneNumber);
            if (existingUser != null)
            {
                return BadRequest(new { Success = false, Message = "User with this phone number already exists" });
            }

            // Create new user
            var newUser = new User
            {
                FullName = request.FullName,
                PhoneNumber = PhoneNumbers.Normalize(request.PhoneNumber),
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var userId = await _userRepository.CreateUserAsync(newUser);

            var additionalClaims = new[]
            {
                new Claim("phone_number", request.PhoneNumber),
                new Claim("role", "user"),
                new Claim("email", request.Email ?? "")
            };

            var token = _jwtService.GenerateToken(userId.ToString(), "user", additionalClaims);

            return Ok(new
            {
                Success = true,
                Token = token,
                UserId = userId,
                UserType = "user",
                Message = "Registration successful"
            });
        }
        catch
        {
            throw;
        }
    }

    [HttpPost("validate")]
    [AllowAnonymous]
    public IActionResult ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var principal = _jwtService.ValidateToken(request.Token);

            if (principal != null)
            {
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userType = principal.FindFirst("user_type")?.Value;
                var phoneNumber = principal.FindFirst("phone_number")?.Value;

                return Ok(new
                {
                    Success = true,
                    Valid = true,
                    UserId = userId,
                    UserType = userType,
                    PhoneNumber = phoneNumber,
                    Message = "Token is valid"
                });
            }

            return Ok(new { Success = false, Valid = false, Message = "Invalid token" });
        }
        catch
        {
            throw;
        }
    }

    [HttpPost("driver-register")]
    [AllowAnonymous]
    public async Task<IActionResult> DriverRegister([FromBody] DriverRegisterRequest request)
    {
        try
        {
            // Check if driver already exists
            var existingDriver = await FindDriverByPhoneAsync(request.PhoneNumber);
            if (existingDriver != null)
            {
                return BadRequest(new { Success = false, Message = "Driver with this phone number already exists" });
            }

            // Create new driver
            var newDriver = new Driver
            {
                FullName = request.FullName,
                PhoneNumber = PhoneNumbers.Normalize(request.PhoneNumber),
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                VehiclePlate = request.VehiclePlate,
                VehicleModel = request.VehicleModel,
                VehicleColor = request.VehicleColor,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var driverId = await _driverRepository.AddDriverAsync(newDriver);

            // Yeni sürücü, belgeleri onaylanana kadar talep alamaz (Migrations/004).
            var approvalStatus = "Pending";
            try
            {
                await _driverRepository.SetApprovalAsync(driverId.DriverID, "Pending", null);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 207)
            {
                approvalStatus = "Approved"; // sütun henüz yok
            }
            var registerRefreshToken = await _refreshTokenRepository.IssueAsync(driverId.DriverID, "driver");

            var additionalClaims = new[]
            {
                new Claim("phone_number", request.PhoneNumber),
                new Claim("role", "driver"),
                new Claim("email", request.Email ?? ""),
                new Claim("vehicle_plate", request.VehiclePlate ?? "")
            };

            var token = _jwtService.GenerateToken(driverId.DriverID.ToString(), "driver", additionalClaims);

            return Ok(new
            {
                Success = true,
                Token = token,
                RefreshToken = registerRefreshToken,
                DriverId = driverId.DriverID,
                UserType = "driver",
                ApprovalStatus = approvalStatus,
                Message = "Driver registration successful"
            });
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Hesap silme (App Store / Google Play zorunluluğu, KVKK): token sahibinin
    /// kişisel verisi anonimleştirilir ve hesap kapatılır. Yolcu ve sürücü
    /// hesapları için çalışır; geri alınamaz.
    /// </summary>
    [HttpDelete("account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var callerId = User.CallerId();
        if (callerId == null)
        {
            return Unauthorized();
        }

        var userType = User.FindFirstValue("user_type");
        bool deleted = userType switch
        {
            "user" => await _userRepository.AnonymizeAndDeleteUserAsync(callerId.Value),
            "driver" => await _driverRepository.AnonymizeAndDeleteDriverAsync(callerId.Value),
            _ => false
        };

        if (!deleted)
        {
            return NotFound(new { Success = false, Message = "Hesap bulunamadı." });
        }

        await _refreshTokenRepository.RevokeAllAsync(callerId.Value, userType!);
        _logger.LogInformation("🗑️ Account deleted: {UserType} {Id}", userType, callerId.Value);
        return Ok(new { Success = true, Message = "Hesabınız silindi." });
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Şifre sadece token sahibinin kendi hesabı için değiştirilebilir.
            if (User.FindFirstValue("user_type") != "user" ||
                !int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId) ||
                currentUserId != request.UserId)
            {
                return Forbid();
            }

            // Get user by ID
            var user = await _userRepository.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Check if user has a password set
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest(new { message = "User has no password set. Please contact support." });
            }

            // Verify current password
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }

            // Hash new password
            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);

            // Update password
            var success = await _userRepository.UpdateUserPasswordAsync(request.UserId, newPasswordHash);
            
            if (!success)
            {
                return StatusCode(500, new { message = "Failed to update password." });
            }

            return Ok(new { message = "Password changed successfully." });
        }
        catch
        {
            throw;
        }
    }
}

public class LoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public class DriverRegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? VehiclePlate { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
}
