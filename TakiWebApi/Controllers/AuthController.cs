using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Services;
using TakiWebApi.Data;
using TakiWebApi.Models;
using System.Security.Claims;

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

    public AuthController(IJwtService jwtService, IUserRepository userRepository, IDriverRepository driverRepository, IPasswordService passwordService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _passwordService = passwordService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 Login attempt for phone: {PhoneNumber}", request.PhoneNumber);
            
            // Retrieve user from database
            var user = await _userRepository.GetUserByPhoneNumberAsync(request.PhoneNumber);
            if (user == null)
            {
                _logger.LogWarning("❌ User not found for phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }
            _logger.LogWarning("1");
            // Check if user is active
            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("❌ Inactive or deleted user for phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Account is not active" });
            }

            // Verify password - ONLY use database password hash
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("❌ No password set for user phone: {PhoneNumber}", request.PhoneNumber);
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }

            bool passwordValid = _passwordService.VerifyPassword(request.Password, user.PasswordHash);
            _logger.LogWarning(passwordValid.ToString());
    
            if (passwordValid)
            {
                _logger.LogInformation("✅ Login successful for phone: {PhoneNumber}", request.PhoneNumber);
                
                var additionalClaims = new[]
                {
                    new Claim("phone_number", user.PhoneNumber),
                    new Claim("role", "user"),
                    new Claim("email", user.Email ?? ""),
                    new Claim("full_name", user.FullName ?? "")
                };
                var token = _jwtService.GenerateToken(user.UserID.ToString(), "user", additionalClaims);

                return Ok(new
                {
                    Success = true,
                    Token = token,
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
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("driver-login")]
    [AllowAnonymous]
    public async Task<IActionResult> DriverLogin([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 Driver login attempt for phone: {PhoneNumber}", request.PhoneNumber);
            
            // Retrieve driver from database using DriverRepository
            var driver = await _driverRepository.GetDriverByPhoneNumberAsync(request.PhoneNumber);
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

            // Verify password
            bool passwordValid = false;
            
            if (!string.IsNullOrEmpty(driver.PasswordHash))
            {
                // Use stored password hash
                passwordValid = _passwordService.VerifyPassword(request.Password, driver.PasswordHash);
            }
            else
            {
                // Fallback for demo purposes if no password hash is stored
                // Accept: driver123, password123, their phone number, or 123456
                var demoPasswords = new[] { "driver123", "password123", driver.PhoneNumber, "123456" };
                passwordValid = demoPasswords.Any(p => p == request.Password);
                
                _logger.LogInformation("🔧 Using demo password validation for driver phone: {PhoneNumber}", request.PhoneNumber);
            }

            if (passwordValid)
            {
                _logger.LogInformation("✅ Driver login successful for phone: {PhoneNumber}", request.PhoneNumber);
                
                var additionalClaims = new[]
                {
                    new Claim("phone_number", driver.PhoneNumber),
                    new Claim("role", "driver"),
                    new Claim("email", driver.Email ?? ""),
                    new Claim("full_name", driver.FullName ?? ""),
                    new Claim("vehicle_plate", driver.VehiclePlate ?? "")
                };

                var token = _jwtService.GenerateToken(driver.DriverID.ToString(), "driver", additionalClaims);

                return Ok(new
                {
                    Success = true,
                    Token = token,
                    UserType = "driver",
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
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetUserByPhoneNumberAsync(request.PhoneNumber);
            if (existingUser != null)
            {
                return BadRequest(new { Success = false, Message = "User with this phone number already exists" });
            }

            // Create new user
            var newUser = new User
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
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
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
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
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("driver-register")]
    [AllowAnonymous]
    public async Task<IActionResult> DriverRegister([FromBody] DriverRegisterRequest request)
    {
        try
        {
            // Check if driver already exists
            var existingDriver = await _driverRepository.GetDriverByPhoneNumberAsync(request.PhoneNumber);
            if (existingDriver != null)
            {
                return BadRequest(new { Success = false, Message = "Driver with this phone number already exists" });
            }

            // Create new driver
            var newDriver = new Driver
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                VehiclePlate = request.VehiclePlate,
                VehicleModel = request.VehicleModel,
                VehicleColor = request.VehicleColor,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var driverId = await _driverRepository.AddDriverAsync(newDriver);

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
                DriverId = driverId.DriverID,
                UserType = "driver",
                Message = "Driver registration successful"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = ex.Message });
        }
    }
}

public class LoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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
