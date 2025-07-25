using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("phone/{phoneNumber}")]
    public async Task<ActionResult<User>> GetUserByPhoneNumber(string phoneNumber)
    {
        var user = await _userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<User>> GetUserByEmail(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<User>>> GetActiveUsers()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        return Ok(users);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var users = await _userRepository.GetUsersPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _userRepository.GetTotalUsersCountAsync();
        
        var response = new
        {
            Users = users,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalUsersCount()
    {
        var count = await _userRepository.GetTotalUsersCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveUsersCount()
    {
        var count = await _userRepository.GetActiveUsersCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<User>>> SearchUsersByName([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        var users = await _userRepository.SearchUsersByNameAsync(searchTerm);
        return Ok(users);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var users = await _userRepository.GetUsersByCreatedDateRangeAsync(startDate, endDate);
        return Ok(users);
    }
}
