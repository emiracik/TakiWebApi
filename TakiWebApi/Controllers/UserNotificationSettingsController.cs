using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserNotificationSettingsController : ControllerBase
{
    private readonly IUserNotificationSettingRepository _userNotificationSettingRepository;

    public UserNotificationSettingsController(IUserNotificationSettingRepository userNotificationSettingRepository)
    {
        _userNotificationSettingRepository = userNotificationSettingRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserNotificationSetting>>> GetAllUserNotificationSettings()
    {
        var settings = await _userNotificationSettingRepository.GetAllUserNotificationSettingsAsync();
        return Ok(settings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserNotificationSetting>> GetUserNotificationSetting(int id)
    {
        var setting = await _userNotificationSettingRepository.GetUserNotificationSettingByIdAsync(id);
        
        if (setting == null)
        {
            return NotFound();
        }

        return Ok(setting);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<UserNotificationSetting>> GetUserNotificationSettingByUserId(int userId)
    {
        var setting = await _userNotificationSettingRepository.GetUserNotificationSettingByUserIdAsync(userId);
        
        if (setting == null)
        {
            return NotFound();
        }

        return Ok(setting);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<IEnumerable<UserNotificationSetting>>> GetUserNotificationSettingsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var settings = await _userNotificationSettingRepository.GetUserNotificationSettingsPaginatedAsync(pageNumber, pageSize);
        return Ok(settings);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalUserNotificationSettingsCount()
    {
        var count = await _userNotificationSettingRepository.GetTotalUserNotificationSettingsCountAsync();
        return Ok(count);
    }

    [HttpPost]
    public async Task<ActionResult<UserNotificationSetting>> CreateUserNotificationSetting([FromBody] UserNotificationSetting userNotificationSetting)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var settingId = await _userNotificationSettingRepository.CreateUserNotificationSettingAsync(userNotificationSetting);
            var createdSetting = await _userNotificationSettingRepository.GetUserNotificationSettingByIdAsync(settingId);
            
            return CreatedAtAction(nameof(GetUserNotificationSetting), new { id = settingId }, createdSetting);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserNotificationSetting(int id, [FromBody] UserNotificationSetting userNotificationSetting)
    {
        if (id != userNotificationSetting.SettingID)
        {
            return BadRequest("ID mismatch between route and body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingSetting = await _userNotificationSettingRepository.GetUserNotificationSettingByIdAsync(id);
            if (existingSetting == null)
            {
                return NotFound();
            }

            var success = await _userNotificationSettingRepository.UpdateUserNotificationSettingAsync(userNotificationSetting);
            
            if (!success)
            {
                return StatusCode(500, "Failed to update user notification setting.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserNotificationSetting(int id)
    {
        try
        {
            var existingSetting = await _userNotificationSettingRepository.GetUserNotificationSettingByIdAsync(id);
            if (existingSetting == null)
            {
                return NotFound();
            }

            var success = await _userNotificationSettingRepository.DeleteUserNotificationSettingAsync(id);
            
            if (!success)
            {
                return StatusCode(500, "Failed to delete user notification setting.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
