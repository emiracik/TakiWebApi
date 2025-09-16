using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TakiWebApi.Data;
using TakiWebApi.Models;
using TakiWebApi.Services;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly ITripRepository _tripRepository;
    private readonly IJwtService _jwtService;

    public TripsController(ITripRepository tripRepository, IJwtService jwtService)
    {
        _tripRepository = tripRepository;
        _jwtService = jwtService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trip>>> GetAllTrips()
    {
        var trips = await _tripRepository.GetAllTripsAsync();
        return Ok(trips);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Trip>> GetTrip(int id)
    {
        var trip = await _tripRepository.GetTripByIdAsync(id);
        
        if (trip == null)
        {
            return NotFound();
        }

        return Ok(trip);
    }

    [HttpGet("passenger/{passengerId}")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByPassenger(int passengerId)
    {
        var trips = await _tripRepository.GetTripsByPassengerIdAsync(passengerId);
        return Ok(trips);
    }

    [HttpGet("driver/{driverId}")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByDriver(int driverId)
    {
        var trips = await _tripRepository.GetTripsByDriverIdAsync(driverId);
        return Ok(trips);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetActiveTrips()
    {
        var trips = await _tripRepository.GetActiveTripsAsync();
        return Ok(trips);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<object>> GetTripsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var trips = await _tripRepository.GetTripsPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _tripRepository.GetTotalTripsCountAsync();
        
        var response = new
        {
            Trips = trips,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalTripsCount()
    {
        var count = await _tripRepository.GetTotalTripsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveTripsCount()
    {
        var count = await _tripRepository.GetActiveTripsCountAsync();
        return Ok(count);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var trips = await _tripRepository.GetTripsByDateRangeAsync(startDate, endDate);
        return Ok(trips);
    }

    [HttpGet("payment-method/{paymentMethod}")]
    public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByPaymentMethod(string paymentMethod)
    {
        var trips = await _tripRepository.GetTripsByPaymentMethodAsync(paymentMethod);
        return Ok(trips);
    }

    [HttpGet("cost/date-range")]
    public async Task<ActionResult<decimal>> GetTotalCostByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var totalCost = await _tripRepository.GetTotalTripCostByDateRangeAsync(startDate, endDate);
        return Ok(totalCost);
    }

    [HttpGet("passenger/{passengerId}/total-cost")]
    public async Task<ActionResult<decimal>> GetTotalTripCostByPassengerId(int passengerId)
    {
        var totalCost = await _tripRepository.GetTotalTripCostByPassengerIdAsync(passengerId);
        return Ok(totalCost);
    }
}
