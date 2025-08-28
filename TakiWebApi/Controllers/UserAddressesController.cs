using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserAddressesController : ControllerBase
{
    private readonly IUserAddressRepository _userAddressRepository;

    public UserAddressesController(IUserAddressRepository userAddressRepository)
    {
        _userAddressRepository = userAddressRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserAddress>>> GetAllUserAddresses()
    {
        var userAddresses = await _userAddressRepository.GetAllUserAddressesAsync();
        return Ok(userAddresses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserAddress>> GetUserAddress(int id)
    {
        var userAddress = await _userAddressRepository.GetUserAddressByIdAsync(id);
        
        if (userAddress == null)
        {
            return NotFound();
        }

        return Ok(userAddress);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserAddress>>> GetUserAddressesByUserId(int userId)
    {
        var userAddresses = await _userAddressRepository.GetUserAddressesByUserIdAsync(userId);
        return Ok(userAddresses);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<UserAddress>>> GetActiveUserAddresses()
    {
        var userAddresses = await _userAddressRepository.GetActiveUserAddressesAsync();
        return Ok(userAddresses);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<IEnumerable<UserAddress>>> GetUserAddressesPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var userAddresses = await _userAddressRepository.GetUserAddressesPaginatedAsync(pageNumber, pageSize);
        return Ok(userAddresses);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalUserAddressesCount()
    {
        var count = await _userAddressRepository.GetTotalUserAddressesCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveUserAddressesCount()
    {
        var count = await _userAddressRepository.GetActiveUserAddressesCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<UserAddress>>> SearchUserAddressesByTitle([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty.");
        }

        var userAddresses = await _userAddressRepository.SearchUserAddressesByTitleAsync(searchTerm);
        return Ok(userAddresses);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<UserAddress>>> GetUserAddressesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date.");
        }

        var userAddresses = await _userAddressRepository.GetUserAddressesByCreatedDateRangeAsync(startDate, endDate);
        return Ok(userAddresses);
    }

    [HttpPost]
    public async Task<ActionResult<UserAddress>> CreateUserAddress([FromBody] UserAddress userAddress)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var addressId = await _userAddressRepository.CreateUserAddressAsync(userAddress);
            var createdUserAddress = await _userAddressRepository.GetUserAddressByIdAsync(addressId);
            
            return CreatedAtAction(nameof(GetUserAddress), new { id = addressId }, createdUserAddress);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserAddress(int id, [FromBody] UserAddress userAddress)
    {
        if (id != userAddress.UserAddressID)
        {
            return BadRequest("ID mismatch between route and body.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingUserAddress = await _userAddressRepository.GetUserAddressByIdAsync(id);
            if (existingUserAddress == null)
            {
                return NotFound();
            }

            userAddress.UpdatedDate = DateTime.UtcNow;
            var success = await _userAddressRepository.UpdateUserAddressAsync(userAddress);
            
            if (!success)
            {
                return StatusCode(500, "Failed to update user address.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserAddress(int id)
    {
        try
        {
            var existingUserAddress = await _userAddressRepository.GetUserAddressByIdAsync(id);
            if (existingUserAddress == null)
            {
                return NotFound();
            }

            var success = await _userAddressRepository.DeleteUserAddressAsync(id);
            
            if (!success)
            {
                return StatusCode(500, "Failed to delete user address.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
