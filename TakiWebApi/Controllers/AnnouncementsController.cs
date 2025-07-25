using Microsoft.AspNetCore.Mvc;
using TakiWebApi.Data;
using TakiWebApi.Models;

namespace TakiWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementRepository _announcementRepository;

    public AnnouncementsController(IAnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Announcement>>> GetAllAnnouncements()
    {
        var announcements = await _announcementRepository.GetAllAnnouncementsAsync();
        return Ok(announcements);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Announcement>> GetAnnouncement(int id)
    {
        var announcement = await _announcementRepository.GetAnnouncementByIdAsync(id);
        
        if (announcement == null)
        {
            return NotFound();
        }

        return Ok(announcement);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Announcement>>> GetActiveAnnouncements()
    {
        var announcements = await _announcementRepository.GetActiveAnnouncementsAsync();
        return Ok(announcements);
    }

    [HttpGet("paginated")]
    public async Task<ActionResult<object>> GetAnnouncementsPaginated([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            return BadRequest("Page number and page size must be greater than 0");
        }

        var announcements = await _announcementRepository.GetAnnouncementsPaginatedAsync(pageNumber, pageSize);
        var totalCount = await _announcementRepository.GetTotalAnnouncementsCountAsync();
        
        var response = new
        {
            Announcements = announcements,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetTotalAnnouncementsCount()
    {
        var count = await _announcementRepository.GetTotalAnnouncementsCountAsync();
        return Ok(count);
    }

    [HttpGet("active/count")]
    public async Task<ActionResult<int>> GetActiveAnnouncementsCount()
    {
        var count = await _announcementRepository.GetActiveAnnouncementsCountAsync();
        return Ok(count);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Announcement>>> SearchAnnouncementsByTitle([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        var announcements = await _announcementRepository.SearchAnnouncementsByTitleAsync(searchTerm);
        return Ok(announcements);
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<Announcement>>> GetAnnouncementsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be greater than end date");
        }

        var announcements = await _announcementRepository.GetAnnouncementsByDateRangeAsync(startDate, endDate);
        return Ok(announcements);
    }
}
