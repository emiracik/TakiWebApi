using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;
using TakiWebApi.Services;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IDriverRepository _driverRepository;
    private readonly IJwtService _jwtService;

    public DriversController(IDriverRepository driverRepository, IJwtService jwtService)
    {
        _driverRepository = driverRepository;
        _jwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Driver>>> GetAllDrivers()
    {
        var drivers = await _driverRepository.GetAllDriversAsync();
        return Ok(drivers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Driver>> GetDriver(int id)
    {
        var driver = await _driverRepository.GetDriverByIdAsync(id);
        
        if (driver == null)
        {
            return NotFound();
        }

        return Ok(driver);
    }

    [HttpGet("phone/{phoneNumber}")]
    public async Task<ActionResult<Driver>> GetDriverByPhoneNumber(string phoneNumber)
    {
        var driver = await _driverRepository.GetDriverByPhoneNumberAsync(phoneNumber);
        
        if (driver == null)
        {
            return NotFound();
        }

        return Ok(driver);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<Driver>> GetDriverByEmail(string email)
    {
        var driver = await _driverRepository.GetDriverByEmailAsync(email);
        
        if (driver == null)
        {
            return NotFound();
        }

        return Ok(driver);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Driver>>> GetActiveDrivers()
    {
        var drivers = await _driverRepository.GetActiveDriversAsync();
        return Ok(drivers);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<IEnumerable<Driver>>> GetDriversPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var drivers = await _driverRepository.GetDriversPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _driverRepository.GetTotalDriversCountAsync();
        
        var response = new
        {
            Drivers = drivers,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalDriversCount()
    {
        var count = await _driverRepository.GetTotalDriversCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveDriversCount()
    {
        var count = await _driverRepository.GetActiveDriversCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Driver>>> SearchDriversByName([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        var drivers = await _driverRepository.SearchDriversByNameAsync(searchTerm);
        return Ok(drivers);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Driver>>> GetDriversByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var drivers = await _driverRepository.GetDriversByCreatedDateRangeAsync(startDate, endDate);
        return Ok(drivers);
    }

    [HttpGet("vehicle-plate/{vehiclePlate}")]
    public async Task<ActionResult<IEnumerable<Driver>>> GetDriversByVehiclePlate(string vehiclePlate)
    {
        if (string.IsNullOrWhiteSpace(vehiclePlate))
        {
            return BadRequest("Vehicle plate cannot be empty");
        }

        var drivers = await _driverRepository.GetDriversByVehiclePlateAsync(vehiclePlate);
        return Ok(drivers);
    }
}
